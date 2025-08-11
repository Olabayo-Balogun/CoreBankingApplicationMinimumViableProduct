using API.Middleware;

using Application.Model;
using Application.Models;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Queries;
using Application.Models.Transactions.Response;
using Application.Utility;

using Asp.Versioning;

using Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

using ThirdPartyIntegrations.Interface;
using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request;

namespace API.Controllers
{
	[ApiVersion ("1.0")]
	[Route ("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[Authorize]
	[ProducesResponseType (StatusCodes.Status204NoContent)]
	[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status404NotFound)]
	[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType (StatusCodes.Status500InternalServerError)]
	public class TransactionsController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IPaymentIntegrationService _paymentIntegrationService;
		private readonly AppSettings _appSettings;
		public TransactionsController (IMediator mediator, IPaymentIntegrationService paymentIntegrationService, IOptions<AppSettings> appsettings)
		{
			_mediator = mediator;
			_paymentIntegrationService = paymentIntegrationService;
			_appSettings = appsettings.Value;
		}

		/// <summary>
		/// An endpoint to enable a user make a deposit, a unique GUID must be passed as the value (for each unique request) to a header key "Idempotence-Key" in the header of this request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("deposit")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[Idempotent (cacheTimeInSeconds: 60)]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status201Created)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> Deposit ([FromBody] DepositCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.Email == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (!request.Currency.Equals ("NGN", StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest ("Only 'NGN' currency is supported at the moment.");
			}

			var paymentReferenceId = Guid.NewGuid ().ToString ();
			string? checkoutUrl = null;
			if (_appSettings.DefaultPaymentService.Equals ("Paystack", StringComparison.OrdinalIgnoreCase))
			{
				PaystackPaymentCommand paystackPaymentCommand = new ()
				{
					Amount = (request.Amount * 100).ToString (),
					Email = tokenResponse.Email,
					Reference = paymentReferenceId,
					Currency = "NGN",
				};

				var paymentResult = await _paymentIntegrationService.CreatePaystackPaymentRequestAsync (paystackPaymentCommand);
				checkoutUrl = paymentResult != null && paymentResult.Data != null ? paymentResult.Data.authorization_url : null;
			}

			request.PaymentReferenceId = paymentReferenceId;
			request.CreatedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			RequestResponse<TransactionResponse> result = await _mediator.Send (request);

			if (result.Data != null)
			{
				result.Data.CheckoutUrl = checkoutUrl;
			}

			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable a user make a withdrawal, a unique GUID must be passed as the value (for each unique request) to a header key "Idempotence-Key" in the header of this request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("withdraw")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[Idempotent (cacheTimeInSeconds: 60)]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status201Created)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> Withdraw ([FromBody] WithdrawCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (!request.Currency.Equals ("NGN", StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest ("Only 'NGN' currency is supported at the moment.");
			}


			request.CreatedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			RequestResponse<TransactionResponse> result = await _mediator.Send (request);


			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable admin and staff to flag suspicious transactions
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("flag")]
		[Authorize (Roles = $"{UserRoles.Admin}, {UserRoles.Staff}")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status201Created)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> Flag ([FromBody] FlagTransactionCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			request.LastModifiedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			RequestResponse<TransactionResponse> result = await _mediator.Send (request);

			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable users confirm the success of their transaction
		/// </summary>
		/// <param name="id">The paymentReferenceId of the transaction</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("verify/{id}")]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> VerifyTransaction ([FromRoute] string id, CancellationToken cancellationToken)
		{
			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			var transaction = await _paymentIntegrationService.VerifyPaystackPaymentRequestAsync (id);
			if (transaction == null)
			{
				var badResult = RequestResponse<TransactionResponse>.Failed (null, 404, "Transaction not found");
				return StatusCode (badResult.StatusCode, badResult);
			}

			if (transaction.data == null)
			{
				var badResult = RequestResponse<TransactionResponse>.Failed (null, 404, "Transaction not found");
				return StatusCode (badResult.StatusCode, badResult);
			}

			VerifyTransactionCommand request = new ()
			{
				PaymentReferenceId = id,
				LastModifiedBy = tokenResponse.UserId,
				CancellationToken = cancellationToken,
				Amount = transaction.data.Amount / 100
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable a admin and staff request for a transaction's information as well as other analytics
		/// </summary>
		/// <param name="userId">This allows admin and staff to get the numbers transactions initiated by a specific customer as long as you specify the date/week/month/year/fromDate and toDate</param>
		/// <param name="date">Allows admin and staff to see the number of transactions initiated by a user for a specific date</param>
		/// <param name="week">Allows admin and staff to see the number of transactions initiated by a user for a specific week</param>
		/// <param name="month">Allows admin and staff to see the number of transactions initiated by a user for a specific month</param>
		/// <param name="publicId">This allows admin and staff to search for a transaction using its unique ID on the database</param>
		/// <param name="year">Allows admin and staff to see the number of transactions initiated by a user for a specific year</param>
		/// <param name="fromDate">Allows admin and staff to see the number of transactions initiated by a user from a specific date</param>
		/// <param name="toDate">Allows admin and staff to see the number of transactions initiated by a user to a specific date</param>
		/// <param name="accountNumber">This allows admin and staff to search for an account using its account number</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("transaction")]
		[Authorize (Roles = $"{UserRoles.Admin}, {UserRoles.Staff}")]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> GetTransaction ([FromQuery] string? publicId, DateTime? date, DateTime? week, DateTime? month, DateTime? year, DateTime? fromDate, DateTime? toDate, string? accountNumber, string? userId, CancellationToken cancellationToken)
		{
			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			TransactionQuery request = new ()
			{
				PublicId = publicId,
				Date = date,
				AccountNumber = accountNumber,
				UserId = userId,
				FromDate = fromDate,
				ToDate = toDate,
				Week = week,
				Month = month,
				Year = year,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable a admin and staff request for a transaction's information as well as other analytics
		/// </summary>
		/// <param name="userId">This allows user to search for all transactions he/she initiated, this is retrieved from the user's token, only staff and admin can pass in different userIds</param>
		/// <param name="date">Allows user retrieve all transactions he/she initiated on a specific date</param>
		/// <param name="week">Allows user retrieve all transactions he/she initiated for a specific week</param>
		/// <param name="month">Allows user retrieve all transactions he/she initiated for a specific month</param>
		/// <param name="amount">This allows a user to search for transactions by a specific amount</param>
		/// <param name="year">Allows user retrieve all transactions he/she initiated for a specific year</param>
		/// <param name="fromDate">Allows user retrieve all transactions he/she initiated from a specific date</param>
		/// <param name="toDate">Allows user retrieve all transactions he/she initiated to a specific date</param>
		/// <param name="accountNumber">This allows admin and staff to search for transactions initiated using a specific account number</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("transactions")]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<TransactionResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<TransactionResponse>>> GetTransactions ([FromQuery] decimal? amount, DateTime? date, DateTime? week, DateTime? month, DateTime? year, DateTime? fromDate, DateTime? toDate, string? accountNumber, string? userId, CancellationToken cancellationToken)
		{
			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserRole == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserRole.Equals (UserRoles.User, StringComparison.OrdinalIgnoreCase) && accountNumber != null)
			{
				return BadRequest ("Only admin and staff can search by account number");
			}

			if (tokenResponse.UserRole.Equals (UserRoles.User, StringComparison.OrdinalIgnoreCase))
			{
				userId = tokenResponse.UserId;
			}

			TransactionsQuery request = new ()
			{
				Amount = amount,
				Date = date,
				AccountNumber = accountNumber,
				UserId = userId,
				FromDate = fromDate,
				ToDate = toDate,
				Week = week,
				Month = month,
				Year = year,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}
	}
}

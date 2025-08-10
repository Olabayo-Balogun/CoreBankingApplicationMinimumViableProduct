using API.Middleware;

using Application.Model;
using Application.Models.Accounts.Command;
using Application.Models.Accounts.Queries;
using Application.Models.Accounts.Response;
using Application.Utility;

using Asp.Versioning;

using Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
	[ApiVersion ("1.0")]
	[Route ("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[Authorize]
	[ProducesResponseType (StatusCodes.Status204NoContent)]
	[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status404NotFound)]
	[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType (StatusCodes.Status500InternalServerError)]
	public class AccountsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public AccountsController (IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// An endpoint to enable a user request for an account opening, a unique GUID must be passed as the value (for each unique request) to a header key "Idempotence-Key" in the header of this request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("account")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[Idempotent (cacheTimeInSeconds: 60)]
		[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status201Created)]
		public async Task<ActionResult<RequestResponse<AccountResponse>>> CreateAccount ([FromBody] CreateAccountCommand request, CancellationToken cancellationToken)
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

			request.CreatedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable a admin and staff request for an account's information
		/// </summary>
		/// <param name="accountLedger">This allows admin and staff to search for an account using the account's ledger</param>
		/// <param name="isCount">If set to true, staff and admin can see analytics on the number of acccounts that have been created, if userPublicId is not null, the number of accounts created for a specific customer will be returned</param>
		/// <param name="userPublicId">This allows admin and staff to get the numbers accounts created for a specific customer as long isCount is set to true</param>
		/// <param name="publicId">This allows admin and staff to search for an account using its unique ID on the database</param>
		/// <param name="accountNumber">This allows admin and staff to search for an account using its account number</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("account")]
		[Authorize (Roles = $"{UserRoles.Admin}, {UserRoles.Staff}")]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[Idempotent (cacheTimeInSeconds: 60)]
		[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<AccountResponse>>> GetAccount ([FromQuery] string? publicId, string? accountLedger, string? accountNumber, string? userPublicId, CancellationToken cancellationToken, bool isCount = false)
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

			AccountQuery request = new ()
			{
				PublicId = publicId,
				AccountLedger = accountLedger,
				AccountNumber = accountNumber,
				UserPublicId = userPublicId,
				IsCount = isCount,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable a logged in user request for all accounts belonging to a user. Admin and staff can also use this endpoint to spool all accounts that belong to a user.
		/// </summary>
		/// <param name="id">The userId of the user whose accounts are being searched for</param>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("accounts/{id}")]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<List<AccountResponse>>), StatusCodes.Status200OK)]
		[ProducesResponseType (type: typeof (RequestResponse<List<AccountResponse>>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType (type: typeof (RequestResponse<List<AccountResponse>>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType (type: typeof (RequestResponse<List<AccountResponse>>), StatusCodes.Status404NotFound)]
		[ProducesResponseType (type: typeof (RequestResponse<List<AccountResponse>>), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<RequestResponse<AccountResponse>>> GetAccounts ([FromRoute] string? id, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
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

			if (!tokenResponse.UserId.Equals (id, StringComparison.OrdinalIgnoreCase) && !tokenResponse.UserRole.Equals (UserRoles.Staff, StringComparison.OrdinalIgnoreCase) && !tokenResponse.UserRole.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
			{
				return Unauthorized ("You are unauthorized to make this request");
			}

			AccountsQuery request = new ()
			{
				PublicId = tokenResponse.UserId,
				PageNumber = pageNumber,
				PageSize = pageSize,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable admin update some crucial parts of an existing account
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("account")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<AccountResponse>>> UpdateAccount ([FromBody] UpdateAccountCommand request, CancellationToken cancellationToken)
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
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// An endpoint to enable admin delete an existing account
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete ("account")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<AccountResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<AccountResponse>>> DeleteAccount ([FromBody] DeleteAccountCommand request, CancellationToken cancellationToken)
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

			request.DeletedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}
	}
}

using Application.Interface.Persistence;
using Application.Model;
using Application.Model.PaymentIntegration.Command;
using Application.Models;
using Application.Models.Transactions.Command;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Persistence;

using RestSharp;

using ThirdPartyIntegrations.Interface;
using ThirdPartyIntegrations.Models.PaymentIntegration.Paystack.Response;
using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request;
using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Response;

namespace ThirdPartyIntegrations.Services
{
	public class PaymentIntegrationService : IPaymentIntegrationService
	{
		private readonly AppSettings _appSettings;
		private readonly ApplicationDbContext _context;
		private readonly ILogger<PaymentIntegrationService> _logger;
		private readonly ITransactionRepository _transactionRepository;
		private readonly IMapper _mapper;

		public PaymentIntegrationService (IOptions<AppSettings> appSettings, ApplicationDbContext context, ILogger<PaymentIntegrationService> logger, ITransactionRepository transactionRepository, IMapper mapper)
		{
			_appSettings = appSettings.Value;
			_context = context;
			_logger = logger;
			_transactionRepository = transactionRepository;
			_mapper = mapper;
		}

		public async Task<PaystackPaymentResponse> CreatePaystackPaymentRequestAsync (PaystackPaymentCommand request)
		{
			try
			{
				_logger.LogInformation ($"CreatePaystackPaymentRequest for entity with email: {request.Email} and amount: {request.Amount} begins at {DateTime.UtcNow.AddHours (1)}");

				var baseUrl = _appSettings.PaystackBaseUrl;
				var endpoint = _appSettings.PaystackTransferEndpoint;

				string secretKey = _appSettings.IsPaystackProductionEnvironment ? _appSettings.PaystackProductionSecretKey : _appSettings.PaystackTestSecretKey;
				var client = new RestClient (baseUrl);

				var restRequest = new RestRequest (endpoint, Method.Post);
				restRequest.AddJsonBody (request);
				restRequest.AddHeader ("Authorization", $"Bearer {secretKey}");
				restRequest.AddHeader ("Content-Type", "application/json");

				var response = await client.ExecuteAsync (restRequest);

				var result = response.Content != null ? JsonConvert.DeserializeObject<PaystackPaymentResponse> (response.Content) : new PaystackPaymentResponse ();

				_logger.LogInformation ($"CreatePaystackPaymentRequest for entity with email: {request.Email} and amount: {request.Amount} ends at {DateTime.UtcNow.AddHours (1)}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreatePaystackPaymentRequest for entity with email: {request.Email} and amount: {request.Amount} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<PaystackPaymentVerificationResponse> VerifyPaystackPaymentRequestAsync (string paymentReferenceId)
		{
			try
			{
				_logger.LogInformation ($"VerifyPaystackPaymentRequest begins at {DateTime.UtcNow.AddHours (1)} for payment with reference Id: {paymentReferenceId}");

				var baseUrl = _appSettings.PaystackBaseUrl;
				var endpoint = _appSettings.PaystackVerificationEndpoint.Replace ("{paymentReferenceId}", paymentReferenceId);

				string secretKey = _appSettings.IsPaystackProductionEnvironment ? _appSettings.PaystackProductionSecretKey : _appSettings.PaystackTestSecretKey;
				var client = new RestClient (baseUrl);

				var restRequest = new RestRequest (endpoint, Method.Get);
				restRequest.AddHeader ("Authorization", $"Bearer {secretKey}");

				var response = await client.ExecuteAsync (restRequest);

				var result = response.Content != null ? JsonConvert.DeserializeObject<PaystackPaymentVerificationResponse> (response.Content) : new PaystackPaymentVerificationResponse ();

				_logger.LogInformation ($"VerifyPaystackPaymentRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with reference Id: {paymentReferenceId}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"VerifyPaystackPaymentRequest exception occurred at {DateTime.UtcNow.AddHours (1)} for payment with reference Id: {paymentReferenceId} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<PaymentIntegrationResponse>> PaystackPaymentWebhookRequestAsync (PaystackWebhookCommand request)
		{
			try
			{
				_logger.LogInformation ($"PaystackPaymentWebhookRequest begins at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount}");

				var check = await _context.Transactions.Where (x => x.PaymentReferenceId == request.Data.offline_reference).FirstOrDefaultAsync ();
				if (check == null)
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (check.IsReconciled == true)
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 200, "Payment already confirmed");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;
				}

				PaystackPaymentVerificationResponse paymentVerification = await VerifyPaystackPaymentRequestAsync (request.Data.offline_reference);

				if (paymentVerification.data == null)
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!paymentVerification.data.Status.Equals ("success", StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 400, "Payment not confirmed");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!request.Data.status.Equals ("success", StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 400, "Payment not confirmed");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var reconcileTransaction = new ConfirmTransactionCommand
				{
					Amount = paymentVerification.data.Amount / 100,
					PaymentReferenceId = request.Data.offline_reference,
					LastModifiedBy = "Paystack",
					CancellationToken = CancellationToken.None,
				};

				var reconcileTransactionRequest = await _transactionRepository.ConfirmTransactionAsync (reconcileTransaction);

				if (reconcileTransactionRequest.Data == null)
				{
					var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 400, "Payment not confirmed");
					_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {badRequest.Remark}");
					return badRequest;

				}

				var response = new PaymentIntegrationResponse
				{
					Channel = reconcileTransactionRequest.Data.Channel,
					PaymentReferenceId = reconcileTransactionRequest.Data.PaymentReferenceId,
					PaymentService = reconcileTransactionRequest.Data.PaymentService,
					Amount = reconcileTransactionRequest.Data.Amount
				};

				var result = RequestResponse<PaymentIntegrationResponse>.Success (response, 1, "Payment request confirmed sucessfully");
				_logger.LogInformation ($"PaystackPaymentWebhookRequest ends at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"PaystackPaymentWebhookRequest exception occurred at {DateTime.UtcNow.AddHours (1)} for payment with PaymentReferenceId: {request.Data.offline_reference} and amount: {request.Data.Amount} with message: {ex.Message}");
				throw;
			}
		}
	}
}
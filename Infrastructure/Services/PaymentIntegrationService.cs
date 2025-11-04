using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.Banks.Response;
using Application.Models.PaymentIntegration.Paystack.Command;
using Application.Models.PaymentIntegration.Paystack.Response;
using Application.Models.Transactions.Command;
using Application.Utility;

using AutoMapper.Internal;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Persistence;

using RestSharp;

namespace Infrastructure.Services
{
    public class PaymentIntegrationService : IPaymentIntegrationService
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentIntegrationService> _logger;
        private readonly ITransactionRepository _transactionRepository;

        public PaymentIntegrationService (IOptions<AppSettings> appSettings, ApplicationDbContext context, ILogger<PaymentIntegrationService> logger, ITransactionRepository transactionRepository)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _logger = logger;
            _transactionRepository = transactionRepository;
        }

        public async Task<PaystackPaymentResponse> CreatePaystackPaymentRequestAsync (PaystackPaymentCommand request)
        {
            try
            {
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreatePaystackPaymentRequestAsync), nameof (request.Email), request.Email, nameof (request.Amount), request.Amount);
				_logger.LogInformation (openingLog);

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

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreatePaystackPaymentRequestAsync), nameof (request.Email), request.Email, nameof (request.Amount), request.Amount, response.StatusCode.ToString());
				_logger.LogInformation (closingLog);
				return result;
            }
            catch (Exception ex)
            {
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreatePaystackPaymentRequestAsync), nameof (request.Email), request.Email, nameof (request.Amount), request.Amount, ex.Message);
				_logger.LogError (errorLog);
				return new PaystackPaymentResponse { Message = ex.Message, Status = false, Data = null };
            }
        }

        public async Task<PaystackPaymentVerificationResponse> VerifyPaystackPaymentRequestAsync (string paymentReferenceId)
        {
            try
            {
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (VerifyPaystackPaymentRequestAsync), nameof (paymentReferenceId), paymentReferenceId);
				_logger.LogInformation (openingLog);

                var baseUrl = _appSettings.PaystackBaseUrl;
                var endpoint = _appSettings.PaystackVerificationEndpoint.Replace ("{paymentReferenceId}", paymentReferenceId);

                string secretKey = _appSettings.IsPaystackProductionEnvironment ? _appSettings.PaystackProductionSecretKey : _appSettings.PaystackTestSecretKey;
                var client = new RestClient (baseUrl);

                var restRequest = new RestRequest (endpoint, Method.Get);
                restRequest.AddHeader ("Authorization", $"Bearer {secretKey}");

                var response = await client.ExecuteAsync (restRequest);

                var result = response.Content != null ? JsonConvert.DeserializeObject<PaystackPaymentVerificationResponse> (response.Content) : new PaystackPaymentVerificationResponse ();

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (VerifyPaystackPaymentRequestAsync), nameof (paymentReferenceId), paymentReferenceId, response.StatusCode.ToString());
				_logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (VerifyPaystackPaymentRequestAsync), nameof (paymentReferenceId), paymentReferenceId, ex.Message);
				_logger.LogError (errorLog);
				return new PaystackPaymentVerificationResponse { Message = ex.Message, Status = false, data = null };
			}
        }

        public async Task<RequestResponse<PaymentIntegrationResponse>> PaystackPaymentWebhookRequestAsync (PaystackWebhookCommand request)
        {
            try
            {
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof(request.Data.Amount), request.Data.Amount.ToString());
				_logger.LogInformation (openingLog);

                var check = await _context.Transactions.Where (x => x.PaymentReferenceId == request.Data.offline_reference).FirstOrDefaultAsync ();
                if (check == null)
                {
                    var badRequest = RequestResponse<PaymentIntegrationResponse>.NotFound (null, "Payment");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString(), badRequest.Remark);
					_logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (check.IsReconciled == true)
                {
                    var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 200, "Payment already confirmed");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
                }

                PaystackPaymentVerificationResponse paymentVerification = await VerifyPaystackPaymentRequestAsync (request.Data.offline_reference);

                if (paymentVerification.data == null)
                {
                    var badRequest = RequestResponse<PaymentIntegrationResponse>.NotFound (null, "Payment");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
                }

                if (!paymentVerification.data.Status.Equals ("success", StringComparison.OrdinalIgnoreCase))
                {
                    var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 400, "Payment not confirmed");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
                }

                if (!request.Data.status.Equals ("success", StringComparison.OrdinalIgnoreCase))
                {
                    var badRequest = RequestResponse<PaymentIntegrationResponse>.Failed (null, 400, "Payment not confirmed");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

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

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

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

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
            }
            catch (Exception ex)
            {
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (PaystackPaymentWebhookRequestAsync), nameof (request.Data.offline_reference), request.Data.offline_reference, nameof (request.Data.Amount), request.Data.Amount.ToString (), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<PaymentIntegrationResponse>.Error (null);
			}
        }
    }
}
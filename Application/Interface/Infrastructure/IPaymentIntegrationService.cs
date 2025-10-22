using Application.Models;
using Application.Models.PaymentIntegration.Paystack.Command;
using Application.Models.PaymentIntegration.Paystack.Response;

namespace Application.Interface.Infrastructure
{
	public interface IPaymentIntegrationService
	{
		Task<PaystackPaymentResponse> CreatePaystackPaymentRequestAsync (PaystackPaymentCommand request);
		Task<RequestResponse<PaymentIntegrationResponse>> PaystackPaymentWebhookRequestAsync (PaystackWebhookCommand request);
		Task<PaystackPaymentVerificationResponse> VerifyPaystackPaymentRequestAsync (string paymentReferenceId);
	}
}

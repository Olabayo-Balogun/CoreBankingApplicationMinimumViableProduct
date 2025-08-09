using Application.Model;
using Application.Model.PaymentIntegration.Command;

using ThirdPartyIntegrations.Models.PaymentIntegration.Paystack.Response;
using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request;
using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Response;

namespace ThirdPartyIntegrations.Interface
{
	public interface IPaymentIntegrationService
	{
		Task<PaystackPaymentResponse> CreatePaystackPaymentRequestAsync (PaystackPaymentCommand request);
		Task<RequestResponse<PaymentIntegrationResponse>> PaystackPaymentWebhookRequestAsync (PaystackWebhookCommand request);
		Task<PaystackPaymentVerificationResponse> VerifyPaystackPaymentRequestAsync (string paymentReferenceId);
	}
}

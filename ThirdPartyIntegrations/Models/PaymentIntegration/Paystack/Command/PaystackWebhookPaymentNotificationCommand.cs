namespace ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request
{
	public class PaystackWebhookPaymentNotificationCommand
	{
		public DateTime sent_at { get; set; }
		public string channel { get; set; }
	}
}

//    "notifications": [
//	  {
//		"sent_at": "2019-06-21T15:25:42.452Z",
//        "channel": "email"
//	  }
//    ],
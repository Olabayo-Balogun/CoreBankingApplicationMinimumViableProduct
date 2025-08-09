namespace ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Response
{
	public class PaystackPaymentVerificationCustomerResponse
	{
		public int id { get; set; }
		public string? first_name { get; set; }
		public string? last_name { get; set; }
		public string? email { get; set; }
		public string? customer_code { get; set; }
		public string? phone { get; set; }
		public string? metadata { get; set; }
		public string? risk_action { get; set; }
		public string? international_format_phone { get; set; }
	}
}

//"customer": {
//	"id": 181873746,
//      "first_name": null,
//      "last_name": null,
//      "email": "demo@test.com",
//      "customer_code": "CUS_1rkzaqsv4rrhqo6",
//      "phone": null,
//      "metadata": null,
//      "risk_action": "default",
//      "international_format_phone": null



//	},
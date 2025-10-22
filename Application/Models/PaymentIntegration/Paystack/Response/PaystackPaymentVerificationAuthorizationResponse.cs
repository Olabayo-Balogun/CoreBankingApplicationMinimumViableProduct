namespace Application.Models.PaymentIntegration.Paystack.Response
{
	public class PaystackPaymentVerificationAuthorizationResponse
	{
		public string authorization_code { get; set; }
		public string bin { get; set; }
		public string last4 { get; set; }
		public string exp_month { get; set; }
		public string exp_year { get; set; }
		public string channel { get; set; }
		public string card_type { get; set; }
		public string bank { get; set; }
		public string country_code { get; set; }
		public string brand { get; set; }
		public string reusable { get; set; }
		public string signature { get; set; }
		public string? account_name { get; set; }
	}
}


//"authorization": {
//	"authorization_code": "AUTH_uh8bcl3zbn",
//      "bin": "408408",
//      "last4": "4081",
//      "exp_month": "12",
//      "exp_year": "2030",
//      "channel": "card",
//      "card_type": "visa ",
//      "bank": "TEST BANK",
//      "country_code": "NG",
//      "brand": "visa",
//      "reusable": true,
//      "signature": "SIG_yEXu7dLBeqG0kU7g95Ke",
//      "account_name": null



//	},
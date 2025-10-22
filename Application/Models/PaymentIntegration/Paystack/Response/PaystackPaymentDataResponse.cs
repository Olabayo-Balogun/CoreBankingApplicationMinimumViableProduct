namespace Application.Models.PaymentIntegration.Paystack.Response
{
	public class PaystackPaymentDataResponse
	{
		public string authorization_url { get; set; }
		public string access_code { get; set; }
		public string reference { get; set; }
	}
}

//"data": {
//	"authorization_url": "https://checkout.paystack.com/3ni8kdavz62431k",
//    "access_code": "3ni8kdavz62431k",
//    "reference": "re4lyvq3s3"
//  }
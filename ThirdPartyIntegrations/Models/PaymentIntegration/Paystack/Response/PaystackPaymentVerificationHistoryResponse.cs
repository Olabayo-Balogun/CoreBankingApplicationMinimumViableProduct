namespace ThirdPartyIntegrations.Models.PaymentIntegration.Paystack.Response
{
	public class PaystackPaymentVerificationHistoryResponse
	{
		public string type { get; set; }
		public string message { get; set; }
		public int time { get; set; }
	}
}


//"history": [
//		{
//		"type": "action",
//          "message": "Attempted to pay with card",
//          "time": 3



//		},
//        {
//	"type": "success",
//          "message": "Successfully paid with card",
//          "time": 4



//		}
//      ]
//    },
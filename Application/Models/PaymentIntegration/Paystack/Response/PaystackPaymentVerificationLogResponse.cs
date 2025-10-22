namespace Application.Models.PaymentIntegration.Paystack.Response
{
	public class PaystackPaymentVerificationLogResponse
	{
		public int start_time { get; set; }
		public int time_spent { get; set; }

		public int attempts { get; set; }
		public int errors { get; set; }
		public bool success { get; set; }
		public bool mobile { get; set; }
		public List<string> input { get; set; }
		public List<PaystackPaymentVerificationHistoryResponse> history { get; set; }
	}
}

//"log": {
//	"start_time": 1724318098,
//      "time_spent": 4,
//      "attempts": 1,
//      "errors": 0,
//      "success": true,
//      "mobile": false,
//      "input": [],
//      "history": [
//		{
//		"type": "action",
//          "message": "Attempted to pay with card",
//          "time": 3



//		},
//        {
//		"type": "success",
//          "message": "Successfully paid with card",
//          "time": 4



//		}
//      ]
//    },
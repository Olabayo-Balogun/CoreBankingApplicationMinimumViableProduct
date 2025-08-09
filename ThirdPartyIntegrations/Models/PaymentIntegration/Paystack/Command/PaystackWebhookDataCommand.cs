namespace ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request
{
	public class PaystackWebhookDataCommand
	{
		public long id { get; set; }
		public string domain { get; set; }
		public int Amount { get; set; }
		public string currency { get; set; }
		public DateTime? due_date { get; set; }
		public bool has_invoice { get; set; }
		public string? invoice_number { get; set; }
		public string description { get; set; }
		public string? pdf_url { get; set; }
		public List<string> line_items { get; set; }
		public List<string> tax { get; set; }
		public string request_code { get; set; }
		public string status { get; set; }
		public string paid { get; set; }
		public DateTime paid_at { get; set; }
		public string? metadata { get; set; }
		public List<PaystackWebhookPaymentNotificationCommand>? notifications { get; set; }

		public string offline_reference { get; set; }
		public int customer { get; set; }
		public DateTime created_at { get; set; }
	}
}

//"data": {
//	"id": 1089700,
//    "domain": "test",
//    "amount": 10000000,
//    "currency": "NGN",
//    "due_date": null,
//    "has_invoice": false,
//    "invoice_number": null,
//    "description": "Pay up now",
//    "pdf_url": null,
//    "line_items": [],
//    "tax": [],
//    "request_code": "PRQ_y0paeo93jh99mho",
//    "status": "success",
//    "paid": true,
//    "paid_at": "2019-06-21T15:26:10.000Z",
//    "metadata": null,
//    "notifications": [



//	  {
//		"sent_at": "2019-06-21T15:25:42.452Z",
//        "channel": "email"



//	  }
//    ],
//    "offline_reference": "3365451089700",
//    "customer": 7454223,
//    "created_at": "2019-06-21T15:25:42.000Z"
//  }
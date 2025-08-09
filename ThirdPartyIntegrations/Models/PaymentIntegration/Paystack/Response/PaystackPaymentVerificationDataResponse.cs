using System.Text.Json.Serialization;

namespace ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Response
{
	public class PaystackPaymentVerificationDataResponse
	{
		[JsonPropertyName ("id")]
		public long Id { get; set; }

		[JsonPropertyName ("domain")]
		public string? Domain { get; set; }

		[JsonPropertyName ("status")]
		public string Status { get; set; }

		[JsonPropertyName ("reference")]
		public string Reference { get; set; }

		[JsonPropertyName ("receipt_number")]
		public object? ReceiptNumber { get; set; }

		[JsonPropertyName ("amount")]
		public int Amount { get; set; }

		[JsonPropertyName ("message")]
		public string Message { get; set; }

		[JsonPropertyName ("gateway_response")]
		public string? GatewayResponse { get; set; }

		[JsonPropertyName ("paid_at")]
		public DateTime? PaidAt { get; set; }

		[JsonPropertyName ("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonPropertyName ("channel")]
		public string Channel { get; set; }

		[JsonPropertyName ("currency")]
		public string Currency { get; set; }

		[JsonPropertyName ("ip_address")]
		public string IpAddress { get; set; }

		[JsonPropertyName ("metadata")]
		public string? Metadata { get; set; }

		[JsonPropertyName ("log")]
		public PaystackPaymentVerificationLogResponse? Log { get; set; }

		[JsonPropertyName ("fees")]
		public object? Fees { get; set; }

		[JsonPropertyName ("fees_split")]
		public object? FeesSplit { get; set; }

		[JsonPropertyName ("authorization")]
		public PaystackPaymentVerificationAuthorizationResponse? Authorization { get; set; }
		[JsonPropertyName ("customer")]
		public PaystackPaymentVerificationCustomerResponse? Customer { get; set; }
		public object? Plan { get; set; }
		public object? Split { get; set; }
		public object? OrderId { get; set; }
		public int? RequestedAmount { get; set; }
		public object? PosTransactionData { get; set; }
		public object? Source { get; set; }
		public object? FeesBreakdown { get; set; }
		public object? Connect { get; set; }
		public DateTime? TransactionDate { get; set; }
		public object? PlanObject { get; set; }
		public object? Subaccount { get; set; }
	}
}

//"data": {
//	"id": 4099260516,
//    "domain": "test",
//    "status": "success",
//    "reference": "re4lyvq3s3",
//    "receipt_number": null,
//    "amount": 40333,
//    "message": null,
//    "gateway_response": "Successful",
//    "paid_at": "2024-08-22T09:15:02.000Z",
//    "created_at": "2024-08-22T09:14:24.000Z",
//    "channel": "card",
//    "currency": "NGN",
//    "ip_address": "197.210.54.33",
//    "metadata": "",
//    "log": {
//		"start_time": 1724318098,
//      "time_spent": 4,
//      "attempts": 1,
//      "errors": 0,
//      "success": true,
//      "mobile": false,
//      "input": [],
//      "history": [
//		{
//			"type": "action",
//          "message": "Attempted to pay with card",
//          "time": 3



//		},
//        {
//			"type": "success",
//          "message": "Successfully paid with card",
//          "time": 4



//		}
//      ]
//    },
//    "fees": 10283,
//    "fees_split": null,
//    "authorization": {
//		"authorization_code": "AUTH_uh8bcl3zbn",
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
//    "customer": {
//		"id": 181873746,
//      "first_name": null,
//      "last_name": null,
//      "email": "demo@test.com",
//      "customer_code": "CUS_1rkzaqsv4rrhqo6",
//      "phone": null,
//      "metadata": null,
//      "risk_action": "default",
//      "international_format_phone": null



//	},
//    "plan": null,
//    "split": { },
//    "order_id": null,
//    "paidAt": "2024-08-22T09:15:02.000Z",
//    "createdAt": "2024-08-22T09:14:24.000Z",
//    "requested_amount": 30050,
//    "pos_transaction_data": null,
//    "source": null,
//    "fees_breakdown": null,
//    "connect": null,
//    "transaction_date": "2024-08-22T09:14:24.000Z",
//    "plan_object": { },
//    "subaccount": { }
//}
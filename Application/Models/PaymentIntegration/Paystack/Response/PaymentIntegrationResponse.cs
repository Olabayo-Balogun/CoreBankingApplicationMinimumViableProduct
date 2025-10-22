using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Application.Models.PaymentIntegration.Paystack.Response
{
	public class PaymentIntegrationResponse
	{
		/// <summary>
		/// The amount paid
		/// </summary>
		[Required (ErrorMessage = "Amount is required")]
		[Precision (18, 2)]
		public decimal Amount { get; set; }
		/// <summary>
		/// The payment platform used eg flutterwave, paystack, etc.
		/// </summary>
		[Required (ErrorMessage = "Channel is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Channel { get; set; }
		/// <summary>
		/// The name of the product being paid for eg Card Payments, Bank Account Payments, Bank Transfer (NGN) etc.
		/// </summary>
		[Required (ErrorMessage = "PaymentService is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PaymentService { get; set; }
		/// <summary>
		/// The payment reference Id gotten from the payment channel.
		/// </summary>
		[Required (ErrorMessage = "Payment reference unique ID is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PaymentReferenceId { get; set; }
	}
}

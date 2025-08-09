using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Application.Model.Payments.Command
{
	public class CreatePaymentCommand
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
		/// The type of payment, maybe for doing a search, or for email reminder to debtors, or for advance payment, etc.
		/// </summary>
		[Required (ErrorMessage = "Payment type is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string ProductName { get; set; }
		/// <summary>
		/// The publicUserId of the user making the payment
		/// </summary>
		[Required (ErrorMessage = "Guid publicId of the paying user is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string CreatedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }

	}
}
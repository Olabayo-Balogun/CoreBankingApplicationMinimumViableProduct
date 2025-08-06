using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
	public class Payment : AuditableEntity
	{
		/// <summary>
		/// Unique GUID converted to string that can be used to access the payment information
		/// </summary>
		[Required (ErrorMessage = "Payment Guid publicId is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }
		/// <summary>
		/// The amount paid
		/// </summary>
		[Required (ErrorMessage = "Amount is required")]
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		[Precision (18, 2)]
		public decimal Amount { get; set; }
		/// <summary>
		/// The name of the mode used eg Card Payments, Bank Account Payments, Bank Transfer etc.
		/// </summary>
		[Required (ErrorMessage = "Channel is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Channel { get; set; }
		/// <summary>
		/// The payment platform used eg flutterwave, paystack, etc.
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
		[Required (ErrorMessage = "IsConfirmed is required")]
		public bool IsConfirmed { get; set; }
		[Required (ErrorMessage = "Payment Currency is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Currency { get; set; }
	}
}
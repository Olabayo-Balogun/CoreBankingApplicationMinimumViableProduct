using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Domain.DTO
{
	public class TransactionDto : AuditableEntityDto
	{
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? PublicId { get; set; } = string.Empty;
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Description { get; set; } = string.Empty;
		[Precision (18, 2)]
		public decimal Amount { get; set; }
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string SenderAccountNumber { get; set; } = string.Empty;
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string SenderAccountName { get; set; } = string.Empty;
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string SenderBankName { get; set; } = string.Empty;
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string RecipientAccountNumber { get; set; } = string.Empty;
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string RecipientAccountName { get; set; } = string.Empty;
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string RecipientBankName { get; set; } = string.Empty;
		public bool IsReconciled { get; set; } = false;
		public bool IsFlagged { get; set; } = false;
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Notes { get; set; }
		/// <summary>
		/// Credit or debit
		/// </summary>
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string TransactionType { get; set; } = string.Empty;
		[Required (ErrorMessage = "Transaction Currency is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Currency { get; set; }
		[Required (ErrorMessage = "Payment Reference ID is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PaymentReferenceId { get; set; }
		/// <summary>
		/// The payment platform used eg flutterwave, paystack, etc.
		/// </summary>
		public string PaymentService { get; set; }
		/// <summary>
		/// The name of the mode used eg Card Payments, Bank Account Payments, Bank Transfer etc.
		/// </summary>
		public string Channel { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

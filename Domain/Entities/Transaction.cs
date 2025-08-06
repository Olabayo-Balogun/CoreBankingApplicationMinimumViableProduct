using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
	public class Transaction : AuditableEntity
	{
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string TransactionId { get; set; } = string.Empty;
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Description { get; set; } = string.Empty;
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		[Precision (18, 2)]
		public decimal Amount { get; set; }
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string TransferFrom { get; set; } = string.Empty;
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
	}
}

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Transaction : AuditableEntity
    {
        [Required]
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string PublicId { get; set; } = string.Empty;
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Description { get; set; } = string.Empty;
        [Precision (18, 2)]
        [Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
        public decimal Amount { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? SenderAccountNumber { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? SenderAccountName { get; set; }
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? SenderBankName { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? RecipientAccountNumber { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? RecipientAccountName { get; set; }
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? RecipientBankName { get; set; }
        public bool IsReconciled { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Notes { get; set; }
        /// <summary>
        /// Credit or debit
        /// </summary>
        [Required]
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string TransactionType { get; set; }
        [Required (ErrorMessage = "Transaction Currency is required")]
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Currency { get; set; }
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? PaymentReferenceId { get; set; }
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
    }
}

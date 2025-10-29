using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.PaymentIntegration.Paystack.Command
{
    public class CreatePaymentIntegrationCommand
    {
        /// <summary>
        /// The name of the product being paid for eg Card Payments, Bank Account Payments, Bank Transfer (NGN) etc.
        /// </summary>
        [Required (ErrorMessage = "Channel is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Channel { get; set; }
        /// <summary>
        /// The amount paid
        /// </summary>
        [Required (ErrorMessage = "Amount is required")]
        [Precision (18, 2)]
        public decimal Amount { get; set; }
        public int? Cvv { get; set; }
        public string? PAN { get; set; }
        public string? CustomerName { get; set; }

        /// <summary>
        /// The payment platform used eg flutterwave, paystack, etc.
        /// </summary>
        [Required (ErrorMessage = "PaymentService is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string PaymentService { get; set; }
        /// <summary>
        /// The publicUserId of the user making the payment
        /// </summary>
        [Required (ErrorMessage = "publicId of the paying user is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string CreatedBy { get; set; }
        [Required (ErrorMessage = "Payment type is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string PaymentType { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? BankName { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? CardIssuer { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? CardType { get; set; }
    }
}

namespace Application.Models.PaymentIntegration.Paystack.Command
{
    public class PaystackPaymentCommand
    {
        public string Currency { get; set; }
        public string Email { get; set; }
        public string Amount { get; set; }
        public string Reference { get; set; }
    }
}

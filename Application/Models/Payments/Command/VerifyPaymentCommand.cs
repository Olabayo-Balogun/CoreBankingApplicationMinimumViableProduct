namespace Application.Models.Payments.Command
{
	public class VerifyPaymentCommand
	{
		public string PublicUserId { get; set; }
		public string PaymentReferenceNumber { get; set; }
		public string? Channel { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
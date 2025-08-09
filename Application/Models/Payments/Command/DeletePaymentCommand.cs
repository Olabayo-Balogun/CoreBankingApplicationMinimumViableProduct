namespace Application.Model.Payments.Command
{
	public class DeletePaymentCommand
	{
		public string PublicId { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

namespace Application.Models.Transactions.Command
{
	public class ConfirmTransactionCommand
	{
		public string PaymentReferenceId { get; set; }
		public decimal Amount { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

namespace Application.Model.Transactions.Command
{
	public class DeleteTransactionCommand
	{
		public string PublicId { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
namespace Application.Model.Transactions.Queries
{
	public class GetTransactionByEmailQuery
	{
		public string Email { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
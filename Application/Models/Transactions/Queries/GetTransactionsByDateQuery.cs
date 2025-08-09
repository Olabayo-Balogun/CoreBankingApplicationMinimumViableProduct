namespace Application.Model.Transactions.Queries
{
	public class GetTransactionsByDateQuery
	{
		public DateTime Date { get; set; }
		public int PageSize { get; set; }
		public int PageNumber { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Period { get; set; }
	}
}
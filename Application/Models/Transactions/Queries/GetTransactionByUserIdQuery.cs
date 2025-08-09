namespace Application.Models.Transactions.Queries
{
	public class GetTransactionByUserIdQuery
	{
		public string UserId { get; set; }
		public int PageSize { get; set; }
		public int PageNumber { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public DateTime? Date { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Period { get; set; }
	}
}
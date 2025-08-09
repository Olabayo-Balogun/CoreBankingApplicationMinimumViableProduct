namespace Application.Models.Payments.Queries
{
	public class GetAllPaymentQuery
	{
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public string? Channel { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public DateTime? Date { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? Period { get; set; }
		public string? UserId { get; set; }
	}
}
namespace Application.Models.Payments.Queries
{
	public class GetPaymentByCustomerIdQuery
	{
		public string UserId { get; set; }
		public DateTime? DatePaid { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public DateTime? Date { get; set; }
		public string? Period { get; set; }
	}
}
namespace Application.Model.Payments.Queries
{
	public class GetPaymentByDatePaidQuery
	{
		public DateTime DatePaid { get; set; }
		public bool IsDeleted { get; set; }
		public string? ProductName { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
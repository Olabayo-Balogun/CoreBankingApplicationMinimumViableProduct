namespace Application.Models.Payments.Queries
{
	public class GetPaymentByAmountPaidQuery
	{
		public string? UserId { get; set; }
		public decimal Amount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
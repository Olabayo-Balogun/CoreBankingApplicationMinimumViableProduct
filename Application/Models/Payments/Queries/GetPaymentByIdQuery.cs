namespace Application.Model.Payments.Queries
{
	public class GetPaymentByIdQuery
	{
		public string PaymentPublicId { get; set; }
		public string? PaymentReferenceId { get; set; }
		public bool IsDeleted { get; set; }
		public string? UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
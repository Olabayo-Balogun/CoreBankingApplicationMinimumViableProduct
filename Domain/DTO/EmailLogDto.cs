namespace Domain.DTO
{
	public class EmailLogDto : AuditableEntityDto
	{
		public string ToRecipient { get; set; }
		public string? CcRecipient { get; set; }
		public string? BccRecipient { get; set; }
		public string? Subject { get; set; }
		public string Message { get; set; }
		public string Sender { get; set; }
		public bool IsHtml { get; set; }
		public bool IsSent { get; set; }
		public DateTime? DateSent { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

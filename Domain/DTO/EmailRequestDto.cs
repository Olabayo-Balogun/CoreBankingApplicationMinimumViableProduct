namespace Domain.DTO
{
	public class EmailRequestDto : AuditableEntityDto
	{
		public string ToRecipient { get; set; }
		public string? CcRecipient { get; set; }
		public string? BccRecipient { get; set; }
		public string? Subject { get; set; }
		public string Message { get; set; }
		public bool IsHtml { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

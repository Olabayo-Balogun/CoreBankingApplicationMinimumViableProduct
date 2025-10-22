namespace Application.Models.AuditLogs.Command
{
	public class CreateAuditLogCommand
	{
		public string Name { get; set; }
		public string Payload { get; set; }
		public string CreatedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

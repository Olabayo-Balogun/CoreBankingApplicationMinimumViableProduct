namespace Application.Model.EmailLogs.Command
{
	public class UpdateEmailLogSentStatusCommand
	{
		public long Id { get; set; }
		public bool IsSent { get; set; }
		public DateTime DateSent { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

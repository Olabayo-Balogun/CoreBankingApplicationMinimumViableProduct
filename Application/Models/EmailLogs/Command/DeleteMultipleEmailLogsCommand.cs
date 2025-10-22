namespace Application.Models.EmailLogs.Command
{
	public class DeleteMultipleEmailLogsCommand
	{
		public List<long> Ids { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

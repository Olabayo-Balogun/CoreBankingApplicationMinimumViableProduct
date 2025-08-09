namespace Application.Model.EmailLogs.Command
{
	public class DeleteEmailLogCommand
	{
		public long Id { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

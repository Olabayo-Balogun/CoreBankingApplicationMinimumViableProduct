namespace Application.Model.EmailRequests.Command
{
	public class DeleteEmailCommand
	{
		public long Id { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

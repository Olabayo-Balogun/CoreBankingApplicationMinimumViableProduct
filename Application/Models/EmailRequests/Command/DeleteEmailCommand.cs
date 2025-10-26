namespace Application.Models.EmailRequests.Command
{
	public class DeleteEmailCommand
	{
		public long Id { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

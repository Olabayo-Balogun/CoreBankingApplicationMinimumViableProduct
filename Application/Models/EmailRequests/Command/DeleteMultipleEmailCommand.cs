namespace Application.Model.EmailRequests.Command
{
	public class DeleteMultipleEmailCommand
	{
		public List<long> Ids { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

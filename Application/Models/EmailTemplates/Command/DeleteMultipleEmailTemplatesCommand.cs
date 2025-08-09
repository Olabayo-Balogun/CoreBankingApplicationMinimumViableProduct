namespace Application.Model.EmailTemplates.Command
{
	public class DeleteMultipleEmailTemplatesCommand
	{
		public List<long> Ids { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

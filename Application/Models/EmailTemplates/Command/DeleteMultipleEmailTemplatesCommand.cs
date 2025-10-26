namespace Application.Models.EmailTemplates.Command
{
	public class DeleteMultipleEmailTemplatesCommand
	{
		public List<long> Ids { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

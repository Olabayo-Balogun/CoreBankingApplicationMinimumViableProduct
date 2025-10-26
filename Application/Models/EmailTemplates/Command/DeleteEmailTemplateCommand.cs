namespace Application.Models.EmailTemplates.Command
{
	public class DeleteEmailTemplateCommand
	{
		public long Id { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

namespace Application.Model.EmailTemplates.Command
{
	public class DeleteEmailTemplateCommand
	{
		public long Id { get; set; }
		public string UserId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

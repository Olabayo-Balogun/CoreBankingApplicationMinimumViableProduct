namespace Application.Model.Uploads.Command
{
	public class DeleteMultipleUploadsCommand
	{
		public List<string> Ids { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

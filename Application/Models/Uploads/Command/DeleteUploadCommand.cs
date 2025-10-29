namespace Application.Models.Uploads.Command
{
    public class DeleteUploadCommand
    {
        public string Id { get; set; }
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

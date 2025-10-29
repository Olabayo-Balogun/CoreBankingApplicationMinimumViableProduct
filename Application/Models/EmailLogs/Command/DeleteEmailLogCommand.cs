namespace Application.Models.EmailLogs.Command
{
    public class DeleteEmailLogCommand
    {
        public long Id { get; set; }
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

namespace Application.Models.EmailRequests.Command
{
    public class DeleteMultipleEmailCommand
    {
        public List<long> Ids { get; set; }
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

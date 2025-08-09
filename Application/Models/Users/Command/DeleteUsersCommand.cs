namespace Application.Model.Users.Command
{
	public class DeleteUsersCommand
	{
		public List<string> UserIds { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
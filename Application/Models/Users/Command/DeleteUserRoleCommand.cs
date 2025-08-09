namespace Application.Model.Users.Command
{
	public class DeleteUserRoleCommand
	{
		public long Id { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

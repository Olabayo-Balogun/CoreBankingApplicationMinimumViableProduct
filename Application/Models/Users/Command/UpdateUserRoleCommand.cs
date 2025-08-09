namespace Application.Model.Users.Command
{
	public class UpdateUserRoleCommand
	{
		public string UserRole { get; set; }
		public string UserId { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

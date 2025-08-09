namespace Application.Model.Users.Command
{
	public class CreateUserRoleCommand
	{
		public string CreatedBy { get; set; }
		public string Name { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

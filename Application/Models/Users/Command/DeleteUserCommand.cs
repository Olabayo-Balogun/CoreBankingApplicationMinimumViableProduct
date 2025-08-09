namespace Application.Model.Users.Command
{
	public class DeleteUserCommand
	{
		public string UserId { get; set; }
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

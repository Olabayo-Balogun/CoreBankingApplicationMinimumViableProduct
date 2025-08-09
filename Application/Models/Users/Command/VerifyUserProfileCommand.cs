namespace Application.Model.Users.Command
{
	public class VerifyUserProfileCommand
	{
		public string UserId { get; set; }
		public string LastModifiedBy { get; set; }
		public bool IsVerified { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
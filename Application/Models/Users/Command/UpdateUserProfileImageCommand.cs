namespace Application.Model.Users.Command
{
	public class UpdateUserProfileImageCommand
	{
		public string ProfileImage { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

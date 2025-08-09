namespace Application.Model.Users.Command
{
	public class ValidateVerificationTokenCommand
	{
		public Guid UserId { get; set; }
		public string VerificationToken { get; set; }
	}
}
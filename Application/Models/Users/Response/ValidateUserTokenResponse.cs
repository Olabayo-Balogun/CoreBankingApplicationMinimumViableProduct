namespace Application.Model.Users.Response
{
	public class ValidateUserTokenResponse
	{
		public string UserToken { get; set; }
		public bool IsValid { get; set; }
	}
}
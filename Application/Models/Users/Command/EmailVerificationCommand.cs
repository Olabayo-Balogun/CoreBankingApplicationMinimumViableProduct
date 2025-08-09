using System.ComponentModel.DataAnnotations;

namespace Application.Model.Users.Command
{
	public class EmailVerificationCommand
	{
		/// <summary>
		/// Email address of the user
		/// </summary>
		[EmailAddress (ErrorMessage = "Please input user email")]
		public string Email { get; set; }
		/// <summary>
		/// The email verification token sent to the user
		/// </summary>
		[Required (ErrorMessage = "Please input verification token")]
		public string Token { get; set; }
	}
}

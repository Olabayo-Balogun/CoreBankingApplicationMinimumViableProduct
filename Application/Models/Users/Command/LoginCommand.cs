using System.ComponentModel.DataAnnotations;

using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class LoginCommand : IRequest<RequestResponse<LoginResponse>>
	{
		/// <summary>
		/// The user's email
		/// </summary>
		[EmailAddress (ErrorMessage = "Email is required")]
		public string Email { get; set; }
		/// <summary>
		/// The user's password
		/// </summary>
		[Required (ErrorMessage = "Password is required")]
		[DataType (DataType.Password)]
		[StringLength (100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		[Display (Name = "Password")]
		[RegularExpression ("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
		public string Password { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

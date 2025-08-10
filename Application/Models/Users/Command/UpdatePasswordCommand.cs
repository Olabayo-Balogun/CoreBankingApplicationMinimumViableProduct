using System.ComponentModel.DataAnnotations;

using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdatePasswordCommand : IRequest<RequestResponse<UserResponse>>
	{
		/// <summary>
		/// The user's new password
		/// </summary>
		[Required (ErrorMessage = "Password cannot be empty")]
		[DataType (DataType.Password)]
		[StringLength (100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		[RegularExpression ("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
		public string NewPassword { get; set; }
		/// <summary>
		/// The password inputted again so we can confirm that the person didn't input the first one by mistake
		/// </summary>
		[Compare ("NewPassword")]
		public string ConfirmPassword { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public string? LastModifiedBy { get; set; }
	}

}

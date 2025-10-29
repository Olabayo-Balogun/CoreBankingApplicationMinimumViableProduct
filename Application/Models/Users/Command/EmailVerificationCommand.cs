using Application.Models.Users.Response;

using MediatR;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Users.Command
{
    public class EmailVerificationCommand : IRequest<RequestResponse<UserResponse>>
    {
        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required]
        [EmailAddress (ErrorMessage = "Please input user email")]
        public string Email { get; set; }
        /// <summary>
        /// The email verification token sent to the user
        /// </summary>
        [Required (ErrorMessage = "Please input verification token")]
        public string Token { get; set; }
    }
}

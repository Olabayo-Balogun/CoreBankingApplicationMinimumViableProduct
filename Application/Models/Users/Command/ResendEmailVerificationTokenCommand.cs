using Application.Models.Users.Response;

using MediatR;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Users.Command
{
    public class ResendEmailVerificationTokenCommand : IRequest<RequestResponse<UserResponse>>
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
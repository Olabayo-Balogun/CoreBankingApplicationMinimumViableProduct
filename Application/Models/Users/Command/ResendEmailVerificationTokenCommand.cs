using System.ComponentModel.DataAnnotations;

using Application.Models.Users.Response;

using MediatR;

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
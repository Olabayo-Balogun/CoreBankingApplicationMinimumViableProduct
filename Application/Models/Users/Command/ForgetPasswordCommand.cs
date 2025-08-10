using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class ForgetPasswordCommand : IRequest<RequestResponse<UserResponse>>
	{
		public string Email { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

using Application.Model;
using Application.Model.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class LogoutCommand : IRequest<RequestResponse<LogoutResponse>>
	{
		public string Token { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

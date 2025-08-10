using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class LoginCommandHandler : IRequestHandler<LoginCommand, RequestResponse<LoginResponse>>
	{
		private readonly IUserRepository _userRepository;
		public LoginCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<LoginResponse>> Handle (LoginCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.LoginAsync (request);

			return result;
		}
	}
}

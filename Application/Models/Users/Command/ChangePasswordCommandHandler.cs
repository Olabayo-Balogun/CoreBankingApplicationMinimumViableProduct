using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public ChangePasswordCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (ChangePasswordCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.ChangePasswordAsync (request);

			return result;
		}
	}
}

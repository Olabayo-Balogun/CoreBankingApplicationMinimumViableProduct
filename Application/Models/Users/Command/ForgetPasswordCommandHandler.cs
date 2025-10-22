using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public ForgetPasswordCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (ForgetPasswordCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.ForgotPasswordAsync (request.Email, request.CancellationToken);

			return result;
		}
	}
}

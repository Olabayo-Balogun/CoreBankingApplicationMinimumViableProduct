using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public UpdatePasswordCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (UpdatePasswordCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.UpdatePasswordAsync (request);

			return result;
		}
	}
}

using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public DeleteUserCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (DeleteUserCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.DeleteUserAsync (request);

			return result;
		}
	}
}

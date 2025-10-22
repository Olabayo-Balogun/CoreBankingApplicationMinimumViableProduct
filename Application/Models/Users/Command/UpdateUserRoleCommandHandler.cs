using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public UpdateUserRoleCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (UpdateUserRoleCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.UpdateUserRoleAsync (request);

			return result;
		}
	}
}

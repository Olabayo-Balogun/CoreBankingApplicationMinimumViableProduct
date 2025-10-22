using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdateUserProfileImageCommandHandler : IRequestHandler<UpdateUserProfileImageCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public UpdateUserProfileImageCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (UpdateUserProfileImageCommand request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.UpdateUserProfileImageAsync (request.ProfileImage, request.LastModifiedBy, request.CancellationToken);

			return result;
		}
	}
}

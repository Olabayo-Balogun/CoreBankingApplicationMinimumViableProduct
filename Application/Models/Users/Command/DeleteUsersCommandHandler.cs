using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand, RequestResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;
        public DeleteUsersCommandHandler (IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RequestResponse<UserResponse>> Handle (DeleteUsersCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.DeleteMultipleUserAsync (request);

            return result;
        }
    }
}

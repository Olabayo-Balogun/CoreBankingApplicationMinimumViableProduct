using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, RequestResponse<LogoutResponse>>
    {
        private readonly IUserRepository _userRepository;
        public LogoutCommandHandler (IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RequestResponse<LogoutResponse>> Handle (LogoutCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.LogoutAsync (request.Token, request.CancellationToken);

            return result;
        }
    }
}

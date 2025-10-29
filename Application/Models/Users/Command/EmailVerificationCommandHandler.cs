using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
    public class EmailVerificationCommandHandler : IRequestHandler<EmailVerificationCommand, RequestResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;
        public EmailVerificationCommandHandler (IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RequestResponse<UserResponse>> Handle (EmailVerificationCommand request, CancellationToken cancellationToken)
        {
            ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.Email, null);
            if (!validateQueryAndPagination.IsValid)
            {
                return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
            }
            request.Email = validateQueryAndPagination.DecodedString;
            var result = await _userRepository.VerifyUserEmailAsync (request);

            return result;
        }
    }
}

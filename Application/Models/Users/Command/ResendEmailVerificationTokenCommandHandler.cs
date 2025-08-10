using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class ResendEmailVerificationTokenCommandHandler : IRequestHandler<ResendEmailVerificationTokenCommand, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public ResendEmailVerificationTokenCommandHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (ResendEmailVerificationTokenCommand request, CancellationToken cancellationToken)
		{
			ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameter (request.EmailAddress, null);
			if (!validateQueryAndPagination.IsValid)
			{
				return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
			}
			request.EmailAddress = validateQueryAndPagination.DecodedString;

			var result = await _userRepository.ResendEmailVerificationTokenAsync (request.EmailAddress, request.CancellationToken);

			return result;
		}
	}
}

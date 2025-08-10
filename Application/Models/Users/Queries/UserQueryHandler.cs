using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Queries
{
	public class UserQueryHandler : IRequestHandler<UserQuery, RequestResponse<UserResponse>>
	{
		private readonly IUserRepository _userRepository;
		public UserQueryHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<UserResponse>> Handle (UserQuery request, CancellationToken cancellationToken)
		{

			if (request.UserPublicId != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameter (request.UserPublicId, null);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.UserPublicId = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetUserByIdAsync (request.UserPublicId, request.CancellationToken);
				return result;
			}
			if (request.EmailAddress != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameter (request.EmailAddress, null);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.EmailAddress = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetUserByEmailAddressAsync (request.EmailAddress, request.CancellationToken);
				return result;
			}
			else if (request.Period != null && request.Date != null && request.IsCount)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameter (request.Period, null);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.Period = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetCountOfActiveUsersByDateAsync (request.Date.Value, request.Period, request.CancellationToken);
				return result;
			}
			if (request.Role != null && request.IsCount)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameter (request.Role, null);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<UserResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.Role = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetCountOfUserByRoleAsync (request.Role, request.CancellationToken);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == true && request.Date != null && request.IsCount)
			{
				var result = await _userRepository.GetCountOfDeletedUsersByDateAsync (request.Date.Value, request.CancellationToken);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == false && request.Date != null && request.IsCount)
			{
				var result = await _userRepository.GetCountOfCreatedUserByDateAsync (request.Date.Value, request.CancellationToken);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == true && request.IsCount)
			{
				var result = await _userRepository.GetCountOfDeletedUserAsync (request.CancellationToken);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value && request.IsCount)
			{
				var result = await _userRepository.GetCountOfDeletedUserAsync (request.CancellationToken);
				return result;
			}
			else
			{
				var result = await _userRepository.GetCountOfCreatedUserAsync (request.CancellationToken);
				return result;
			}
		}
	}
}

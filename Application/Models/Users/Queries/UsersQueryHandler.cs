using Application.Interface.Persistence;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Queries
{
	public class UsersQueryHandler : IRequestHandler<UsersQuery, RequestResponse<List<UserResponse>>>
	{
		private readonly IUserRepository _userRepository;
		public UsersQueryHandler (IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		public async Task<RequestResponse<List<UserResponse>>> Handle (UsersQuery request, CancellationToken cancellationToken)
		{

			if (request.Country != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameterAndPagination (request.Country, null, request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.Country = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetAllUserByCountryAsync (request.Country, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.Role != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameterAndPagination (request.Role, null, request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.Role = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetAllUserByRoleAsync (request.Role, request.IsDeleted.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == true && request.UserPublicId != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
				.ValidateQueryParameterAndPagination (request.UserPublicId, null, request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.UserPublicId = validateQueryAndPagination.DecodedString;
				var result = await _userRepository.GetDeletedUsersByUserIdAsync (request.UserPublicId, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == false && request.Date != null)
			{
				ValidationResponse validateQueryAndPagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				var result = await _userRepository.GetAllUserByDateAsync (request.Date.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == true && request.Date != null)
			{
				ValidationResponse validateQueryAndPagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				var result = await _userRepository.GetAllDeletedUserByDateAsync (request.Date.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == false)
			{
				ValidationResponse validateQueryAndPagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				var result = await _userRepository.GetLatestCreatedUsersAsync (request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.IsDeleted != null && request.IsDeleted.Value == true)
			{
				ValidationResponse validateQueryAndPagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				var result = await _userRepository.GetAllDeletedUsersAsync (request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else
			{
				ValidationResponse validateQueryAndPagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<UserResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				var result = await _userRepository.GetAllUsersAsync (request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
		}
	}
}

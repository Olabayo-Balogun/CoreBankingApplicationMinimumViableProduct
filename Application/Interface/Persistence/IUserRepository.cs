using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Application.Model;
using Application.Model.Users.Response;
using Application.Models.Users.Command;
using Application.Models.Users.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IUserRepository
	{
		Task<RequestResponse<LoginResponse>> LoginAsync (LoginCommand login);
		Task<RequestResponse<LogoutResponse>> LogoutAsync (string token, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> RegisterAsync (UserDto user);
		Task<RequestResponse<UserResponse>> VerifyUserEmailAsync (EmailVerificationCommand request);
		Task<RequestResponse<UserResponse>> ChangePasswordAsync (ChangePasswordCommand request);
		Task<RequestResponse<UserResponse>> ForgotPasswordAsync (string email, CancellationToken cancellationToken);
		JwtSecurityToken GetToken (List<Claim> authClaims);
		Task<RequestResponse<UserResponse>> UpdatePasswordAsync (UpdatePasswordCommand request);
		Task<RequestResponse<UserResponse>> GetUserFullNameByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> UpdateUserAsync (UserDto user);
		Task<RequestResponse<UserResponse>> GetCountOfActiveUsersByDateAsync (DateTime date, string period, CancellationToken cancellation);
		Task<RequestResponse<UserResponse>> UpdateUserProfileImageAsync (string profileImage, string userId, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> DeleteUserAsync (DeleteUserCommand request);
		Task<RequestResponse<UserResponse>> DeleteMultipleUserAsync (DeleteUsersCommand request);
		Task<RequestResponse<UserResponse>> GetUserByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> GetCountOfCreatedUserAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> GetCountOfDeletedUserAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> GetCountOfCreatedUserByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> GetCountOfDeletedUsersByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<List<UserResponse>>> GetAllUserByCountryAsync (string name, CancellationToken cancellation, int page, int pageSize);

		Task<RequestResponse<List<UserResponse>>> GetAllUsersAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UserResponse>>> GetAllUserByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UserResponse>>> GetAllUserByRoleAsync (string role, bool isDeleted, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<UserResponse>> GetCountOfUserByRoleAsync (string role, CancellationToken cancellationToken);
		Task<RequestResponse<List<UserResponse>>> GetAllDeletedUserByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UserResponse>>> GetAllDeletedUsersAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UserResponse>>> GetLatestCreatedUsersAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UserResponse>>> GetDeletedUsersByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<UserResponse>> ResendEmailVerificationTokenAsync (string emailAddress, CancellationToken cancellationToken);
		Task<RequestResponse<UserResponse>> GetUserLocationByIdAsync (string id, CancellationToken cancellation);
		Task<RequestResponse<UserResponse>> GetUserByEmailAddressAsync (string emailAddress, CancellationToken cancellation);
		Task<RequestResponse<UserResponse>> UpdateUserRoleAsync (UpdateUserRoleCommand updateUserResponsibility);
	}
}

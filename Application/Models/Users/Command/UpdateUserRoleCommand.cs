using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdateUserRoleCommand : IRequest<RequestResponse<UserResponse>>
	{
		public string UserRole { get; set; }
		public string UserId { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

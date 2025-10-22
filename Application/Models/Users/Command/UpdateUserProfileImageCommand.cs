using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
	public class UpdateUserProfileImageCommand : IRequest<RequestResponse<UserResponse>>
	{
		public string ProfileImage { get; set; }
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

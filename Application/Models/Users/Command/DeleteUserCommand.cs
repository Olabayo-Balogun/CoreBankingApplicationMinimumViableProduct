using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
    public class DeleteUserCommand : IRequest<RequestResponse<UserResponse>>
    {
        public string UserId { get; set; }
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

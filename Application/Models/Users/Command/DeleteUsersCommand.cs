using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Command
{
    public class DeleteUsersCommand : IRequest<RequestResponse<UserResponse>>
    {
        public List<string> UserIds { get; set; }
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
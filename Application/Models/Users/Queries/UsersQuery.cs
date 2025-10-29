using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Queries
{
    public class UsersQuery : IRequest<RequestResponse<List<UserResponse>>>
    {
        public string? UserPublicId { get; set; }
        public DateTime? Date { get; set; }
        public string? Role { get; set; }
        public string? Country { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsDeleted { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

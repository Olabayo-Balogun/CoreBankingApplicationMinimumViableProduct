using Application.Models.Industry.Response;

using MediatR;

namespace Application.Models.Industry.Queries
{
    public class IndustriesQuery : IRequest<RequestResponse<List<IndustryResponse>>>
    {
        public string? UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

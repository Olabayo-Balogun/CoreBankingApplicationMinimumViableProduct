using Application.Models.Industry.Response;

using MediatR;

namespace Application.Models.Industry.Queries
{
    public class IndustryQuery : IRequest<RequestResponse<IndustryResponse>>
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

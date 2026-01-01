using Application.Models.IndustryField.Response;

using MediatR;

namespace Application.Models.IndustryField.Queries
{
    public class IndustryFieldsQuery : IRequest<RequestResponse<List<IndustryFieldResponse>>>
    {
        public string? UserId { get; set; }
        public long? IndustryId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

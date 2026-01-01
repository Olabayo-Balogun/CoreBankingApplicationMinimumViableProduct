using Application.Models.IndustryField.Response;

using MediatR;

namespace Application.Models.IndustryField.Queries
{
    public class IndustryFieldQuery : IRequest<RequestResponse<IndustryFieldResponse>>
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

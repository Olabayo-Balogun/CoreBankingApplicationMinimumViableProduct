using Application.Models.IndustryField.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

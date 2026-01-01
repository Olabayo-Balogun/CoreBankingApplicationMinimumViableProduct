using Application.Models.Industry.Response;
using Application.Models.IndustryField.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

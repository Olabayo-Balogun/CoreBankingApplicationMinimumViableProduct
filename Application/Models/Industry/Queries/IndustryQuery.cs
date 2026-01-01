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
    public class IndustryQuery : IRequest<RequestResponse<IndustryResponse>>
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

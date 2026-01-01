using Application.Models.IndustryField.Response;
using Application.Models.Transactions.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.IndustryField.Queries
{
    public class IndustryFieldQuery : IRequest<RequestResponse<IndustryFieldResponse>>
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

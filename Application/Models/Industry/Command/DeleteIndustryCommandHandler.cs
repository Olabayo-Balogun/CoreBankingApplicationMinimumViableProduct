using Application.Interface.Persistence;
using Application.Models.Industry.Response;
using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Industry.Command
{
    public class DeleteIndustryCommandHandler : IRequest<RequestResponse<IndustryResponse>>
    {
        private readonly IIndustryRepository _industryRepository;
        public DeleteIndustryCommandHandler (IIndustryRepository industryRepository)
        {
            _industryRepository = industryRepository;
        }

        public async Task<RequestResponse<IndustryResponse>> Handle (DeleteIndustryCommand request, CancellationToken cancellationToken)
        {
            var result = await _industryRepository.DeleteIndustryAsync (request);

            return result;
        }
    }
}

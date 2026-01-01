using Application.Interface.Persistence;
using Application.Models.Industry.Queries;
using Application.Models.Industry.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Industry.Queries
{
    public class IndustryQueryHandler : IRequestHandler<IndustryQuery, RequestResponse<IndustryResponse>>
    {
        private readonly IIndustryRepository _industryRepository;
        public IndustryQueryHandler (IIndustryRepository industryRepository)
        {
            _industryRepository = industryRepository;
        }

        public async Task<RequestResponse<IndustryResponse>> Handle (IndustryQuery request, CancellationToken cancellationToken)
        {

            if (request.Id.HasValue)
            {
                var result = await _industryRepository.GetIndustryByIdAsync (request.Id.GetValueOrDefault (), request.CancellationToken);
                return result;
            }
            else if (request.Name != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.Name, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<IndustryResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.Name = validateQueryAndPagination.DecodedString;
                var result = await _industryRepository.GetIndustryByNameAsync (request.Name, request.CancellationToken);
                return result;
            }
            else
            {
                var result = await _industryRepository.GetIndustryCountAsync (request.CancellationToken);
                return result;
            }
        }
    }
}

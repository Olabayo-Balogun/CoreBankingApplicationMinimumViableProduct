using Application.Interface.Persistence;
using Application.Models.Industry.Response;
using Application.Models.IndustryField.Queries;
using Application.Models.IndustryField.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Industry.Queries
{
    public class IndustriesQueryHandler : IRequestHandler<IndustriesQuery, RequestResponse<List<IndustryResponse>>>
    {
        private readonly IIndustryRepository _industryRepository;
        public IndustriesQueryHandler (IIndustryRepository industryRepository)
        {
            _industryRepository = industryRepository;
        }

        public async Task<RequestResponse<List<IndustryResponse>>> Handle (IndustriesQuery request, CancellationToken cancellationToken)
        {

            if (request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameterAndPagination (request.UserId, null, request.PageNumber, request.PageSize);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<List<IndustryResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _industryRepository.GetIndustriesByUserIdAsync (request.UserId, request.CancellationToken, request.PageNumber, request.PageSize);
                return result;
            }  
            else
            {
                var result = await _industryRepository.GetAllIndustriesAsync (request.CancellationToken);
                return result;
            }
        }
    }
}

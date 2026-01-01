using Application.Interface.Persistence;
using Application.Models.Accounts.Response;
using Application.Models.IndustryField.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.IndustryField.Queries
{
    public class IndustryFieldsQueryHandler : IRequestHandler<IndustryFieldsQuery, RequestResponse<List<IndustryFieldResponse>>>
    {
        private readonly IIndustryFieldRepository _industryFieldRepository;
        public IndustryFieldsQueryHandler (IIndustryFieldRepository industryFieldRepository)
        {
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<List<IndustryFieldResponse>>> Handle (IndustryFieldsQuery request, CancellationToken cancellationToken)
        {

            if (request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameterAndPagination (request.UserId, null, request.PageNumber, request.PageSize);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<List<IndustryFieldResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _industryFieldRepository.GetIndustryFieldsByUserIdAsync (request.UserId, request.CancellationToken, request.PageNumber, request.PageSize);
                return result;
            }
            else if (request.IndustryId.HasValue)
            {
                ValidationResponse validatePagination = Utility.Utility
                .ValidatePagination (request.PageNumber, request.PageSize);
                if (!validatePagination.IsValid)
                {
                    return RequestResponse<List<IndustryFieldResponse>>.Failed (null, 400, validatePagination.Remark);
                }
                var result = await _industryFieldRepository.GetIndustryFieldsByIndustryIdAsync (request.IndustryId.GetValueOrDefault (), request.CancellationToken, request.PageNumber, request.PageSize);
                return result;
            }
            else
            {
                var result = await _industryFieldRepository.GetAllIndustryFieldsAsync (request.CancellationToken);
                return result;
            }
        }
    }
}

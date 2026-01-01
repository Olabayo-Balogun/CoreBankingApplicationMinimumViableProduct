using Application.Interface.Persistence;
using Application.Models.IndustryField.Response;

using MediatR;

namespace Application.Models.IndustryField.Queries
{
    public class IndustryFieldQueryHandler : IRequestHandler<IndustryFieldQuery, RequestResponse<IndustryFieldResponse>>
    {
        private readonly IIndustryFieldRepository _industryFieldRepository;
        public IndustryFieldQueryHandler (IIndustryFieldRepository industryFieldRepository)
        {
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<IndustryFieldResponse>> Handle (IndustryFieldQuery request, CancellationToken cancellationToken)
        {

            if (request.Id.HasValue)
            {
                var result = await _industryFieldRepository.GetIndustryFieldByIdAsync (request.Id.GetValueOrDefault (), request.CancellationToken);
                return result;
            }
            else if (request.Name != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.Name, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<IndustryFieldResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.Name = validateQueryAndPagination.DecodedString;
                var result = await _industryFieldRepository.GetIndustryFieldByNameAsync (request.Name, request.CancellationToken);
                return result;
            }
            else
            {
                var result = await _industryFieldRepository.GetIndustryFieldCountAsync (request.CancellationToken);
                return result;
            }
        }
    }
}

using Application.Interface.Persistence;
using Application.Models.IndustryField.Response;

using MediatR;

namespace Application.Models.IndustryField.Command
{
    public class DeleteIndustryFieldCommandHandler : IRequest<RequestResponse<IndustryFieldResponse>>
    {
        private readonly IIndustryFieldRepository _industryFieldRepository;
        public DeleteIndustryFieldCommandHandler (IIndustryFieldRepository industryFieldRepository)
        {
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<IndustryFieldResponse>> Handle (DeleteIndustryFieldCommand request, CancellationToken cancellationToken)
        {
            var result = await _industryFieldRepository.DeleteIndustryFieldAsync (request);

            return result;
        }
    }
}

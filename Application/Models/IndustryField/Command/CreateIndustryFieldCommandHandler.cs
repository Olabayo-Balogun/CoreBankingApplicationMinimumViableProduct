using Application.Interface.Persistence;
using Application.Models.IndustryField.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.IndustryField.Command
{
    public class CreateIndustryFieldCommandHandler : IRequestHandler<CreateIndustryFieldCommand, RequestResponse<IndustryFieldResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IIndustryFieldRepository _industryFieldRepository;
        public CreateIndustryFieldCommandHandler (IMapper mapper, IIndustryFieldRepository industryFieldRepository)
        {
            _mapper = mapper;
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<IndustryFieldResponse>> Handle (CreateIndustryFieldCommand request, CancellationToken cancellationToken)
        {
            var payload = _mapper.Map<IndustryFieldDto> (request);
            var result = await _industryFieldRepository.CreateIndustryFieldAsync (payload);

            return result;
        }
    }
}

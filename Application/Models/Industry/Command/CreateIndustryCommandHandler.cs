using Application.Interface.Persistence;
using Application.Models.Industry.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Industry.Command
{
    public class CreateIndustryCommandHandler : IRequestHandler<CreateIndustryCommand, RequestResponse<IndustryResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IIndustryRepository _industryRepository;
        public CreateIndustryCommandHandler (IMapper mapper, IIndustryRepository industryRepository)
        {
            _mapper = mapper;
            _industryRepository = industryRepository;
        }

        public async Task<RequestResponse<IndustryResponse>> Handle (CreateIndustryCommand request, CancellationToken cancellationToken)
        {
            var payload = _mapper.Map<IndustryDto> (request);
            var result = await _industryRepository.CreateIndustryAsync (payload);

            return result;
        }
    }
}

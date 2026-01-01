using Application.Interface.Persistence;
using Application.Models.Industry.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Industry.Command
{
    public class UpdateIndustryCommandHandler : IRequestHandler<UpdateIndustryCommand, RequestResponse<IndustryResponse>>
    {
        private readonly IIndustryRepository _industryRepository;
        private readonly IMapper _mapper;
        public UpdateIndustryCommandHandler (IMapper mapper, IIndustryRepository industryRepository)
        {
            _mapper = mapper;
            _industryRepository = industryRepository;
        }

        public async Task<RequestResponse<IndustryResponse>> Handle (UpdateIndustryCommand request, CancellationToken cancellationToken)
        {
            var industry = _mapper.Map<IndustryDto> (request);
            var result = await _industryRepository.UpdateIndustryAsync (industry);

            return result;
        }
    }
}

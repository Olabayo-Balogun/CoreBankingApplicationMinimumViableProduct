using Application.Interface.Persistence;
using Application.Models.Industry.Command;
using Application.Models.Industry.Response;
using Application.Models.IndustryField.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.IndustryField.Command
{
    public class UpdateIndustryFieldCommandHandler : IRequestHandler<UpdateIndustryFieldCommand, RequestResponse<IndustryFieldResponse>>
    {
        private readonly IIndustryFieldRepository _industryFieldRepository;
        private readonly IMapper _mapper;
        public UpdateIndustryFieldCommandHandler (IMapper mapper, IIndustryFieldRepository industryFieldRepository)
        {
            _mapper = mapper;
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<IndustryFieldResponse>> Handle (UpdateIndustryFieldCommand request, CancellationToken cancellationToken)
        {
            var industry = _mapper.Map<IndustryFieldDto> (request);
            var result = await _industryFieldRepository.UpdateIndustryFieldAsync (industry);

            return result;
        }
    }
}

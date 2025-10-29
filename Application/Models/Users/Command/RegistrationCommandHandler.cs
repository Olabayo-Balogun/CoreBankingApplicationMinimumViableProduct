using Application.Interface.Persistence;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Users.Command
{
    public class RegistrationCommandHandler : IRequestHandler<RegistrationCommand, RequestResponse<UserResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public RegistrationCommandHandler (IMapper mapper, IUserRepository userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<RequestResponse<UserResponse>> Handle (RegistrationCommand request, CancellationToken cancellationToken)
        {
            var payload = _mapper.Map<UserDto> (request);
            var result = await _userRepository.RegisterAsync (payload);

            return result;
        }
    }
}

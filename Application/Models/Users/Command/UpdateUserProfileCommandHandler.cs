using Application.Interface.Persistence;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Users.Command
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, RequestResponse<UserResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public UpdateUserProfileCommandHandler (IMapper mapper, IUserRepository userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<RequestResponse<UserResponse>> Handle (UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var payload = _mapper.Map<UserDto> (request);
            var result = await _userRepository.UpdateUserAsync (payload);

            return result;
        }
    }
}

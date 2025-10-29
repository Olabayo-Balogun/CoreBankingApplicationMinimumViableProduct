using Application.Interface.Persistence;
using Application.Models.Accounts.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Accounts.Command
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, RequestResponse<AccountResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        public CreateAccountCommandHandler (IMapper mapper, IAccountRepository accountRepository)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
        }

        public async Task<RequestResponse<AccountResponse>> Handle (CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var payload = _mapper.Map<AccountDto> (request);
            var result = await _accountRepository.CreateAccountAsync (payload);

            return result;
        }
    }

}

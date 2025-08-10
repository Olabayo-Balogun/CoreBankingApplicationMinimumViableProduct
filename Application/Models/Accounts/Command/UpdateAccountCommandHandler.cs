using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Accounts.Response;

using AutoMapper;

using Domain.DTO;

using MediatR;

namespace Application.Models.Accounts.Command
{
	public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, RequestResponse<AccountResponse>>
	{
		private readonly IAccountRepository _accountRepository;
		private readonly IMapper _mapper;
		public UpdateAccountCommandHandler (IMapper mapper, IAccountRepository accountRepository)
		{
			_mapper = mapper;
			_accountRepository = accountRepository;
		}

		public async Task<RequestResponse<AccountResponse>> Handle (UpdateAccountCommand request, CancellationToken cancellationToken)
		{
			var account = _mapper.Map<AccountDto> (request);
			var result = await _accountRepository.UpdateAccountAsync (account);

			return result;
		}
	}
}

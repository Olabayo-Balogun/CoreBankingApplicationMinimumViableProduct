using Application.Interface.Persistence;
using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Command
{
	public class DeleteAccountCommandHandler : IRequest<RequestResponse<AccountResponse>>
	{
		private readonly IAccountRepository _accountRepository;
		public DeleteAccountCommandHandler (IAccountRepository accountRepository)
		{
			_accountRepository = accountRepository;
		}

		public async Task<RequestResponse<AccountResponse>> Handle (DeleteAccountCommand request, CancellationToken cancellationToken)
		{
			var result = await _accountRepository.DeleteAccountAsync (request);

			return result;
		}
	}
}

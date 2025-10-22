using Application.Models;
using Application.Models.Accounts.Command;
using Application.Models.Accounts.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IAccountRepository
	{
		Task<RequestResponse<AccountResponse>> CreateAccountAsync (AccountDto account);
		Task<RequestResponse<AccountResponse>> DeleteAccountAsync (DeleteAccountCommand request);
		Task<RequestResponse<AccountResponse>> GetAccountByPublicIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<List<AccountResponse>>> GetAccountsByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<AccountResponse>> GetAccountCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<AccountResponse>> GetAccountCountByUserIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<AccountResponse>> UpdateAccountAsync (AccountDto account);
		Task<RequestResponse<AccountResponse>> GetAccountByAccountNumberAsync (string accountNumber, CancellationToken cancellationToken);
		Task<RequestResponse<AccountResponse>> GetAccountByLedgerNumberAsync (string ledgerNumber, CancellationToken cancellationToken);
	}
}

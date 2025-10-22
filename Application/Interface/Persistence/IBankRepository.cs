using Application.Models;
using Application.Models.Banks.Command;
using Application.Models.Banks.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IBankRepository
	{
		Task<RequestResponse<BankResponse>> CreateBankAsync (BankDto bank);
		Task<RequestResponse<BankResponse>> DeleteBankAsync (DeleteBankCommand request);
		Task<RequestResponse<BankResponse>> GetBankByPublicIdAsync (long id, CancellationToken cancellationToken);
		Task<RequestResponse<List<BankResponse>>> GetBanksByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<BankResponse>> GetBankCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<BankResponse>> GetBankCountByUserIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<BankResponse>> UpdateBankAsync (BankDto bank);
	}
}

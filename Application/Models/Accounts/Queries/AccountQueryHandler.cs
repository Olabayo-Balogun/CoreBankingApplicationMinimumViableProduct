using Application.Interface.Persistence;
using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Queries
{
    public class AccountQueryHandler : IRequestHandler<AccountQuery, RequestResponse<AccountResponse>>
    {
        private readonly IAccountRepository _accountRepository;
        public AccountQueryHandler (IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<RequestResponse<AccountResponse>> Handle (AccountQuery request, CancellationToken cancellationToken)
        {

            if (request.PublicId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.PublicId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<AccountResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.PublicId = validateQueryAndPagination.DecodedString;
                var result = await _accountRepository.GetAccountByPublicIdAsync (request.PublicId, request.CancellationToken);
                return result;
            }
            else if (request.AccountLedger != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.AccountLedger, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<AccountResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.AccountLedger = validateQueryAndPagination.DecodedString;
                var result = await _accountRepository.GetAccountByLedgerNumberAsync (request.AccountLedger, request.CancellationToken);
                return result;
            }
            else if (request.AccountNumber != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.AccountNumber, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<AccountResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.AccountNumber = validateQueryAndPagination.DecodedString;
                var result = await _accountRepository.GetAccountByAccountNumberAsync (request.AccountNumber, request.CancellationToken);
                return result;
            }
            else if (request.UserPublicId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserPublicId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<AccountResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.AccountNumber = validateQueryAndPagination.DecodedString;
                var result = await _accountRepository.GetAccountCountByUserIdAsync (request.AccountNumber, request.CancellationToken);
                return result;
            }
            else
            {
                var result = await _accountRepository.GetAccountCountAsync (request.CancellationToken);
                return result;
            }
        }
    }
}

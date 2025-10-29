using Application.Interface.Persistence;
using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Queries
{
    public class AccountsQueryHandler : IRequestHandler<AccountsQuery, RequestResponse<List<AccountResponse>>>
    {
        private readonly IAccountRepository _accountRepository;
        public AccountsQueryHandler (IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<RequestResponse<List<AccountResponse>>> Handle (AccountsQuery request, CancellationToken cancellationToken)
        {

            ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameterAndPagination (request.PublicId, null, request.PageNumber, request.PageSize);
            if (!validateQueryAndPagination.IsValid)
            {
                return RequestResponse<List<AccountResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
            }
            request.PublicId = validateQueryAndPagination.DecodedString;
            var result = await _accountRepository.GetAccountsByUserIdAsync (request.PublicId, request.CancellationToken, request.PageNumber, request.PageSize);
            return result;
        }
    }
}

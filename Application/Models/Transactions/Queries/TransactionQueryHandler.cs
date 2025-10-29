using Application.Interface.Persistence;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Queries
{
    public class TransactionQueryHandler : IRequestHandler<TransactionQuery, RequestResponse<TransactionResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        public TransactionQueryHandler (ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<RequestResponse<TransactionResponse>> Handle (TransactionQuery request, CancellationToken cancellationToken)
        {
            if (request.FromDate != null && request.ToDate == null)
            {
                var result = RequestResponse<TransactionResponse>.Failed (null, 400, "Please specify from and to date");
                return result;
            }
            else if (request.FromDate == null && request.ToDate != null)
            {
                var result = RequestResponse<TransactionResponse>.Failed (null, 400, "Please specify from and to date");
                return result;
            }
            else if (request.FromDate > request.ToDate)
            {
                var result = RequestResponse<TransactionResponse>.Failed (null, 400, "From date must be a date earlier than or equal to toDate");
                return result;
            }
            else if (request.FromDate > DateTime.Now || request.ToDate > DateTime.Now)
            {
                var result = RequestResponse<TransactionResponse>.Failed (null, 400, "You cannot select a date in the future");
                return result;
            }

            if (request.PublicId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.PublicId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.PublicId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsByIdAsync (request.PublicId, request.CancellationToken);
                return result;
            }
            else if (request.Date != null && request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsCountByDateAsync (request.UserId, request.Date.Value, request.CancellationToken);
                return result;
            }
            else if (request.Week != null && request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsCountByWeekAsync (request.UserId, request.Week.Value, request.CancellationToken);
                return result;
            }
            else if (request.Month != null && request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsCountByMonthAsync (request.UserId, request.Month.Value, request.CancellationToken);
                return result;
            }
            else if (request.Year != null && request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsCountByYearAsync (request.UserId, request.Year.Value, request.CancellationToken);
                return result;
            }
            else if (request.FromDate != null && request.ToDate != null && request.UserId != null)
            {
                ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
                .ValidateQueryParameter (request.UserId, null);
                if (!validateQueryAndPagination.IsValid)
                {
                    return RequestResponse<TransactionResponse>.Failed (null, 400, validateQueryAndPagination.Remark);
                }
                request.UserId = validateQueryAndPagination.DecodedString;
                var result = await _transactionRepository.GetTransactionsCountByCustomDateAsync (request.UserId, request.FromDate.Value, request.ToDate.Value, request.CancellationToken);
                return result;
            }
            else
            {
                var result = RequestResponse<TransactionResponse>.Failed (null, 400, "Input valid query parameters");
                return result;
            }
        }
    }
}

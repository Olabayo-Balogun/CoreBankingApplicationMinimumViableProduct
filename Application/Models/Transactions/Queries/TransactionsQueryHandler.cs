using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Queries
{
	public class TransactionsQueryHandler : IRequestHandler<TransactionsQuery, RequestResponse<List<TransactionResponse>>>
	{
		private readonly ITransactionRepository _transactionRepository;
		public TransactionsQueryHandler (ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

		public async Task<RequestResponse<List<TransactionResponse>>> Handle (TransactionsQuery request, CancellationToken cancellationToken)
		{
			if (request.UserId != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
					.ValidateQueryParameterAndPagination (request.UserId, null, request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<TransactionResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.UserId = validateQueryAndPagination.DecodedString;
			}
			else if (request.AccountNumber != null)
			{
				ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.Utility
					.ValidateQueryParameterAndPagination (request.AccountNumber, null, request.PageNumber, request.PageSize);
				if (!validateQueryAndPagination.IsValid)
				{
					return RequestResponse<List<TransactionResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
				}
				request.AccountNumber = validateQueryAndPagination.DecodedString;
			}

			if (request.Amount != null)
			{
				var result = await _transactionRepository.GetTransactionsByAmountPaidAsync (request.Amount.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Date != null && request.UserId != null)
			{
				var result = await _transactionRepository.GetTransactionsByDateAsync (request.UserId, request.Date.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Week != null && request.UserId != null)
			{
				var result = await _transactionRepository.GetTransactionsByWeekAsync (request.UserId, request.Week.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Month != null && request.UserId != null)
			{
				var result = await _transactionRepository.GetTransactionsByMonthAsync (request.UserId, request.Month.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Year != null && request.UserId != null)
			{
				var result = await _transactionRepository.GetTransactionsByYearAsync (request.UserId, request.Year.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.FromDate != null && request.ToDate != null && request.UserId != null)
			{
				var result = await _transactionRepository.GetTransactionsByCustomDateAsync (request.UserId, request.FromDate.Value, request.ToDate.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Date != null && request.AccountNumber != null)
			{
				var result = await _transactionRepository.GetTransactionByAccountNumberAndDateAsync (request.AccountNumber, request.Date.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Week != null && request.AccountNumber != null)
			{
				var result = await _transactionRepository.GetTransactionsByAccountNumberAndWeekAsync (request.AccountNumber, request.Week.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Month != null && request.AccountNumber != null)
			{
				var result = await _transactionRepository.GetTransactionsByAccountNumberAndMonthAsync (request.AccountNumber, request.Month.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.Year != null && request.AccountNumber != null)
			{
				var result = await _transactionRepository.GetTransactionsByAccountNumberAndYearAsync (request.AccountNumber, request.Year.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else if (request.FromDate != null && request.ToDate != null && request.AccountNumber != null)
			{
				var result = await _transactionRepository.GetTransactionsByAccountNumberAndCustomDateAsync (request.AccountNumber, request.FromDate.Value, request.ToDate.Value, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
			else
			{
				ValidationResponse validatePagination = Utility.Utility
				.ValidatePagination (request.PageNumber, request.PageSize);
				if (!validatePagination.IsValid)
				{
					return RequestResponse<List<TransactionResponse>>.Failed (null, 400, validatePagination.Remark);
				}
				var result = await _transactionRepository.GetAllTransactionsAsync (false, request.CancellationToken, request.PageNumber, request.PageSize);
				return result;
			}
		}
	}
}

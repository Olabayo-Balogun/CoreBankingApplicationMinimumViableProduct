using Application.Interface.Persistence;
using Application.Model;
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
			else if (request.Date != null && request.IsCount && request.UserId != null)
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
			else
			{
				var result = RequestResponse<TransactionResponse>.Failed (null, 400, "Input valid query parameters");
				return result;
			}
		}
	}
}

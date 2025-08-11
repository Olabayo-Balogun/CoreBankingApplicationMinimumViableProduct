using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class FlagTransactionCommandHandler : IRequestHandler<FlagTransactionCommand, RequestResponse<TransactionResponse>>
	{
		private readonly ITransactionRepository _transactionRepository;
		public FlagTransactionCommandHandler (ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

		public async Task<RequestResponse<TransactionResponse>> Handle (FlagTransactionCommand request, CancellationToken cancellationToken)
		{
			var result = await _transactionRepository.FlagTransactionAsync (request);

			return result;
		}
	}
}

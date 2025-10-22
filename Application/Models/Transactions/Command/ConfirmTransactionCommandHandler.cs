using Application.Interface.Persistence;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class ConfirmTransactionCommandHandler : IRequestHandler<ConfirmTransactionCommand, RequestResponse<TransactionResponse>>
	{
		private readonly ITransactionRepository _transactionRepository;
		public ConfirmTransactionCommandHandler (ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

		public async Task<RequestResponse<TransactionResponse>> Handle (ConfirmTransactionCommand request, CancellationToken cancellationToken)
		{
			var result = await _transactionRepository.ConfirmTransactionAsync (request);

			return result;
		}
	}
}

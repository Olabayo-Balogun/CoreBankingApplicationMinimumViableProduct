using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, RequestResponse<TransactionResponse>>
	{
		private readonly ITransactionRepository _transactionRepository;
		public DeleteTransactionCommandHandler (ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

		public async Task<RequestResponse<TransactionResponse>> Handle (DeleteTransactionCommand request, CancellationToken cancellationToken)
		{
			var result = await _transactionRepository.DeleteTransactionAsync (request);

			return result;
		}
	}
}

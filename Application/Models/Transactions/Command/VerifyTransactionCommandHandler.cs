using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class VerifyTransactionCommandHandler : IRequestHandler<ConfirmTransactionCommand, RequestResponse<TransactionResponse>>
	{
		private readonly ITransactionRepository _transactionRepository;
		public VerifyTransactionCommandHandler (ITransactionRepository transactionRepository)
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

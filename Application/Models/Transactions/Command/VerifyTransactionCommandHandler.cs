using Application.Interface.Persistence;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
    public class VerifyTransactionCommandHandler : IRequestHandler<VerifyTransactionCommand, RequestResponse<TransactionResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        public VerifyTransactionCommandHandler (ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<RequestResponse<TransactionResponse>> Handle (VerifyTransactionCommand request, CancellationToken cancellationToken)
        {
            var payload = new ConfirmTransactionCommand
            {
                PaymentReferenceId = request.PaymentReferenceId,
                Amount = request.Amount,
                CancellationToken = request.CancellationToken,
                LastModifiedBy = request.LastModifiedBy
            };
            var result = await _transactionRepository.ConfirmTransactionAsync (payload);

            return result;
        }
    }
}

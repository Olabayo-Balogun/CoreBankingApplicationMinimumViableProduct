using Application.Model;
using Application.Model.Transactions.Command;
using Application.Model.Transactions.Queries;
using Application.Models.Transactions.Queries;
using Application.Models.Transactions.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface ITransactionRepository
	{
		Task<RequestResponse<TransactionResponse>> CreateTransaction (TransactionDto createTransaction);
		Task<RequestResponse<List<TransactionResponse>>> CreateMultipleTransaction (List<TransactionDto> createTransaction);
		Task<RequestResponse<TransactionResponse>> UpdateTransaction (TransactionDto updateTransaction);
		Task<RequestResponse<TransactionResponse>> DeleteTransaction (DeleteTransactionCommand deleteTransaction);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAmountPaid (decimal amountPaid, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<TransactionResponse>> GetTransactionsById (string publicId, CancellationToken cancellationToken);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByBankName (string bankName, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionByBankNameAndAccountNumber (GetTransactionByBankNameAndAccountNumberQuery getTransactionByBankNameAndAccountNumberRequest);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByUserId (GetTransactionByUserIdQuery getTransactionByUserId);
		Task<RequestResponse<List<TransactionResponse>>> GetAllTransactions (bool isDeleted, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByDate (GetTransactionsByDateQuery getTransactionsByDate);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByCustomDate (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionByDate (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByWeek (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByMonth (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByYear (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByCustomDate (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByDate (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByWeek (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByMonth (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByYear (string userId, DateTime date, CancellationToken cancellationToken);
	}
}

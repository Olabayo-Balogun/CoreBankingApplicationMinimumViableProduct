using Application.Model;
using Application.Model.Transactions.Command;
using Application.Model.Transactions.Queries;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Queries;
using Application.Models.Transactions.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface ITransactionRepository
	{
		Task<RequestResponse<TransactionResponse>> CreateTransactionAsync (TransactionDto createTransaction);
		Task<RequestResponse<List<TransactionResponse>>> CreateMultipleTransactionAsync (List<TransactionDto> createTransaction);
		Task<RequestResponse<TransactionResponse>> UpdateTransactionAsync (TransactionDto updateTransaction);
		Task<RequestResponse<TransactionResponse>> DeleteTransactionAsync (DeleteTransactionCommand deleteTransaction);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAmountPaidAsync (decimal amountPaid, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<TransactionResponse>> GetTransactionsByIdAsync (string publicId, CancellationToken cancellationToken);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByBankNameAsync (string bankName, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionByBankNameAndAccountNumberAsync (GetTransactionByBankNameAndAccountNumberQuery getTransactionByBankNameAndAccountNumberRequest);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByUserIdAsync (GetTransactionByUserIdQuery getTransactionByUserId);
		Task<RequestResponse<List<TransactionResponse>>> GetAllTransactionsAsync (bool isDeleted, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByDateAsync (GetTransactionsByDateQuery getTransactionsByDate);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByCustomDateAsync (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionByDateAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByWeekAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByMonthAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByYearAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByCustomDateAsync (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByDateAsync (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByWeekAsync (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByMonthAsync (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByYearAsync (string userId, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> ConfirmTransactionAsync (ConfirmTransactionCommand updateTransactionRequest);
	}
}

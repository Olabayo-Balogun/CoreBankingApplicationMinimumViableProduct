using Application.Model;
using Application.Model.Transactions.Queries;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface ITransactionRepository
	{
		Task<RequestResponse<TransactionResponse>> CreateTransactionAsync (TransactionDto createTransaction);
		Task<RequestResponse<TransactionResponse>> DeleteTransactionAsync (DeleteTransactionCommand deleteTransaction);
		Task<RequestResponse<TransactionResponse>> FlagTransactionAsync (FlagTransactionCommand flagTransaction);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAmountPaidAsync (decimal amountPaid, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<TransactionResponse>> GetTransactionsByIdAsync (string publicId, CancellationToken cancellationToken);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByBankNameAsync (string bankName, CancellationToken cancellationToken, int pageNumber, int pageSize);
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
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndDateAsync (string accountNumber, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndDateAsync (string accountNumber, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndWeekAsync (string accountNumber, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndMonthAsync (string accountNumber, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndYearAsync (string accountNumber, DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndCustomDateAsync (string accountNumber, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionByAccountNumberAndDateAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndWeekAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndMonthAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndYearAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize);
	}
}

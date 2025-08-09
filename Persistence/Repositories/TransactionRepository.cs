using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Transactions.Command;
using Application.Model.Transactions.Queries;
using Application.Models.AuditLogs.Response;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Queries;
using Application.Models.Transactions.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
	public class TransactionRepository : ITransactionRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<TransactionRepository> _logger;
		private readonly IAuditLogRepository _auditLogRepository;
		public TransactionRepository (ApplicationDbContext context, IMapper mapper, ILogger<TransactionRepository> logger, IAuditLogRepository auditLogRepository)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
			_auditLogRepository = auditLogRepository;
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByBankName (string bankName, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionByBankName begins at {DateTime.UtcNow.AddHours (1)} for bank name: {bankName}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.RecipientBankName == bankName)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionByBankName for bank name: {bankName} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.RecipientBankName == bankName)
					.LongCountAsync ();

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionByBankName for bank name: {bankName} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionByBankName for bank name: {bankName} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsById (string publicId, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsById begins at {DateTime.UtcNow.AddHours (1)} for publicId: {publicId}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == publicId)
					.OrderBy (x => x.RecipientBankName)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badResponse = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");
					_logger.LogInformation ($"GetTransactionsById for publicId: {publicId} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var response = RequestResponse<TransactionResponse>.SearchSuccessful (result, 1, "Transaction");
				_logger.LogInformation ($"GetTransactionsById for publicId: {publicId} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsById for publicId: {publicId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionByBankNameAndAccountNumber (GetTransactionByBankNameAndAccountNumberQuery getTransactionByBankNameAndAccountNumberRequest)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionByBankNameAndAccountNumber begins at {DateTime.UtcNow.AddHours (1)} for Bank Name: {getTransactionByBankNameAndAccountNumberRequest.BankName}, account number: {getTransactionByBankNameAndAccountNumberRequest.AccountNumber}, isDepositor: {getTransactionByBankNameAndAccountNumberRequest.IsDepositor}, and isDeleted: {getTransactionByBankNameAndAccountNumberRequest.IsDeleted}");

				var result = getTransactionByBankNameAndAccountNumberRequest.IsDepositor
					? await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == getTransactionByBankNameAndAccountNumberRequest.IsDeleted && x.SenderBankName == getTransactionByBankNameAndAccountNumberRequest.BankName && x.SenderAccountNumber == getTransactionByBankNameAndAccountNumberRequest.AccountNumber)
					.OrderBy (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((getTransactionByBankNameAndAccountNumberRequest.PageNumber - 1) * getTransactionByBankNameAndAccountNumberRequest.PageSize)
					.Take (getTransactionByBankNameAndAccountNumberRequest.PageSize)
					.ToListAsync (getTransactionByBankNameAndAccountNumberRequest.CancellationToken)
					: await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == getTransactionByBankNameAndAccountNumberRequest.IsDeleted && x.RecipientBankName == getTransactionByBankNameAndAccountNumberRequest.BankName && x.RecipientAccountNumber == getTransactionByBankNameAndAccountNumberRequest.AccountNumber)
					.OrderBy (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((getTransactionByBankNameAndAccountNumberRequest.PageNumber - 1) * getTransactionByBankNameAndAccountNumberRequest.PageSize)
					.Take (getTransactionByBankNameAndAccountNumberRequest.PageSize)
					.ToListAsync (getTransactionByBankNameAndAccountNumberRequest.CancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionByBankNameAndAccountNumber for Bank Name: {getTransactionByBankNameAndAccountNumberRequest.BankName}, account number: {getTransactionByBankNameAndAccountNumberRequest.AccountNumber}, isDepositor: {getTransactionByBankNameAndAccountNumberRequest.IsDepositor}, and isDeleted: {getTransactionByBankNameAndAccountNumberRequest.IsDeleted} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = getTransactionByBankNameAndAccountNumberRequest.IsDepositor ? await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == getTransactionByBankNameAndAccountNumberRequest.IsDeleted && x.SenderBankName == getTransactionByBankNameAndAccountNumberRequest.BankName && x.SenderAccountNumber == getTransactionByBankNameAndAccountNumberRequest.AccountNumber).LongCountAsync (getTransactionByBankNameAndAccountNumberRequest.CancellationToken) : await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == getTransactionByBankNameAndAccountNumberRequest.IsDeleted && x.RecipientBankName == getTransactionByBankNameAndAccountNumberRequest.BankName && x.RecipientAccountNumber == getTransactionByBankNameAndAccountNumberRequest.AccountNumber).LongCountAsync (getTransactionByBankNameAndAccountNumberRequest.CancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionByBankNameAndAccountNumber for Bank Name: {getTransactionByBankNameAndAccountNumberRequest.BankName}, account number: {getTransactionByBankNameAndAccountNumberRequest.AccountNumber}, isDepositor: {getTransactionByBankNameAndAccountNumberRequest.IsDepositor}, and isDeleted: {getTransactionByBankNameAndAccountNumberRequest.IsDeleted} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionByBankNameAndAccountNumber for Bank Name: {getTransactionByBankNameAndAccountNumberRequest.BankName}, account number: {getTransactionByBankNameAndAccountNumberRequest.AccountNumber}, isDepositor: {getTransactionByBankNameAndAccountNumberRequest.IsDepositor}, and isDeleted: {getTransactionByBankNameAndAccountNumberRequest.IsDeleted} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> CreateTransaction (TransactionDto createTransaction)
		{
			try
			{
				_logger.LogInformation ($"CreateTransaction begins at {DateTime.UtcNow.AddHours (1)} by User PublicId: {createTransaction.CreatedBy} for amount: {createTransaction.Amount}");
				if (createTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);
					return badRequest;
				}

				if (!createTransaction.TransactionType.Equals (TransactionType.Credit, StringComparison.OrdinalIgnoreCase) && !createTransaction.TransactionType.Equals (TransactionType.Debit, StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<TransactionResponse>.Failed (null, 400, "Specify transaction type as either debit or credit");
					return badRequest;
				}

				var payload = _mapper.Map<Domain.Entities.Transaction> (createTransaction);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);
				payload.CreatedBy = createTransaction.CreatedBy;
				payload.PublicId = Guid.NewGuid ().ToString ();

				await _context.Transactions.AddAsync (payload, createTransaction.CancellationToken);

				await _context.SaveChangesAsync (createTransaction.CancellationToken);

				var response = _mapper.Map<TransactionResponse> (payload);
				var result = RequestResponse<TransactionResponse>.Created (response, 1, "Transaction");

				_logger.LogInformation ($"CreateTransaction ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by User PublicId: {createTransaction.CreatedBy} for amount: {createTransaction.Amount}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateTransaction by User PublicId: {createTransaction.CreatedBy} for amount: {createTransaction.Amount} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> CreateMultipleTransaction (List<TransactionDto> createTransaction)
		{
			try
			{
				_logger.LogInformation ($"CreateMultipleTransaction begins at {DateTime.UtcNow.AddHours (1)} by User PublicId: {createTransaction.First ().CreatedBy}");
				if (createTransaction == null)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NullPayload (null);
					return badRequest;
				}

				var payload = _mapper.Map<List<Domain.Entities.Transaction>> (createTransaction);
				foreach (var transaction in payload)
				{
					transaction.IsDeleted = false;
					transaction.DateDeleted = null;
					transaction.LastModifiedBy = null;
					transaction.LastModifiedDate = null;
					transaction.DeletedBy = null;
					transaction.DateCreated = DateTime.UtcNow.AddHours (1);
					transaction.CreatedBy = createTransaction.First ().CreatedBy;
					transaction.PublicId = Guid.NewGuid ().ToString ();
				}

				await _context.AddRangeAsync (payload, createTransaction.First ().CancellationToken);
				await _context.SaveChangesAsync (createTransaction.First ().CancellationToken);

				var response = _mapper.Map<List<TransactionResponse>> (payload);
				var result = RequestResponse<List<TransactionResponse>>.Created (response, response.Count, "Transactions");

				_logger.LogInformation ($"CreateMultipleTransaction ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by User PublicId: {createTransaction.First ().CreatedBy}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateMultipleTransaction by User PublicId: {createTransaction.First ().CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> UpdateTransaction (TransactionDto updateTransactionRequest)
		{
			try
			{
				_logger.LogInformation ($"UpdateTransaction begins at {DateTime.UtcNow.AddHours (1)} by User PublicId: {updateTransactionRequest.LastModifiedBy} for amount: {updateTransactionRequest.Amount}");
				if (updateTransactionRequest == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateTransaction ends at {DateTime.UtcNow.AddHours (1)}");

					return badRequest;
				}

				var updateTransaction = await _context.Transactions.Where (x => x.PublicId == updateTransactionRequest.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");
					_logger.LogInformation ($"UpdateTransaction ends at {DateTime.UtcNow.AddHours (1)} by User PublicId: {updateTransactionRequest.LastModifiedBy} for amount: {updateTransactionRequest.Amount}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = updateTransactionRequest.CancellationToken,
					CreatedBy = updateTransaction.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (updateTransaction)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateTransaction ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by User PublicId: {updateTransactionRequest.LastModifiedBy} for amount: {updateTransactionRequest.Amount}");
					return badRequest;
				}

				updateTransaction.Amount = updateTransactionRequest.Amount;
				updateTransaction.Notes = updateTransactionRequest.Notes;
				updateTransaction.Description = updateTransactionRequest.Description;
				updateTransaction.SenderAccountNumber = updateTransactionRequest.SenderAccountNumber;
				updateTransaction.SenderBankName = updateTransactionRequest.SenderBankName;
				updateTransaction.RecipientAccountNumber = updateTransactionRequest.RecipientAccountNumber;
				updateTransaction.RecipientBankName = updateTransactionRequest.RecipientBankName;
				updateTransaction.TransactionType = updateTransactionRequest.TransactionType;
				updateTransaction.LastModifiedBy = updateTransactionRequest.LastModifiedBy;
				updateTransaction.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				_context.Entry (updateTransaction).State = EntityState.Modified;
				_context.Transactions.Update (updateTransaction);
				await _context.SaveChangesAsync (updateTransactionRequest.CancellationToken);

				var result = _mapper.Map<TransactionResponse> (updateTransaction);
				var response = RequestResponse<TransactionResponse>.Updated (result, 1, "Transaction");
				_logger.LogInformation ($"UpdateTransaction at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by User PublicId: {updateTransactionRequest.LastModifiedBy} for amount: {updateTransactionRequest.Amount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateTransaction by User PublicId: {updateTransactionRequest.LastModifiedBy} for amount: {updateTransactionRequest.Amount} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> ConfirmTransaction (ConfirmTransactionCommand updateTransactionRequest)
		{
			try
			{
				_logger.LogInformation ($"ConfirmTransaction begins at {DateTime.UtcNow.AddHours (1)} by System for amount: {updateTransactionRequest.Amount}");
				if (updateTransactionRequest == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);
					_logger.LogInformation ($"ConfirmTransaction ends at {DateTime.UtcNow.AddHours (1)}");

					return badRequest;
				}

				var updateTransaction = await _context.Transactions.Where (x => x.PaymentReferenceId == updateTransactionRequest.PaymentReferenceId && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");
					_logger.LogInformation ($"ConfirmTransaction ends at {DateTime.UtcNow.AddHours (1)} by System for amount: {updateTransactionRequest.Amount}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = updateTransactionRequest.CancellationToken,
					CreatedBy = updateTransaction.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (updateTransaction)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"ConfirmTransaction ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by System for amount: {updateTransactionRequest.Amount}");
					return badRequest;
				}

				updateTransaction.IsReconciled = true;
				updateTransaction.LastModifiedBy = "SYSTEM";
				updateTransaction.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				var updateSenderAccountDetails = await _context.Accounts.Where (x => x.AccountNumber == updateTransaction.RecipientAccountNumber && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateSenderAccountDetails == null && updateTransaction.TransactionType.Equals (TransactionType.Credit, StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Sender Bank account details");
					_logger.LogInformation ($"ConfirmTransaction ends at {DateTime.UtcNow.AddHours (1)} by System for amount: {updateTransactionRequest.Amount}");
					return badRequest;
				}

				if (updateTransaction.TransactionType.Equals (TransactionType.Credit, StringComparison.OrdinalIgnoreCase))
				{
					updateSenderAccountDetails.Balance += updateTransactionRequest.Amount;
					updateSenderAccountDetails.LastModifiedBy = "SYSTEM";
					updateSenderAccountDetails.LastModifiedDate = DateTime.UtcNow.AddHours (1);

					_context.Accounts.Update (updateSenderAccountDetails);
				}
				else if (updateTransaction.TransactionType.Equals (TransactionType.Debit, StringComparison.OrdinalIgnoreCase))
				{
					var updateAccountDetails = await _context.Accounts.Where (x => x.AccountNumber == updateTransaction.SenderAccountNumber && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

					if (updateAccountDetails == null)
					{
						var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Bank account details");
						_logger.LogInformation ($"ConfirmTransaction ends at {DateTime.UtcNow.AddHours (1)} by System for amount: {updateTransactionRequest.Amount}");
						return badRequest;
					}

					updateAccountDetails.Balance -= updateTransactionRequest.Amount;
					updateAccountDetails.LastModifiedBy = "SYSTEM";
					updateAccountDetails.LastModifiedDate = DateTime.UtcNow.AddHours (1);

					_context.Accounts.Update (updateAccountDetails);
				}


				_context.Transactions.Update (updateTransaction);
				await _context.SaveChangesAsync (updateTransactionRequest.CancellationToken);

				var result = _mapper.Map<TransactionResponse> (updateTransaction);
				var response = RequestResponse<TransactionResponse>.Updated (result, 1, "Transaction");
				_logger.LogInformation ($"ConfirmTransaction at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by System for amount: {updateTransactionRequest.Amount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"ConfirmTransaction by System for amount: {updateTransactionRequest.Amount} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> DeleteTransaction (DeleteTransactionCommand deleteTransaction)
		{
			try
			{
				_logger.LogInformation ($"DeleteTransaction begins at {DateTime.UtcNow.AddHours (1)} by UserId: {deleteTransaction.DeletedBy} for publicId: {deleteTransaction.PublicId}");
				if (deleteTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);
					return badRequest;
				}

				var transactionCheck = await _context.Transactions.Where (x => x.PublicId == deleteTransaction.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (deleteTransaction.CancellationToken);
				if (transactionCheck == null)
				{
					_logger.LogInformation ($"DeleteTransaction ends at {DateTime.UtcNow.AddHours (1)} by UserId: {deleteTransaction.DeletedBy} for publicId: {deleteTransaction.PublicId}");
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = deleteTransaction.CancellationToken,
					CreatedBy = transactionCheck.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (transactionCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badResult = RequestResponse<TransactionResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"DeleteTransaction ends at {DateTime.UtcNow.AddHours (1)} by UserId: {deleteTransaction.DeletedBy} for publicId: {deleteTransaction.PublicId}");
					return badResult;
				}

				transactionCheck.IsDeleted = true;
				transactionCheck.DeletedBy = deleteTransaction.DeletedBy;
				transactionCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Entry (transactionCheck).State = EntityState.Modified;
				_context.Update (transactionCheck);
				await _context.SaveChangesAsync (deleteTransaction.CancellationToken);

				_logger.LogInformation ($"DeleteTransaction ends at {DateTime.UtcNow.AddHours (1)} by UserId: {deleteTransaction.DeletedBy} for publicId: {deleteTransaction.PublicId}");
				var result = RequestResponse<TransactionResponse>.Deleted (null, 1, "Transaction");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteTransaction by UserId: {deleteTransaction.DeletedBy} for publicId: {deleteTransaction.PublicId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAmountPaid (decimal amountPaid, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByAmountPaid begins at {DateTime.UtcNow.AddHours (1)} for Amount paid: {amountPaid}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Amount == amountPaid)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByAmountPaid for Amount paid: {amountPaid} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.Amount == amountPaid).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionsByAmountPaid for Amount paid: {amountPaid} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByAmountPaid for Amount paid: {amountPaid} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByUserId (GetTransactionByUserIdQuery getTransactionByUserId)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByUserId begins at {DateTime.UtcNow.AddHours (1)} for User PublicId: {getTransactionByUserId.UserId}, isDeleted: {getTransactionByUserId.IsDeleted}, toDate: {getTransactionByUserId.ToDate}, fromDate: {getTransactionByUserId.FromDate}, date: {getTransactionByUserId.Date}, and period: {getTransactionByUserId.Period}");

				var result = RequestResponse<List<TransactionResponse>>.NullPayload (null);
				if (getTransactionByUserId.Date != null && getTransactionByUserId.UserId != null)
				{
					result = await GetTransactionByDate (getTransactionByUserId.UserId, getTransactionByUserId.Date.GetValueOrDefault (), getTransactionByUserId.CancellationToken, getTransactionByUserId.PageNumber, getTransactionByUserId.PageSize);
				}
				else if (getTransactionByUserId.Date != null && getTransactionByUserId.Period != null && getTransactionByUserId.UserId != null && getTransactionByUserId.Period.ToLower () == "week" && getTransactionByUserId.Date != null)
				{
					result = await GetTransactionsByWeek (getTransactionByUserId.UserId, getTransactionByUserId.Date.GetValueOrDefault (), getTransactionByUserId.CancellationToken, getTransactionByUserId.PageNumber, getTransactionByUserId.PageSize);
				}
				else if (getTransactionByUserId.Date != null && getTransactionByUserId.Period != null && getTransactionByUserId.UserId != null && getTransactionByUserId.Period.ToLower () == "month" && getTransactionByUserId.Date != null)
				{
					result = await GetTransactionsByMonth (getTransactionByUserId.UserId, getTransactionByUserId.Date.GetValueOrDefault (), getTransactionByUserId.CancellationToken, getTransactionByUserId.PageNumber, getTransactionByUserId.PageSize);
				}
				else if (getTransactionByUserId.Date != null && getTransactionByUserId.Period != null && getTransactionByUserId.UserId != null && getTransactionByUserId.Period.ToLower () == "year" && getTransactionByUserId.Date != null)
				{
					result = await GetTransactionsByYear (getTransactionByUserId.UserId, getTransactionByUserId.Date.GetValueOrDefault (), getTransactionByUserId.CancellationToken, getTransactionByUserId.PageNumber, getTransactionByUserId.PageSize);
				}
				else if (getTransactionByUserId.Date != null && getTransactionByUserId.Period != null && getTransactionByUserId.UserId != null && getTransactionByUserId.FromDate != null && getTransactionByUserId.ToDate != null)
				{
					result = await GetTransactionsByCustomDate (getTransactionByUserId.UserId, getTransactionByUserId.FromDate.GetValueOrDefault (), getTransactionByUserId.ToDate.GetValueOrDefault (), getTransactionByUserId.CancellationToken, getTransactionByUserId.PageNumber, getTransactionByUserId.PageSize);
				}
				else
				{
					result.Data = await _context.Transactions
						.AsNoTracking ()
						.Where (x => x.IsDeleted == getTransactionByUserId.IsDeleted && x.CreatedBy == getTransactionByUserId.UserId)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
						.ToListAsync (getTransactionByUserId.CancellationToken);
				}

				if (result.Data != null && result.Data.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByUserId for User PublicId: {getTransactionByUserId.UserId}, isDeleted: {getTransactionByUserId.IsDeleted}, toDate: {getTransactionByUserId.ToDate}, fromDate: {getTransactionByUserId.FromDate}, date: {getTransactionByUserId.Date}, and period: {getTransactionByUserId.Period} ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} with count: {result.TotalCount}");

					return result;
				}

				result.TotalCount = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == getTransactionByUserId.IsDeleted && x.CreatedBy == getTransactionByUserId.UserId).LongCountAsync (getTransactionByUserId.CancellationToken);

				_logger.LogInformation ($"GetTransactionsByUserId for User PublicId: {getTransactionByUserId.UserId}, isDeleted: {getTransactionByUserId.IsDeleted}, toDate: {getTransactionByUserId.ToDate}, fromDate: {getTransactionByUserId.FromDate}, date: {getTransactionByUserId.Date}, and period: {getTransactionByUserId.Period} ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} with count: {result.TotalCount}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByUserId for User PublicId: {getTransactionByUserId.UserId}, isDeleted: {getTransactionByUserId.IsDeleted}, toDate: {getTransactionByUserId.ToDate}, fromDate: {getTransactionByUserId.FromDate}, date: {getTransactionByUserId.Date}, and period: {getTransactionByUserId.Period} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetAllTransactions (bool isDeleted, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllTransactions for isDeleted: {isDeleted} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == isDeleted)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetAllTransactions for isDeleted: {isDeleted} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == isDeleted).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetAllTransactions for isDeleted: {isDeleted} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllTransactions for isDeleted: {isDeleted} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByDate (GetTransactionsByDateQuery getTransactionsByDate)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByDate for isDeleted: {getTransactionsByDate.IsDeleted}, toDate: {getTransactionsByDate.ToDate}, fromDate: {getTransactionsByDate.FromDate}, date: {getTransactionsByDate.Date}, and period: {getTransactionsByDate.Period} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.DateCreated == getTransactionsByDate.Date.Date)
					.OrderBy (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((getTransactionsByDate.PageNumber - 1) * getTransactionsByDate.PageSize)
					.Take (getTransactionsByDate.PageSize)
					.ToListAsync (getTransactionsByDate.CancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					_logger.LogInformation ($"GetTransactionsByDate for isDeleted: {getTransactionsByDate.IsDeleted}, toDate: {getTransactionsByDate.ToDate}, fromDate: {getTransactionsByDate.FromDate}, date: {getTransactionsByDate.Date}, and period: {getTransactionsByDate.Period} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");

					return badResponse;
				}

				var count = await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.DateCreated == getTransactionsByDate.Date.Date).LongCountAsync (getTransactionsByDate.CancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				_logger.LogInformation ($"GetTransactionsByDate for isDeleted: {getTransactionsByDate.IsDeleted}, toDate: {getTransactionsByDate.ToDate}, fromDate: {getTransactionsByDate.FromDate}, date: {getTransactionsByDate.Date}, and period: {getTransactionsByDate.Period} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByDate for isDeleted: {getTransactionsByDate.IsDeleted}, toDate: {getTransactionsByDate.ToDate}, fromDate: {getTransactionsByDate.FromDate}, date: {getTransactionsByDate.Date}, and period: {getTransactionsByDate.Period} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByCustomDate (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsCountByCustomDate begins at {DateTime.UtcNow.AddHours (1)} for fromDate: {fromDate:dd/MM/yyyy} to toDate: {toDate:dd/MM/yyyy} by user ID: {userId}");

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");
				_logger.LogInformation ($"GetTransactionsCountByCustomDate ends at {DateTime.UtcNow.AddHours (1)} for fromDate: {fromDate:dd/MM/yyyy} to toDate: {toDate:dd/MM/yyyy} by user ID: {userId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsCountByCustomDate for fromDate: {fromDate:dd/MM/yyyy} to toDate: {toDate:dd/MM/yyyy} by user ID: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByDate (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsCountByDate begins at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");
				_logger.LogInformation ($"GetTransactionsCountByDate ends at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsCountByDate for date: {date:dd/MM/yyyy} by user ID: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByWeek (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsCountByWeek begins at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");
				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				_logger.LogInformation ($"GetTransactionsCountByWeek ends at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsCountByWeek for date: {date:dd/MM/yyyy} by user ID: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByMonth (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsCountByMonth begins at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");
				_logger.LogInformation ($"GetTransactionsCountByMonth ends at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsCountByMonth for date: {date:dd/MM/yyyy} by user ID: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByYear (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsCountByYear begins at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");
				_logger.LogInformation ($"GetTransactionsCountByYear ends at {DateTime.UtcNow.AddHours (1)} for date: {date:dd/MM/yyyy} by user ID: {userId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsCountByYear for date: {date:dd/MM/yyyy} by user ID: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByCustomDate (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByCustomDate for user ID: {userId}, fromDate: {fromDate}, and toDate: {toDate} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.CreatedBy)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByCustomDate for user ID: {userId}, fromDate: {fromDate}, and toDate: {toDate} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionsByCustomDate for user ID: {userId}, fromDate: {fromDate}, and toDate: {toDate} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByCustomDate for user ID: {userId}, fromDate: {fromDate}, and toDate: {toDate} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionByDate (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionByDate for user ID: {userId} and date: {date} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionByDate for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionByDate for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionByDate for user ID: {userId} and date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByWeek (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByWeek for user ID: {userId} and date: {date} begins at {DateTime.UtcNow.AddHours (1)}");

				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.CreatedBy)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByWeek for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionsByWeek for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByWeek for user ID: {userId} and date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByMonth (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByMonth for user ID: {userId} and date: {date} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.OrderByDescending (x => x.CreatedBy)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByMonth for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionsByMonth for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByMonth for user ID: {userId} and date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByYear (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetTransactionsByYear for user ID: {userId} and date: {date} begins at {DateTime.UtcNow.AddHours (1)}");

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.OrderByDescending (x => x.CreatedBy)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");
					_logger.LogInformation ($"GetTransactionsByYear for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");
				_logger.LogInformation ($"GetTransactionsByYear for user ID: {userId} and date: {date} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetTransactionsByYear for user ID: {userId} and date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

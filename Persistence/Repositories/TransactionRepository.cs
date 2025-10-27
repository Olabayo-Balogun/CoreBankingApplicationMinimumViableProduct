using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Branches.Response;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Response;
using Application.Utility;

using AutoMapper;
using AutoMapper.Internal;

using Azure.Core;

using CloudinaryDotNet;

using Domain.DTO;
using Domain.Entities;
using Domain.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByBankNameAsync (string bankName, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByBankNameAsync), nameof (bankName), bankName);
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByBankNameAsync), nameof (bankName), bankName, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.RecipientBankName == bankName)
					.LongCountAsync ();

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByBankNameAsync), nameof (bankName), bankName, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByBankNameAsync), nameof (bankName), bankName, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsByIdAsync (string publicId, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByIdAsync), nameof (publicId), publicId);
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == publicId)
					.OrderBy (x => x.RecipientBankName)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByIdAsync), nameof (publicId), publicId, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var response = RequestResponse<TransactionResponse>.SearchSuccessful (result, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByIdAsync), nameof (publicId), publicId, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByIdAsync), nameof (publicId), publicId, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> CreateTransactionAsync (TransactionDto createTransaction)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString(), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy);
				_logger.LogInformation (openingLog);

				if (createTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateTransactionAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				if (!createTransaction.TransactionType.Equals (TransactionType.Credit, StringComparison.OrdinalIgnoreCase) && !createTransaction.TransactionType.Equals (TransactionType.Debit, StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<TransactionResponse>.Failed (null, 400, "Specify transaction type as either debit or credit");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var payload = _mapper.Map<Transaction> (createTransaction);

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

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> CreateWithdrawalTransactionAsync (TransactionDto createTransaction)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy);
				_logger.LogInformation (openingLog);

				if (createTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateWithdrawalTransactionAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				if (!createTransaction.TransactionType.Equals (TransactionType.Credit, StringComparison.OrdinalIgnoreCase) && !createTransaction.TransactionType.Equals (TransactionType.Debit, StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<TransactionResponse>.Failed (null, 400, "Specify transaction type as either debit or credit");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var payload = _mapper.Map<Domain.Entities.Transaction> (createTransaction);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.DeletedBy = null;
				payload.IsReconciled = true;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);
				payload.CreatedBy = createTransaction.CreatedBy;
				payload.PublicId = Guid.NewGuid ().ToString ();

				await _context.Transactions.AddAsync (payload, createTransaction.CancellationToken);

				var updateAccountDetails = await _context.Accounts.Where (x => x.AccountNumber == createTransaction.SenderAccountNumber && x.IsDeleted == false).FirstOrDefaultAsync (createTransaction.CancellationToken);

				if (updateAccountDetails == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Sender bank account details");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = createTransaction.CancellationToken,
					CreatedBy = updateAccountDetails.CreatedBy,
					Name = "Account",
					Payload = JsonConvert.SerializeObject (updateAccountDetails)
				};

				updateAccountDetails.Balance -= createTransaction.Amount;
				updateAccountDetails.LastModifiedBy = "SYSTEM";
				updateAccountDetails.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				await _context.SaveChangesAsync (createTransaction.CancellationToken);

				var response = _mapper.Map<TransactionResponse> (payload);
				var result = RequestResponse<TransactionResponse>.Created (response, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateWithdrawalTransactionAsync), nameof (createTransaction.Amount), createTransaction.Amount.ToString (), nameof (createTransaction.CreatedBy), createTransaction.CreatedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> UpdateTransactionAsync (TransactionDto updateTransactionRequest)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateTransactionAsync), nameof (updateTransactionRequest.Amount), updateTransactionRequest.Amount.ToString (), nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy);
				_logger.LogInformation (openingLog);

				if (updateTransactionRequest == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateTransactionAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var updateTransaction = await _context.Transactions.Where (x => x.PublicId == updateTransactionRequest.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateTransactionAsync), nameof (updateTransactionRequest.Amount), updateTransactionRequest.Amount.ToString (), nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = updateTransactionRequest.CancellationToken,
					CreatedBy = updateTransaction.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (updateTransaction)
				};

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

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateTransactionAsync), nameof (updateTransactionRequest.Amount), updateTransactionRequest.Amount.ToString (), nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				await _context.SaveChangesAsync (updateTransactionRequest.CancellationToken);

				var result = _mapper.Map<TransactionResponse> (updateTransaction);
				var response = RequestResponse<TransactionResponse>.Updated (result, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateTransactionAsync), nameof (updateTransactionRequest.Amount), updateTransactionRequest.Amount.ToString (), nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateTransactionAsync), nameof (updateTransactionRequest.Amount), updateTransactionRequest.Amount.ToString (), nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> ConfirmTransactionAsync (ConfirmTransactionCommand updateTransactionRequest)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy);
				_logger.LogInformation (openingLog);

				if (updateTransactionRequest == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (ConfirmTransactionAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var updateTransaction = await _context.Transactions.Where (x => x.PaymentReferenceId == updateTransactionRequest.PaymentReferenceId && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createUpdateTransactionAuditLogRequest = new ()
				{
					CancellationToken = updateTransactionRequest.CancellationToken,
					CreatedBy = updateTransaction.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (updateTransaction)
				};

				updateTransaction.IsReconciled = true;
				updateTransaction.LastModifiedBy = updateTransactionRequest.LastModifiedBy;
				updateTransaction.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				var updateSenderAccountDetails = await _context.Accounts.Where (x => x.AccountNumber == updateTransaction.RecipientAccountNumber && x.IsDeleted == false).FirstOrDefaultAsync (updateTransactionRequest.CancellationToken);

				if (updateSenderAccountDetails == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Recipient Bank account details");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestForAccount = new ()
				{
					CancellationToken = updateTransactionRequest.CancellationToken,
					CreatedBy = updateSenderAccountDetails.CreatedBy,
					Name = "Account",
					Payload = JsonConvert.SerializeObject (updateSenderAccountDetails)
				};

				updateSenderAccountDetails.Balance += updateTransactionRequest.Amount;
				updateSenderAccountDetails.LastModifiedBy = updateTransactionRequest.LastModifiedBy;
				updateSenderAccountDetails.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				var auditPayloads = new List<CreateAuditLogCommand>
				{
					createUpdateTransactionAuditLogRequest,
					createAuditLogRequestForAccount
				};

				RequestResponse<AuditLogsQueryResponse> createAuditLog = await _auditLogRepository.CreateMultipleAuditLogAsync (auditPayloads);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				await _context.SaveChangesAsync (updateTransactionRequest.CancellationToken);

				var result = _mapper.Map<TransactionResponse> (updateTransaction);
				var response = RequestResponse<TransactionResponse>.Updated (result, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (ConfirmTransactionAsync), nameof (updateTransactionRequest.PaymentReferenceId), updateTransactionRequest.PaymentReferenceId, nameof (updateTransactionRequest.LastModifiedBy), updateTransactionRequest.LastModifiedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> DeleteTransactionAsync (DeleteTransactionCommand deleteTransaction)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.PublicId), deleteTransaction.PublicId, nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy);
				_logger.LogInformation (openingLog);

				if (deleteTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var transactionCheck = await _context.Transactions.Where (x => x.PublicId == deleteTransaction.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (deleteTransaction.CancellationToken);

				if (transactionCheck == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.PublicId), deleteTransaction.PublicId, nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = deleteTransaction.CancellationToken,
					CreatedBy = transactionCheck.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (transactionCheck)
				};

				transactionCheck.IsDeleted = true;
				transactionCheck.DeletedBy = deleteTransaction.DeletedBy;
				transactionCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.PublicId), deleteTransaction.PublicId, nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}				

				await _context.SaveChangesAsync (deleteTransaction.CancellationToken);

				var result = RequestResponse<TransactionResponse>.Deleted (null, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.PublicId), deleteTransaction.PublicId, nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy, result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteTransactionAsync), nameof (deleteTransaction.PublicId), deleteTransaction.PublicId, nameof (deleteTransaction.DeletedBy), deleteTransaction.DeletedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> FlagTransactionAsync (FlagTransactionCommand flagTransaction)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (FlagTransactionAsync), nameof (flagTransaction.PublicId), flagTransaction.PublicId, nameof (flagTransaction.LastModifiedBy), flagTransaction.LastModifiedBy);
				_logger.LogInformation (openingLog);

				if (flagTransaction == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (FlagTransactionAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var transactionCheck = await _context.Transactions.Where (x => x.PublicId == flagTransaction.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (flagTransaction.CancellationToken);
				if (transactionCheck == null)
				{
					var badRequest = RequestResponse<TransactionResponse>.NotFound (null, "Transaction");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (FlagTransactionAsync), nameof (flagTransaction.PublicId), flagTransaction.PublicId, nameof (flagTransaction.LastModifiedBy), flagTransaction.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequest = new ()
				{
					CancellationToken = flagTransaction.CancellationToken,
					CreatedBy = transactionCheck.CreatedBy,
					Name = "Transaction",
					Payload = JsonConvert.SerializeObject (transactionCheck)
				};

				transactionCheck.IsFlagged = true;
				transactionCheck.LastModifiedBy = flagTransaction.LastModifiedBy;
				transactionCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequest);

				if (!createAuditLog.IsSuccessful)
				{
					var badRequest = RequestResponse<TransactionResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (FlagTransactionAsync), nameof (flagTransaction.PublicId), flagTransaction.PublicId, nameof (flagTransaction.LastModifiedBy), flagTransaction.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				await _context.SaveChangesAsync (flagTransaction.CancellationToken);

				var result = RequestResponse<TransactionResponse>.Deleted (null, 1, "Transaction");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (FlagTransactionAsync), nameof (flagTransaction.PublicId), flagTransaction.PublicId, nameof (flagTransaction.LastModifiedBy), flagTransaction.LastModifiedBy, result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (FlagTransactionAsync), nameof (flagTransaction.PublicId), flagTransaction.PublicId, nameof (flagTransaction.LastModifiedBy), flagTransaction.LastModifiedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAmountPaidAsync (decimal amount, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByAmountPaidAsync), nameof (amount), amount.ToString ());
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Amount == amount)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAmountPaidAsync), nameof (amount), amount.ToString (), nameof(badRequest.TotalCount), badRequest.TotalCount.ToString(), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Amount == amount)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAmountPaidAsync), nameof (amount), amount.ToString (), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByAmountPaidAsync), nameof (amount), amount.ToString (), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetAllTransactionsAsync (bool isDeleted, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllTransactionsAsync), nameof (isDeleted), isDeleted.ToString ());
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllTransactionsAsync), nameof (isDeleted), isDeleted.ToString (), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
				.AsNoTracking ()
				.Where (x => x.IsDeleted == isDeleted).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllTransactionsAsync), nameof (isDeleted), isDeleted.ToString (), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllTransactionsAsync), nameof (isDeleted), isDeleted.ToString (), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByCustomDateAsync (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByDateAsync (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByWeekAsync (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByWeekAsync), nameof (userId), userId, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByWeekAsync), nameof (userId), userId, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByWeekAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByMonthAsync (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByYearAsync (string userId, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByCustomDateAsync (string userId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString(), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByCustomDateAsync), nameof (userId), userId, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByDateAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByDateAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByWeekAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByWeekAsync), nameof (userId), userId, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByWeekAsync), nameof (userId), userId, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByWeekAsync), nameof (userId), userId, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByWeekAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByMonthAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByMonthAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByYearAsync (string userId, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

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
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByYearAsync), nameof (userId), userId, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndDateAsync (string accountNumber, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndDateAsync (string accountNumber, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndWeekAsync (string accountNumber, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndMonthAsync (string accountNumber, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTransactionsCountByAccountNumberAndYearAsync (string accountNumber, DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsCountByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<TransactionResponse>.CountSuccessful (null, count, "Transaction");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsCountByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsCountByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndCustomDateAsync (string accountNumber, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByAccountNumberAndCustomDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndCustomDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= fromDate.Date && x.DateCreated.Date <= toDate.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndCustomDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByAccountNumberAndCustomDateAsync), nameof (accountNumber), accountNumber, nameof (fromDate), fromDate.ToString ("dd/MM/yyyy"), nameof (toDate), toDate.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionByAccountNumberAndDateAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date == date.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionByAccountNumberAndDateAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndWeekAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				DateTime startOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
				DateTime endOfWeek = startOfWeek.AddDays (7);

				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date >= startOfWeek.Date && x.DateCreated.Date <= endOfWeek.Date && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (startOfWeek), startOfWeek.ToString ("dd/MM/yyyy"), nameof (endOfWeek), endOfWeek.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByAccountNumberAndWeekAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndMonthAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Month == date.Month && x.IsDeleted == false && x.DateCreated.Year == date.Year)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByAccountNumberAndMonthAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<List<TransactionResponse>>> GetTransactionsByAccountNumberAndYearAsync (string accountNumber, DateTime date, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTransactionsByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"));
				_logger.LogInformation (openingLog);

				var result = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new TransactionResponse { Amount = x.Amount, Description = x.Description, IsFlagged = x.IsFlagged, IsReconciled = x.IsReconciled, Notes = x.Notes, PublicId = x.PublicId, RecipientAccountName = x.RecipientAccountName, RecipientAccountNumber = x.RecipientAccountNumber, RecipientBankName = x.RecipientBankName, SenderAccountName = x.SenderAccountName, SenderAccountNumber = x.SenderAccountNumber, SenderBankName = x.SenderBankName, TransactionType = x.TransactionType, Currency = x.Currency })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<TransactionResponse>>.NotFound (null, "Transactions");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var count = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Year == date.Year && x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<TransactionResponse>>.SearchSuccessful (result, count, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTransactionsByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTransactionsByAccountNumberAndYearAsync), nameof (accountNumber), accountNumber, nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<TransactionResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTotalTransactionWithdrawalByAccountNumberAsync (string accountNumber, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTotalTransactionWithdrawalByAccountNumberAsync), nameof (accountNumber), accountNumber);
				_logger.LogInformation (openingLog);

				var sum = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date == DateTime.Now.Date && x.IsDeleted == false && x.TransactionType == TransactionType.Debit)
					.Select (x => x.Amount)
					.SumAsync (cancellationToken);

				var result = new TransactionResponse { Amount = sum };

				var response = RequestResponse<TransactionResponse>.SearchSuccessful (result, 1, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTotalTransactionWithdrawalByAccountNumberAsync), nameof (accountNumber), accountNumber, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTotalTransactionWithdrawalByAccountNumberAsync), nameof (accountNumber), accountNumber, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<TransactionResponse>> GetTotalTransactionDepositByAccountNumberAsync (string accountNumber, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetTotalTransactionDepositByAccountNumberAsync), nameof (accountNumber), accountNumber);
				_logger.LogInformation (openingLog);

				var sum = await _context.Transactions
					.AsNoTracking ()
					.Where (x => x.RecipientAccountNumber == accountNumber && x.DateCreated.Date == DateTime.Now.Date && x.IsDeleted == false && x.TransactionType == TransactionType.Credit)
					.Select (x => x.Amount)
					.SumAsync (cancellationToken);

				var result = new TransactionResponse { Amount = sum };

				var response = RequestResponse<TransactionResponse>.SearchSuccessful (result, 1, "Transactions");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetTotalTransactionDepositByAccountNumberAsync), nameof (accountNumber), accountNumber, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetTotalTransactionDepositByAccountNumberAsync), nameof (accountNumber), accountNumber, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<TransactionResponse>.Error (null);
			}
		}
	}
}
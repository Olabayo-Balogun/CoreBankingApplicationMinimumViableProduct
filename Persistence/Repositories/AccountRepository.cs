using Application.Interface.Persistence;
using Application.Models;
using Application.Models.Accounts.Command;
using Application.Models.Accounts.Response;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Account = Domain.Entities.Account;
using Utility = Application.Utility.Utility;

namespace Persistence.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IAuditLogRepository _auditLogRepository;
		private readonly IMapper _mapper;
		private readonly AppSettings _appSettings;
		private readonly ILogger<AccountRepository> _logger;
		public AccountRepository (ApplicationDbContext context, IMapper mapper, ILogger<AccountRepository> logger, IAuditLogRepository auditLogRepository, IOptions<AppSettings> appsettings)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
			_auditLogRepository = auditLogRepository;
			_appSettings = appsettings.Value;
		}

		public async Task<RequestResponse<AccountResponse>> CreateAccountAsync (AccountDto account)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy);
				_logger.LogInformation (openingLog);

				if (account == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NullPayload (null);
					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				string branchCode = _appSettings.DefaultBranchCode.ToString ();
				string bankCode = _appSettings.BankCode.ToString ();

				long customerId = await _context.Users.AsNoTracking ().Where (x => x.PublicId == account.CreatedBy)
					.Select (x => x.Id)
					.FirstOrDefaultAsync (account.CancellationToken);

				if (customerId < 1)
				{
					var badRequest = RequestResponse<AccountResponse>.Failed (null, 400, "Unable to validate user identity");
					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);
					return badRequest;
				}

				string customerNumber = customerId.ToString ().PadLeft (6, '0');

				string ledgerNumber = Utility.GenerateLedgerNumber (branchCode, customerNumber, account.AccountType.ToString (), 0.ToString ()).Trim ();

				long existingLedgerNumberCount = await _context.Accounts.AsNoTracking ().Where (x => x.LedgerNumber == ledgerNumber)
					.LongCountAsync (account.CancellationToken);

				if (existingLedgerNumberCount > 0)
				{
					ledgerNumber = Utility.GenerateLedgerNumber (branchCode, customerNumber, account.AccountType.ToString (), existingLedgerNumberCount.ToString ()).Trim ();
				}

				string? nuban = await GenerateUniqueNUBANAsync (bankCode, customerNumber, account.CancellationToken);

				if (nuban == null)
				{
					var badRequest = RequestResponse<AccountResponse>.Failed (null, 500, "Unable to generate unique account number");
					string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);
					return badRequest;
				}

				var payload = _mapper.Map<Account> (account);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.AccountNumber = nuban;
				payload.AccountStatus = AccountStatus.ActiveTier1;
				payload.LedgerNumber = ledgerNumber;
				payload.MaximumDailyDepositLimitAmount = _appSettings.MaximumDailyDepositLimitAmount;
				payload.MaximumDailyWithdrawalLimitAmount = _appSettings.MaximumDailyWithdrawalLimitAmount;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);
				payload.PublicId = Guid.NewGuid ().ToString ();

				await _context.Accounts.AddAsync (payload, account.CancellationToken);
				await _context.SaveChangesAsync (account.CancellationToken);

				var response = _mapper.Map<AccountResponse> (payload);
				var result = RequestResponse<AccountResponse>.Created (response, 1, "Account");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy, result.Remark);
				_logger.LogInformation (conclusionLog);
				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateAccountAsync), nameof (account.CreatedBy), account.CreatedBy, ex.Message);
				_logger.LogError (errorLog);
				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		private async Task<string?> GenerateUniqueNUBANAsync (string bankCode, string customerNumber, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GenerateUniqueNUBANAsync), nameof (bankCode), bankCode, nameof (customerNumber), customerNumber);
				_logger.LogInformation (openingLog);
				string nuban = Utility.GenerateNUBAN (bankCode, customerNumber);

				Random rand = new ();
				int maxAttempts = 1000;
				int attempts = 0;

				long existingAccountNumberCount = await _context.Accounts
						.AsNoTracking ()
						.Where (x => x.AccountNumber == nuban)
						.LongCountAsync (cancellationToken);

				if (existingAccountNumberCount < 1)
				{
					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GenerateUniqueNUBANAsync), nameof (bankCode), bankCode, nameof (customerNumber), customerNumber, nuban);
					_logger.LogInformation (closingLog);
					return nuban;
				}

				while (attempts < maxAttempts)
				{
					int serial = rand.Next (0, 1000000);
					string serialStr = serial.ToString ("D6");

					string newNuban = Utility.GenerateNUBAN (bankCode, serialStr);

					long count = await _context.Accounts
						.AsNoTracking ()
						.Where (x => x.AccountNumber == newNuban)
						.LongCountAsync (cancellationToken);

					if (count < 1)
					{
						string closingLog = Utility.GenerateMethodConclusionLog (nameof (GenerateUniqueNUBANAsync), nameof (bankCode), bankCode, nameof (customerNumber), customerNumber, nuban);
						_logger.LogInformation (closingLog);
						return newNuban;
					}

					attempts++;
				}

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GenerateUniqueNUBANAsync), nameof (bankCode), bankCode, nameof (customerNumber), customerNumber, nuban);
				_logger.LogInformation (conclusionLog);
				return null;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GenerateUniqueNUBANAsync), nameof (bankCode), bankCode, nameof (customerNumber), customerNumber, ex.Message);
				_logger.LogError (errorLog);
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> DeleteAccountAsync (DeleteAccountCommand request)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteAccountAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (request.Id), request.Id);
				_logger.LogInformation (openingLog);

				var accountCheck = await _context.Accounts.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (accountCheck == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteAccountAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (request.Id), request.Id, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = request.CancellationToken,
					CreatedBy = accountCheck.CreatedBy,
					Name = "Account",
					Payload = JsonConvert.SerializeObject (accountCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<AccountResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteAccountAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (request.Id), request.Id, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				accountCheck.IsDeleted = true;
				accountCheck.DeletedBy = request.DeletedBy;
				accountCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				await _context.SaveChangesAsync ();

				var result = RequestResponse<AccountResponse>.Deleted (null, 1, "Account");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteAccountAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (request.Id), request.Id, result.Remark);
				_logger.LogInformation (conclusionLog);

				return result;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteAccountAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (request.Id), request.Id, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByPublicIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountByPublicIdAsync), nameof (id), id);
				_logger.LogInformation (openingLog);

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.PublicId == id)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByPublicIdAsync), nameof (id), id, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");
				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByPublicIdAsync), nameof (id), id, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountByPublicIdAsync), nameof (id), id, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByAccountNumberAsync (string accountNumber, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountByAccountNumberAsync), nameof (accountNumber), accountNumber);
				_logger.LogInformation (openingLog);

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.AccountNumber == accountNumber)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByAccountNumberAsync), nameof (accountNumber), accountNumber, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByAccountNumberAsync), nameof (accountNumber), accountNumber, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountByAccountNumberAsync), nameof (accountNumber), accountNumber, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByLedgerNumberAsync (string ledgerNumber, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountByLedgerNumberAsync), nameof (ledgerNumber), ledgerNumber);
				_logger.LogInformation (openingLog);

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.LedgerNumber == ledgerNumber)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByLedgerNumberAsync), nameof (ledgerNumber), ledgerNumber, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountByLedgerNumberAsync), nameof (ledgerNumber), ledgerNumber, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountByLedgerNumberAsync), nameof (ledgerNumber), ledgerNumber, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<List<AccountResponse>>> GetAccountsByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountsByUserIdAsync), nameof (id), id);
				_logger.LogInformation (openingLog);

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.CreatedBy == id)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<AccountResponse>>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountsByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badResponse.Remark);
					_logger.LogInformation (closingLog);

					return badResponse;
				}

				var count = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.CreatedBy == id && account.IsDeleted == false)
					.LongCountAsync ();

				var response = RequestResponse<List<AccountResponse>>.SearchSuccessful (result, count, "Accounts");
				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountsByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountsByUserIdAsync), nameof (id), id, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<List<AccountResponse>>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountCountAsync));
				_logger.LogInformation (openingLog);

				long count = await _context.Accounts
					.AsNoTracking ()
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<AccountResponse>.CountSuccessful (null, count, "Account");

				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountCountAsync), response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountCountAsync), ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountCountByUserIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAccountCountByUserIdAsync), nameof (id), id);
				_logger.LogInformation (openingLog);

				long count = await _context.Accounts
					.AsNoTracking ()
					.Where (x => x.CreatedBy == id)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<AccountResponse>.CountSuccessful (null, count, "Account");
				string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAccountCountByUserIdAsync), nameof (id), id, response.Remark);
				_logger.LogInformation (closingLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAccountCountByUserIdAsync), nameof (id), id, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}

		public async Task<RequestResponse<AccountResponse>> UpdateAccountAsync (AccountDto account)
		{
			try
			{
				string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy);
				_logger.LogInformation (openingLog);

				if (account == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NullPayload (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateAccountAsync), badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				var updateAccountRequest = await _context.Accounts
					.Where (x => x.PublicId == account.PublicId && x.IsDeleted == false)
					.FirstOrDefaultAsync (account.CancellationToken);

				if (updateAccountRequest == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				long existingLedgerNumberCount = await _context.Accounts.AsNoTracking ().Where (x => x.LedgerNumber == account.LedgerNumber)
					.LongCountAsync (account.CancellationToken);

				if (existingLedgerNumberCount > 0)
				{
					var badRequest = RequestResponse<AccountResponse>.AlreadyExists (null, existingLedgerNumberCount, "You are not allowed to create multiple accounts of the same type");

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = account.CancellationToken,
					CreatedBy = updateAccountRequest.CreatedBy,
					Name = "Account",
					Payload = JsonConvert.SerializeObject (updateAccountRequest)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<AccountResponse>.AuditLogFailed (null);

					string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy, badRequest.Remark);
					_logger.LogInformation (closingLog);

					return badRequest;
				}

				updateAccountRequest.LedgerNumber = account.LedgerNumber;
				updateAccountRequest.AccountStatus = account.AccountStatus;
				updateAccountRequest.MaximumDailyWithdrawalLimitAmount = account.MaximumDailyWithdrawalLimitAmount;
				updateAccountRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateAccountRequest.LastModifiedBy = account.LastModifiedBy;

				await _context.SaveChangesAsync (account.CancellationToken);

				var result = _mapper.Map<AccountResponse> (updateAccountRequest);
				var response = RequestResponse<AccountResponse>.Updated (result, 1, "Account");

				string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy, response.Remark);
				_logger.LogInformation (conclusionLog);

				return response;
			}
			catch (Exception ex)
			{
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateAccountAsync), nameof (account.PublicId), account.PublicId, nameof (account.LastModifiedBy), account.LastModifiedBy, ex.Message);
				_logger.LogError (errorLog);

				return RequestResponse<AccountResponse>.Error (null);
			}
		}
	}
}

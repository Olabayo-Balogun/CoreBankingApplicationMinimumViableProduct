using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Models;
using Application.Models.Accounts.Command;
using Application.Models.Accounts.Response;
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
				_logger.LogInformation ($"CreateAccount begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {account.CreatedBy}");

				if (account == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NullPayload (null);
					_logger.LogInformation ($"CreateAccount ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

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
					_logger.LogInformation ($"CreateAccount ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				string customerNumber = customerId.ToString ().PadLeft (6, '0');

				string ledgerNumber = Utility.GenerateLedgerNumber (branchCode, customerNumber, account.AccountType.ToString ()).Trim ();

				long existingLedgerNumberCount = await _context.Accounts.AsNoTracking ().Where (x => x.LedgerNumber == ledgerNumber)
					.LongCountAsync (account.CancellationToken);

				if (existingLedgerNumberCount > 0)
				{
					var failure = RequestResponse<AccountResponse>.AlreadyExists (null, existingLedgerNumberCount, "You are not allowed to create multiple accounts of the same type");
					_logger.LogError ($"CreateAccount failed at {DateTime.UtcNow.AddHours (1)}: {failure.Remark}");
					return failure;
				}

				string? nuban = await GenerateUniqueNUBANAsync (bankCode, customerNumber, account.CancellationToken);

				if (nuban == null)
				{
					var failure = RequestResponse<AccountResponse>.Failed (null, 500, "Unable to generate unique account number");
					_logger.LogError ($"CreateAccount failed at {DateTime.UtcNow.AddHours (1)}: {failure.Remark}");
					return failure;
				}

				var payload = _mapper.Map<Account> (account);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.AccountNumber = nuban;
				payload.AccountStatus = AccountStatus.ActiveTier1;
				payload.LedgerNumber = ledgerNumber;
				payload.MaximumDailyTransferLimitAmount = _appSettings.MaximumDailyTransferLimitAmount;
				payload.MaximumDailyWithdrawalLimitAmount = _appSettings.MaximumDailyWithdrawalLimitAmount;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);
				payload.PublicId = Guid.NewGuid ().ToString ();

				await _context.Accounts.AddAsync (payload, account.CancellationToken);
				await _context.SaveChangesAsync (account.CancellationToken);

				var response = _mapper.Map<AccountResponse> (payload);
				var result = RequestResponse<AccountResponse>.Created (response, 1, "Account");

				_logger.LogInformation ($"CreateAccount ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by UserPublicId: {account.CreatedBy}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateAccount by UserPublicId: {account.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		private async Task<string?> GenerateUniqueNUBANAsync (string bankCode, string customerNumber, CancellationToken cancellationToken)
		{
			try
			{
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
						return newNuban;
					}

					attempts++;
				}

				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GenerateUniqueNUBANAsync exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> DeleteAccountAsync (DeleteAccountCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteAccount begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} for Account with ID: {request.Id}");

				var accountCheck = await _context.Accounts.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (accountCheck == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					_logger.LogInformation ($"DeleteAccount ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

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

					_logger.LogInformation ($"DeleteAccount ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

					return badRequest;
				}

				accountCheck.IsDeleted = true;
				accountCheck.DeletedBy = request.DeletedBy;
				accountCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Accounts.Update (accountCheck);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<AccountResponse>.Deleted (null, 1, "Account");

				_logger.LogInformation ($"DeleteAccount ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {result.Remark}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteAccount by UserPublicId: {request.DeletedBy} for Account with ID: {request.Id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByPublicIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAccountByPublicId begins at {DateTime.UtcNow.AddHours (1)} for account with publicId: {id}");

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.PublicId == id)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					_logger.LogInformation ($"GetAccountByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for id: {id}");

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");
				_logger.LogInformation ($"GetAccountByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountByPublicId for account with publicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByAccountNumberAsync (string accountNumber, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAccountByAccountNumber begins at {DateTime.UtcNow.AddHours (1)} for account number: {accountNumber}");

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.AccountNumber == accountNumber)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					_logger.LogInformation ($"GetAccountByAccountNumber ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for account number: {accountNumber}");

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");
				_logger.LogInformation ($"GetAccountByAccountNumber ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for account number: {accountNumber}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountByAccountNumber for account with account number: {accountNumber} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountByLedgerNumberAsync (string ledgerNumber, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAccountByLedgerNumber begins at {DateTime.UtcNow.AddHours (1)} for ledger number: {ledgerNumber}");

				var result = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.LedgerNumber == ledgerNumber)
					.Select (x => new AccountResponse { PublicId = x.PublicId, AccountNumber = x.AccountNumber, LedgerNumber = x.LedgerNumber, AccountStatus = x.AccountStatus, AccountType = x.AccountType, Balance = x.Balance, MaximumDailyWithdrawalLimitAmount = x.MaximumDailyWithdrawalLimitAmount })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");

					_logger.LogInformation ($"GetAccountByLedgerNumber ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for ledger number: {ledgerNumber}");

					return badRequest;
				}

				var response = RequestResponse<AccountResponse>.SearchSuccessful (result, 1, "Account");
				_logger.LogInformation ($"GetAccountByLedgerNumber ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for ledger number: {ledgerNumber}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountByLedgerNumber for account with ledger number: {ledgerNumber} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<AccountResponse>>> GetAccountsByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAccountByUserId begins at {DateTime.UtcNow.AddHours (1)} for account with userPublicId: {id}");

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
					_logger.LogInformation ($"GetAccountByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {result.Count}");

					return badResponse;
				}

				var count = await _context.Accounts
					.AsNoTracking ()
					.Where (account => account.CreatedBy == id && account.IsDeleted == false)
					.LongCountAsync ();

				var response = RequestResponse<List<AccountResponse>>.SearchSuccessful (result, count, "Accounts");

				_logger.LogInformation ($"GetAccountByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountByUserId for account with userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAccountCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Accounts
					.AsNoTracking ()
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<AccountResponse>.CountSuccessful (null, count, "Account");
				_logger.LogInformation ($"GetAccountCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> GetAccountCountByUserIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAccountCountByUserId for userPublicId: {id} begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Accounts
					.AsNoTracking ()
					.Where (x => x.CreatedBy == id)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<AccountResponse>.CountSuccessful (null, count, "Account");
				_logger.LogInformation ($"GetAccountCountByUserId for userPublicId: {id} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAccountCountByUserId for userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<AccountResponse>> UpdateAccountAsync (AccountDto account)
		{
			try
			{
				_logger.LogInformation ($"UpdateAccount begins at {DateTime.UtcNow.AddHours (1)} for account with publicId: {account.PublicId} by UserPublicId: {account.LastModifiedBy}");

				if (account == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateAccount ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var updateAccountRequest = await _context.Accounts
					.Where (x => x.PublicId == account.PublicId && x.IsDeleted == false)
					.FirstOrDefaultAsync (account.CancellationToken);

				if (updateAccountRequest == null)
				{
					var badRequest = RequestResponse<AccountResponse>.NotFound (null, "Account");
					_logger.LogInformation ($"UpdateAccount ends at {DateTime.UtcNow.AddHours (1)}  with remark: {badRequest.Remark} by UserPublicId: {account.LastModifiedBy} for account with Id: {account.Id}");
					return badRequest;
				}

				long existingLedgerNumberCount = await _context.Accounts.AsNoTracking ().Where (x => x.LedgerNumber == account.LedgerNumber)
					.LongCountAsync (account.CancellationToken);

				if (existingLedgerNumberCount > 0)
				{
					var failure = RequestResponse<AccountResponse>.AlreadyExists (null, existingLedgerNumberCount, "You are not allowed to create multiple accounts of the same type");
					_logger.LogError ($"CreateAccount failed at {DateTime.UtcNow.AddHours (1)}: {failure.Remark}");
					return failure;
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
					_logger.LogInformation ($"UpdateAccount ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by UserPublicId: {account.LastModifiedBy} for account with Id: {account.Id}");
					return badRequest;
				}

				updateAccountRequest.LedgerNumber = account.LedgerNumber;
				updateAccountRequest.AccountStatus = account.AccountStatus;
				updateAccountRequest.MaximumDailyWithdrawalLimitAmount = account.MaximumDailyWithdrawalLimitAmount;
				updateAccountRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateAccountRequest.LastModifiedBy = account.LastModifiedBy;

				_context.Accounts.Update (updateAccountRequest);
				await _context.SaveChangesAsync (account.CancellationToken);

				var result = _mapper.Map<AccountResponse> (updateAccountRequest);
				var response = RequestResponse<AccountResponse>.Updated (result, 1, "Account");
				_logger.LogInformation ($"UpdateAccount at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by UserPublicId: {account.LastModifiedBy} for account with Id: {account.Id}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateAccount for account with publicId: {account.Id} error occurred at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {account.LastModifiedBy} with message: {ex.Message}");
				throw;
			}
		}
	}
}

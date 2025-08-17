using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Models;
using Application.Models.Accounts.Command;
using Application.Models.Accounts.Response;
using Application.Models.AuditLogs.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class AccountRepositoryTests
	{
		private readonly IMapper _mapper;
		private readonly ILogger<AccountRepository> _logger;
		private readonly IAuditLogRepository _auditLogRepo;
		private readonly AppSettings _appSettings;

		public AccountRepositoryTests ()
		{
			// Configure AutoMapper
			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<AccountDto, Account> ();
				cfg.CreateMap<Account, AccountResponse> ();
			});
			_mapper = config.CreateMapper ();

			// Mock logger and audit log
			_logger = new Mock<ILogger<AccountRepository>> ().Object;
			_auditLogRepo = new Mock<IAuditLogRepository> ().Object;

			// AppSettings stub
			_appSettings = new AppSettings
			{
				DefaultBranchCode = 001,
				BankCode = 123,
				MaximumDailyDepositLimitAmount = 100000,
				MaximumDailyWithdrawalLimitAmount = 50000
			};
		}

		private ApplicationDbContext CreateDbContext ()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;

			return new ApplicationDbContext (options);
		}

		[Fact]
		public async Task CreateAccountAsync_ValidAccount_ReturnsCreatedResponse ()
		{
			using var context = CreateDbContext ();

			context.Users.Add (new User
			{
				PublicId = "user123",
				Id = 1,
				Email = "user@gmail.com",
				Password = "Password1!",
				UserRole = UserRoles.Staff,
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));

			var accountDto = new AccountDto
			{
				CreatedBy = "user123",
				AccountType = AccountType.NairaCurrent,
				AccountStatus = AccountStatus.ActiveTier1,
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.CreateAccountAsync (accountDto);

			Assert.True (result.IsSuccessful);
			Assert.Equal (201, result.StatusCode);
			Assert.Equal ("Account creation successful", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal (accountDto.AccountType, result.Data.AccountType);
		}

		[Fact]
		public async Task DeleteAccountAsync_ValidAccount_ReturnsDeletedResponse ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc123",
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				AccountType = AccountType.NairaSaving,
				AccountStatus = AccountStatus.ActiveTier2,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000,
				IsDeleted = false
			});
			await context.SaveChangesAsync ();

			var auditLogMock = new Mock<IAuditLogRepository> ();
			auditLogMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "AuditLog"));

			var repo = new AccountRepository (context, new Mock<IMapper> ().Object, _logger, auditLogMock.Object, Options.Create (_appSettings));

			var request = new DeleteAccountCommand
			{
				Id = "acc123",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteAccountAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Account deleted sucessfully", result.Remark);
		}

		[Fact]
		public async Task DeleteAccountAsync_AccountNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, new Mock<IMapper> ().Object, _logger, _auditLogRepo, Options.Create (_appSettings));

			var request = new DeleteAccountCommand
			{
				Id = "nonexistent",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteAccountAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Account not found", result.Remark);
		}

		[Fact]
		public async Task DeleteAccountAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc123",
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				AccountType = AccountType.NairaSaving,
				AccountStatus = AccountStatus.ActiveTier1,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000,
				IsDeleted = false
			});
			await context.SaveChangesAsync ();

			var auditLogMock = new Mock<IAuditLogRepository> ();
			auditLogMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new AccountRepository (context, new Mock<IMapper> ().Object, _logger, auditLogMock.Object, Options.Create (_appSettings));

			var request = new DeleteAccountCommand
			{
				Id = "acc123",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteAccountAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal (500, result.StatusCode);
			Assert.Equal ("Update failed please try again later", result.Remark);
		}

		[Fact]
		public async Task UpdateAccountAsync_ValidUpdate_ReturnsUpdatedResponse ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc123",
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				AccountType = AccountType.NairaSaving,
				AccountStatus = AccountStatus.ActiveTier1,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000,
				IsDeleted = false
			});
			await context.SaveChangesAsync ();

			var auditLogMock = new Mock<IAuditLogRepository> ();
			auditLogMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "AuditLog"));

			var repo = new AccountRepository (context, _mapper, _logger, auditLogMock.Object, Options.Create (_appSettings));

			var dto = new AccountDto
			{
				PublicId = "acc123",
				LastModifiedBy = "user123",
				LedgerNumber = "LED002",
				AccountStatus = AccountStatus.PostNoDebit,
				MaximumDailyWithdrawalLimitAmount = 50000,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateAccountAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Account update successful", result.Remark);
			Assert.Equal ("LED002", result.Data.LedgerNumber);
		}

		[Fact]
		public async Task GetAccountByPublicIdAsync_ValidId_ReturnsAccount ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc123",
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				AccountStatus = AccountStatus.ActiveTier1,
				AccountType = AccountType.NairaSaving,
				Balance = 1500,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByPublicIdAsync ("acc123", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Account retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal ("acc123", result.Data.PublicId);
			Assert.Equal ("1234567890", result.Data.AccountNumber);
			Assert.Equal ("LED001", result.Data.LedgerNumber);
			Assert.Equal (AccountType.NairaSaving, result.Data.AccountType);
			Assert.Equal (AccountStatus.ActiveTier1, result.Data.AccountStatus);
			Assert.Equal (1500, result.Data.Balance);
		}

		[Fact]
		public async Task GetAccountByPublicIdAsync_InvalidId_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByPublicIdAsync ("invalid", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Account not found", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountByAccountNumberAsync_ValidAccountNumber_ReturnsAccount ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc001",
				AccountNumber = "1234567890",
				LedgerNumber = "LED001",
				AccountStatus = AccountStatus.ActiveTier1,
				AccountType = AccountType.NairaSaving,
				Balance = 1500,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 500
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByAccountNumberAsync ("1234567890", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Account retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal ("acc001", result.Data.PublicId);
			Assert.Equal ("1234567890", result.Data.AccountNumber);
			Assert.Equal ("LED001", result.Data.LedgerNumber);
			Assert.Equal (AccountType.NairaSaving, result.Data.AccountType);
			Assert.Equal (AccountStatus.ActiveTier1, result.Data.AccountStatus);
			Assert.Equal (1500, result.Data.Balance);
		}

		[Fact]
		public async Task GetAccountByAccountNumberAsync_InvalidAccountNumber_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByAccountNumberAsync ("9999999999", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Account not found", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountByLedgerNumberAsync_ValidLedgerNumber_ReturnsAccount ()
		{
			using var context = CreateDbContext ();

			context.Accounts.Add (new Account
			{
				PublicId = "acc002",
				AccountNumber = "9876543210",
				LedgerNumber = "LED002",
				AccountStatus = AccountStatus.ActiveTier1,
				AccountType = AccountType.NairaSaving,
				Balance = 3000,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow,
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 1000
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByLedgerNumberAsync ("LED002", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Account retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal ("acc002", result.Data.PublicId);
			Assert.Equal ("9876543210", result.Data.AccountNumber);
			Assert.Equal ("LED002", result.Data.LedgerNumber);
			Assert.Equal (AccountType.NairaSaving, result.Data.AccountType);
			Assert.Equal (AccountStatus.ActiveTier1, result.Data.AccountStatus);
			Assert.Equal (3000, result.Data.Balance);
		}

		[Fact]
		public async Task GetAccountByLedgerNumberAsync_InvalidLedgerNumber_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByLedgerNumberAsync ("UNKNOWN_LEDGER", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Account not found", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountsByUserIdAsync_ValidUser_ReturnsPagedAccounts ()
		{
			using var context = CreateDbContext ();

			context.Accounts.AddRange (
				new Account
				{
					PublicId = "acc001",
					AccountNumber = "1111111111",
					LedgerNumber = "LED001",
					AccountType = AccountType.NairaSaving,
					AccountStatus = AccountStatus.ActiveTier1,
					CreatedBy = "user123",
					DateCreated = DateTime.UtcNow.AddHours (-1),
					MaximumDailyDepositLimitAmount = 50000,
					MaximumDailyWithdrawalLimitAmount = 25000
				},
				new Account
				{
					PublicId = "acc002",
					AccountNumber = "2222222222",
					LedgerNumber = "LED002",
					AccountType = AccountType.NairaCurrent,
					AccountStatus = AccountStatus.ActiveTier2,
					CreatedBy = "user123",
					DateCreated = DateTime.UtcNow,
					MaximumDailyDepositLimitAmount = 50000,
					MaximumDailyWithdrawalLimitAmount = 25000
				}
			);
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountsByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Accounts retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal (2, result.Data.Count);
			Assert.Equal (2, result.TotalCount);

			Assert.Contains (result.Data, a => a.PublicId == "acc001");
			Assert.Contains (result.Data, a => a.PublicId == "acc002");
		}

		[Fact]
		public async Task GetAccountsByUserIdAsync_NoAccounts_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountsByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountCountAsync_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();

			context.Accounts.AddRange (
				new Account
				{
					PublicId = "acc001",
					AccountNumber = "1111111111",
					LedgerNumber = "LED001",
					AccountType = AccountType.NairaSaving,
					AccountStatus = AccountStatus.ActiveTier1,
					CreatedBy = "user123",
					DateCreated = DateTime.UtcNow,
					MaximumDailyDepositLimitAmount = 50000,
					MaximumDailyWithdrawalLimitAmount = 25000
				},
				new Account
				{
					PublicId = "acc002",
					AccountNumber = "2222222222",
					LedgerNumber = "LED002",
					AccountType = AccountType.NairaCurrent,
					AccountStatus = AccountStatus.ActiveTier2,
					CreatedBy = "user456",
					DateCreated = DateTime.UtcNow,
					MaximumDailyDepositLimitAmount = 50000,
					MaximumDailyWithdrawalLimitAmount = 25000
				}
			);
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetAccountCountByUserIdAsync_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();

			context.Accounts.AddRange (
				new Account { CreatedBy = "user123", PublicId = "acc001", AccountNumber = "111", LedgerNumber = "LED1", AccountType = AccountType.NairaSaving, AccountStatus = AccountStatus.ActiveTier1, MaximumDailyDepositLimitAmount = 50000, MaximumDailyWithdrawalLimitAmount = 25000, DateCreated = DateTime.UtcNow },
				new Account { CreatedBy = "user123", PublicId = "acc002", AccountNumber = "222", LedgerNumber = "LED2", AccountType = AccountType.NairaSaving, AccountStatus = AccountStatus.ActiveTier2, MaximumDailyDepositLimitAmount = 50000, MaximumDailyWithdrawalLimitAmount = 25000, DateCreated = DateTime.UtcNow },
				new Account { CreatedBy = "otherUser", PublicId = "acc003", AccountNumber = "333", LedgerNumber = "LED3", AccountType = AccountType.NairaCurrent, AccountStatus = AccountStatus.ActiveTier3, MaximumDailyDepositLimitAmount = 50000, MaximumDailyWithdrawalLimitAmount = 25000, DateCreated = DateTime.UtcNow }
			);
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountCountByUserIdAsync ("user123", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task UpdateAccountAsync_AccountNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));

			var dto = new AccountDto
			{
				PublicId = "missing",
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateAccountAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Account not found", result.Remark);
		}
	}
}

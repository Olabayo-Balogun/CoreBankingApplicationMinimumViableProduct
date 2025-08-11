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
			// Arrange: In-memory EF Core
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (databaseName: Guid.NewGuid ().ToString ())
				.Options;

			using var context = new ApplicationDbContext (options);

			// Seed user
			context.Users.Add (new User { PublicId = "user123", Id = 1, Email = "user@gmail.com", Password = "Password1!", UserRole = UserRoles.Staff, DateCreated = DateTime.UtcNow });
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (
				context,
				_mapper,
				_logger,
				_auditLogRepo,
				Options.Create (_appSettings)
			);

			var accountDto = new AccountDto
			{
				CreatedBy = "user123",
				AccountType = AccountType.NairaCurrent,
				CancellationToken = CancellationToken.None,
			};

			// Act
			var result = await repo.CreateAccountAsync (accountDto);

			// Assert
			Assert.NotNull (result);
			Assert.True (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal (accountDto.AccountType, result.Data.AccountType);
		}

		[Fact]
		public async Task DeleteAccountAsync_ValidAccount_ReturnsDeletedResponse ()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (databaseName: Guid.NewGuid ().ToString ())
				.Options;

			using var context = new ApplicationDbContext (options);

			var account = new Account
			{
				PublicId = "acc123",
				CreatedBy = "user123",
				IsDeleted = false
			};

			context.Accounts.Add (account);
			await context.SaveChangesAsync ();

			var auditLogMock = new Mock<IAuditLogRepository> ();
			auditLogMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "AuditLog"));

			var repo = new AccountRepository (
				context,
				new Mock<IMapper> ().Object,
				_logger,
				auditLogMock.Object,
				Options.Create (_appSettings)
			);

			var request = new DeleteAccountCommand
			{
				Id = "acc123",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			// Act
			var result = await repo.DeleteAccountAsync (request);

			// Assert
			Assert.NotNull (result);
			Assert.True (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
		}

		[Fact]
		public async Task DeleteAccountAsync_AccountNotFound_ReturnsNotFound ()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (databaseName: Guid.NewGuid ().ToString ())
				.Options;

			using var context = new ApplicationDbContext (options);

			var repo = new AccountRepository (
				context,
				new Mock<IMapper> ().Object,
				_logger,
				_auditLogRepo,
				Options.Create (_appSettings)
			);

			var request = new DeleteAccountCommand
			{
				Id = "nonexistent",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			// Act
			var result = await repo.DeleteAccountAsync (request);

			// Assert
			Assert.False (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Equal (404, result.StatusCode);
		}

		[Fact]
		public async Task DeleteAccountAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (databaseName: Guid.NewGuid ().ToString ())
				.Options;

			using var context = new ApplicationDbContext (options);

			var account = new Account
			{
				PublicId = "acc123",
				CreatedBy = "user123",
				IsDeleted = false
			};

			context.Accounts.Add (account);
			await context.SaveChangesAsync ();

			var auditLogMock = new Mock<IAuditLogRepository> ();
			auditLogMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new AccountRepository (
				context,
				new Mock<IMapper> ().Object,
				_logger,
				auditLogMock.Object,
				Options.Create (_appSettings)
			);

			var request = new DeleteAccountCommand
			{
				Id = "acc123",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			// Act
			var result = await repo.DeleteAccountAsync (request);

			// Assert
			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
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
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByPublicIdAsync ("acc123", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Equal ("acc123", result.Data.PublicId);
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
				MaximumDailyWithdrawalLimitAmount = 500
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByAccountNumberAsync ("1234567890", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Equal ("1234567890", result.Data.AccountNumber);
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
				MaximumDailyWithdrawalLimitAmount = 1000
			});
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByLedgerNumberAsync ("LED002", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Equal ("LED002", result.Data.LedgerNumber);
		}

		[Fact]
		public async Task GetAccountByLedgerNumberAsync_InvalidLedgerNumber_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByLedgerNumberAsync ("UNKNOWN_LEDGER", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountByAccountNumberAsync_InvalidAccountNumber_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByAccountNumberAsync ("9999999999", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Account", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAccountByPublicIdAsync_InvalidId_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountByPublicIdAsync ("invalid", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
		}

		[Fact]
		public async Task GetAccountsByUserIdAsync_ValidUser_ReturnsPagedAccounts ()
		{
			using var context = CreateDbContext ();
			context.Accounts.AddRange (
				new Account { CreatedBy = "user123", DateCreated = DateTime.UtcNow.AddHours (-1) },
				new Account { CreatedBy = "user123", DateCreated = DateTime.UtcNow }
			);
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountsByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetAccountsByUserIdAsync_NoAccounts_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountsByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
		}

		[Fact]
		public async Task GetAccountCountAsync_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Accounts.AddRange (new Account (), new Account ());
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
				new Account { CreatedBy = "user123" },
				new Account { CreatedBy = "user123" },
				new Account { CreatedBy = "otherUser" }
			);
			await context.SaveChangesAsync ();

			var repo = new AccountRepository (context, _mapper, _logger, _auditLogRepo, Options.Create (_appSettings));
			var result = await repo.GetAccountCountByUserIdAsync ("user123", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task UpdateAccountAsync_ValidUpdate_ReturnsUpdatedResponse ()
		{
			using var context = CreateDbContext ();
			var account = new Account { PublicId = "acc123", CreatedBy = "user123", IsDeleted = false };
			context.Accounts.Add (account);
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
			Assert.Equal ("Account", result.Remark);
			Assert.Equal ("LED002", result.Data.LedgerNumber);
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
		}
	}
}

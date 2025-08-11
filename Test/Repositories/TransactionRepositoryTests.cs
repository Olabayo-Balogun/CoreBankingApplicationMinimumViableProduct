using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Transactions.Queries;
using Application.Models.AuditLogs.Response;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Queries;
using Application.Models.Transactions.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class TransactionRepositoryTests
	{
		private readonly Mock<ILogger<TransactionRepository>> _loggerMock;
		private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
		private readonly IMapper _mapper;

		public TransactionRepositoryTests ()
		{
			_loggerMock = new Mock<ILogger<TransactionRepository>> ();
			_auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<Transaction, TransactionResponse> ();
			});
			_mapper = config.CreateMapper ();
		}

		private ApplicationDbContext CreateDbContext ()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;

			return new ApplicationDbContext (options);
		}

		[Fact]
		public async Task GetTransactionsByBankNameAsync_NoTransactions_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No transactions added

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsByBankNameAsync ("NonExistentBank", CancellationToken.None, pageNumber: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsByIdAsync_InvalidPublicId_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching transaction

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsByIdAsync ("invalid-id", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transaction not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionByBankNameAndAccountNumberAsync_NoDepositorMatches_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching sender transactions

			var query = new GetTransactionByBankNameAndAccountNumberQuery
			{
				BankName = "BankX",
				AccountNumber = "1234567890",
				IsDepositor = true,
				IsDeleted = false,
				PageNumber = 1,
				PageSize = 10,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionByBankNameAndAccountNumberAsync (query);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionByBankNameAndAccountNumberAsync_NoRecipientMatches_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching recipient transactions

			var query = new GetTransactionByBankNameAndAccountNumberQuery
			{
				BankName = "BankY",
				AccountNumber = "9876543210",
				IsDepositor = false,
				IsDeleted = false,
				PageNumber = 1,
				PageSize = 10,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionByBankNameAndAccountNumberAsync (query);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task CreateTransactionAsync_NullPayload_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.CreateTransactionAsync (null);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payload is null", result.Remark);
		}

		[Fact]
		public async Task CreateTransactionAsync_InvalidType_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();
			var dto = new TransactionDto
			{
				TransactionType = "Transfer", // Invalid
				CreatedBy = "user123",
				Amount = 1000,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.CreateTransactionAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Specify transaction type as either debit or credit", result.Remark);
		}

		[Fact]
		public async Task CreateTransactionAsync_ValidCreditTransaction_ReturnsCreated ()
		{
			using var context = CreateDbContext ();
			var dto = new TransactionDto
			{
				TransactionType = "Credit",
				CreatedBy = "user123",
				Amount = 5000,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.CreateTransactionAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Transaction", result.Remark);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task UpdateTransactionAsync_NullPayload_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateTransactionAsync (null);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payload is null", result.Remark);
		}

		[Fact]
		public async Task UpdateTransactionAsync_TransactionNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var dto = new TransactionDto
			{
				PublicId = "nonexistent-id",
				LastModifiedBy = "user123",
				Amount = 1000,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateTransactionAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transaction not found", result.Remark);
		}

		[Fact]
		public async Task UpdateTransactionAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			var transaction = new Transaction
			{
				PublicId = "tx123",
				IsDeleted = false,
				CreatedBy = "user123",
				Amount = 500,
				DateCreated = DateTime.UtcNow
			};
			context.Transactions.Add (transaction);
			await context.SaveChangesAsync ();

			var dto = new TransactionDto
			{
				PublicId = "tx123",
				LastModifiedBy = "user123",
				Amount = 1000,
				CancellationToken = CancellationToken.None
			};

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateTransactionAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Audit log creation failed", result.Remark);
		}

		[Fact]
		public async Task ConfirmTransactionAsync_NullPayload_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ConfirmTransactionAsync (null);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payload is null", result.Remark);
		}

		[Fact]
		public async Task ConfirmTransactionAsync_TransactionNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new ConfirmTransactionCommand
			{
				PaymentReferenceId = "ref123",
				Amount = 1000,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ConfirmTransactionAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transaction not found", result.Remark);
		}

		[Fact]
		public async Task ConfirmTransactionAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			var transaction = new Transaction
			{
				PaymentReferenceId = "ref123",
				IsDeleted = false,
				TransactionType = "Credit",
				RecipientAccountNumber = "1234567890",
				CreatedBy = "user123",
				Amount = 1000,
				DateCreated = DateTime.UtcNow
			};
			context.Transactions.Add (transaction);
			await context.SaveChangesAsync ();

			var command = new ConfirmTransactionCommand
			{
				PaymentReferenceId = "ref123",
				Amount = 1000,
				CancellationToken = CancellationToken.None
			};

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ConfirmTransactionAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Audit log creation failed", result.Remark);
		}

		[Fact]
		public async Task DeleteTransactionAsync_NullPayload_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.DeleteTransactionAsync (null);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payload is null", result.Remark);
		}

		[Fact]
		public async Task DeleteTransactionAsync_TransactionNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new DeleteTransactionCommand
			{
				PublicId = "nonexistent-id",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.DeleteTransactionAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transaction not found", result.Remark);
		}

		[Fact]
		public async Task DeleteTransactionAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Transactions.Add (new Transaction
			{
				PublicId = "tx123",
				IsDeleted = false,
				CreatedBy = "user123",
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var command = new DeleteTransactionCommand
			{
				PublicId = "tx123",
				DeletedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.DeleteTransactionAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Audit log creation failed", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsByAmountPaidAsync_NoMatches_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching transactions

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsByAmountPaidAsync (9999, CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsByUserIdAsync_NoMatches_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching transactions

			var query = new GetTransactionByUserIdQuery
			{
				UserId = "user123",
				IsDeleted = false,
				PageNumber = 1,
				PageSize = 10,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsByUserIdAsync (query);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetAllTransactionsAsync_NoResults_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No transactions added

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllTransactionsAsync (isDeleted: false, CancellationToken.None, pageNumber: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsByDateAsync_NoMatches_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching transactions

			var query = new GetTransactionsByDateQuery
			{
				Date = DateTime.UtcNow.Date,
				PageNumber = 1,
				PageSize = 10,
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsByDateAsync (query);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions not found", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsCountByCustomDateAsync_NoMatches_ReturnsZeroCount ()
		{
			using var context = CreateDbContext (); // No matching transactions

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetTransactionsCountByCustomDateAsync (
				userId: "user123",
				fromDate: DateTime.UtcNow.AddDays (-7),
				toDate: DateTime.UtcNow,
				cancellationToken: CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
			Assert.Equal ("Transaction", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsCountByDateAsync_NoTransactions_ReturnsZero ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsCountByDateAsync ("user123", DateTime.UtcNow.Date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
			Assert.Equal ("Transaction", result.Remark);
		}

		[Fact]
		public async Task GetTransactionsCountByWeekAsync_NoTransactionsInWeek_ReturnsZero ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsCountByWeekAsync ("user123", DateTime.UtcNow.Date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByMonthAsync_NoTransactionsInMonth_ReturnsZero ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsCountByMonthAsync ("user123", DateTime.UtcNow.Date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByYearAsync_NoTransactionsInYear_ReturnsZero ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsCountByYearAsync ("user123", DateTime.UtcNow.Date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByCustomDateAsync_ReturnsTransactionsWithinDateRange ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var userId = "user123";
			var fromDate = DateTime.UtcNow.AddDays (-5);
			var toDate = DateTime.UtcNow;

			context.Transactions.AddRange (
				new Transaction { CreatedBy = userId, DateCreated = DateTime.UtcNow.AddDays (-4), IsDeleted = false, Amount = 100, Description = "Test 1" },
				new Transaction { CreatedBy = userId, DateCreated = DateTime.UtcNow.AddDays (-2), IsDeleted = false, Amount = 200, Description = "Test 2" },
				new Transaction { CreatedBy = "otherUser", DateCreated = DateTime.UtcNow.AddDays (-3), IsDeleted = false, Amount = 300, Description = "Should be excluded" }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByCustomDateAsync (userId, fromDate, toDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetTransactionByDateAsync_ReturnsTransactionsForSpecificDate ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var userId = "user123";
			var targetDate = DateTime.UtcNow.Date;

			context.Transactions.AddRange (
				new Transaction { CreatedBy = userId, DateCreated = targetDate, IsDeleted = false, Amount = 150, Description = "Today’s transaction" },
				new Transaction { CreatedBy = userId, DateCreated = targetDate.AddDays (-1), IsDeleted = false, Amount = 250, Description = "Yesterday’s transaction" }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionByDateAsync (userId, targetDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal ("Today’s transaction", result.Data.First ().Description);
		}

		[Fact]
		public async Task GetTransactionsByWeekAsync_ReturnsTransactionsInWeek ()
		{
			using var context = CreateDbContext ();

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var userId = "user123";
			var referenceDate = new DateTime (2024, 8, 7); // Wednesday

			var startOfWeek = referenceDate.AddDays (-1 * (int)referenceDate.DayOfWeek);
			var endOfWeek = startOfWeek.AddDays (7);

			context.Transactions.AddRange (
				new Transaction { CreatedBy = userId, DateCreated = startOfWeek.AddDays (1), IsDeleted = false, Amount = 100 },
				new Transaction { CreatedBy = userId, DateCreated = endOfWeek.AddDays (-1), IsDeleted = false, Amount = 200 },
				new Transaction { CreatedBy = userId, DateCreated = endOfWeek.AddDays (1), IsDeleted = false, Amount = 300 } // Outside week
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByWeekAsync (userId, referenceDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetTransactionsByMonthAsync_ReturnsTransactionsForGivenMonth ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var userId = "user123";
			var targetDate = new DateTime (2024, 8, 15); // August

			context.Transactions.AddRange (
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2024, 8, 1), IsDeleted = false, Amount = 100 },
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2024, 8, 20), IsDeleted = false, Amount = 200 },
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2024, 7, 31), IsDeleted = false, Amount = 300 } // Outside month
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByMonthAsync (userId, targetDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetTransactionsByYearAsync_ReturnsTransactionsForGivenYear ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var userId = "user123";
			var targetDate = new DateTime (2024, 1, 1); // Year 2024

			context.Transactions.AddRange (
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2024, 3, 15), IsDeleted = false, Amount = 150 },
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2024, 11, 5), IsDeleted = false, Amount = 250 },
				new Transaction { CreatedBy = userId, DateCreated = new DateTime (2023, 12, 31), IsDeleted = false, Amount = 350 } // Outside year
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByYearAsync (userId, targetDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task FlagTransactionAsync_ValidTransaction_FlagsSuccessfully ()
		{
			// Arrange
			using var context = CreateDbContext ();
			var transaction = new Transaction
			{
				PublicId = "txn123",
				CreatedBy = "user1",
				IsDeleted = false
			};
			context.Transactions.Add (transaction);
			await context.SaveChangesAsync ();

			var command = new FlagTransactionCommand
			{
				PublicId = "txn123",
				LastModifiedBy = "admin",
				CancellationToken = CancellationToken.None
			};

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			// Act
			var result = await repo.FlagTransactionAsync (command);

			// Assert
			Assert.True (result.IsSuccessful);
			var updatedTransaction = context.Transactions.First (t => t.PublicId == "txn123");
			Assert.True (updatedTransaction.IsFlagged);
			Assert.Equal ("admin", updatedTransaction.LastModifiedBy);
		}

		[Fact]
		public async Task FlagTransactionAsync_NullCommand_ReturnsNullPayload ()
		{
			// Arrange
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			// Act
			var result = await repo.FlagTransactionAsync (null);

			// Assert
			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task FlagTransactionAsync_TransactionNotFound_ReturnsNotFound ()
		{
			// Arrange
			using var context = CreateDbContext ();
			var command = new FlagTransactionCommand
			{
				PublicId = "nonexistent",
				LastModifiedBy = "admin",
				CancellationToken = CancellationToken.None
			};

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			// Act
			var result = await repo.FlagTransactionAsync (command);

			// Assert
			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task FlagTransactionAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			// Arrange
			using var context = CreateDbContext ();
			var transaction = new Transaction
			{
				PublicId = "txn123",
				CreatedBy = "user1",
				IsDeleted = false
			};
			context.Transactions.Add (transaction);
			await context.SaveChangesAsync ();

			var command = new FlagTransactionCommand
			{
				PublicId = "txn123",
				LastModifiedBy = "admin",
				CancellationToken = CancellationToken.None
			};

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			// Act
			var result = await repo.FlagTransactionAsync (command);

			// Assert
			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task GetTransactionsCountByAccountNumberAndDateAsync_ReturnsCorrectTotalCount ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var fromDate = DateTime.UtcNow.Date.AddDays (-2);
			var toDate = DateTime.UtcNow.Date;

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = fromDate, IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = toDate, IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = toDate.AddDays (1), IsDeleted = false } // outside range
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsCountByAccountNumberAndDateAsync (account, fromDate, toDate, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByAccountNumberAndDateAsync_SingleDate_ReturnsCorrectTotalCount ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var date = DateTime.UtcNow.Date;

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = date, IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = date.AddDays (-1), IsDeleted = false }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsCountByAccountNumberAndDateAsync (account, date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByAccountNumberAndWeekAsync_ReturnsCorrectTotalCount ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var referenceDate = new DateTime (2024, 8, 7); // Wednesday

			var startOfWeek = referenceDate.AddDays (-1 * (int)referenceDate.DayOfWeek);
			var endOfWeek = startOfWeek.AddDays (7);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = startOfWeek.AddDays (1), IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = endOfWeek.AddDays (1), IsDeleted = false } // outside week
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsCountByAccountNumberAndWeekAsync (account, referenceDate, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByAccountNumberAndMonthAsync_ReturnsCorrectTotalCount ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var date = new DateTime (2024, 8, 15);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 8, 1), IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 7, 31), IsDeleted = false }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsCountByAccountNumberAndMonthAsync (account, date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsCountByAccountNumberAndYearAsync_ReturnsCorrectTotalCount ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var date = new DateTime (2024, 5, 10);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 1, 1), IsDeleted = false },
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2023, 12, 31), IsDeleted = false }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsCountByAccountNumberAndYearAsync (account, date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndCustomDateAsync_ReturnsCorrectResults ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC123";
			var fromDate = DateTime.UtcNow.Date.AddDays (-2);
			var toDate = DateTime.UtcNow.Date;

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = fromDate, IsDeleted = false, Amount = 100 },
				new Transaction { RecipientAccountNumber = account, DateCreated = toDate, IsDeleted = false, Amount = 200 },
				new Transaction { RecipientAccountNumber = account, DateCreated = toDate.AddDays (1), IsDeleted = false, Amount = 300 } // outside range
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByAccountNumberAndCustomDateAsync (account, fromDate, toDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetTransactionByAccountNumberAndDateAsync_ReturnsCorrectResults ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC456";
			var date = DateTime.UtcNow.Date;

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = date, IsDeleted = false, Amount = 150 },
				new Transaction { RecipientAccountNumber = account, DateCreated = date.AddDays (-1), IsDeleted = false, Amount = 250 }
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionByAccountNumberAndDateAsync (account, date, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndWeekAsync_ReturnsCorrectResults ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC789";
			var referenceDate = new DateTime (2024, 8, 7); // Wednesday
			var startOfWeek = referenceDate.AddDays (-1 * (int)referenceDate.DayOfWeek);
			var endOfWeek = startOfWeek.AddDays (7);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = startOfWeek.AddDays (1), IsDeleted = false, Amount = 300 },
				new Transaction { RecipientAccountNumber = account, DateCreated = endOfWeek.AddDays (1), IsDeleted = false, Amount = 400 } // outside week
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByAccountNumberAndWeekAsync (account, referenceDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndCustomDateAsync_ReturnsNotFound_WhenNoTransactionsExist ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsByAccountNumberAndCustomDateAsync ("NON_EXISTENT", DateTime.UtcNow.AddDays (-5), DateTime.UtcNow, CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions", result.Remark);
			Assert.Equal (0, result.TotalCount);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndMonthAsync_ReturnsCorrectResults ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC321";
			var targetDate = new DateTime (2024, 8, 15);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 8, 1), IsDeleted = false, Amount = 100 },
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 7, 31), IsDeleted = false, Amount = 200 } // outside month
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByAccountNumberAndMonthAsync (account, targetDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndYearAsync_ReturnsCorrectResults ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var account = "ACC654";
			var targetDate = new DateTime (2024, 5, 10);

			context.Transactions.AddRange (
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2024, 1, 1), IsDeleted = false, Amount = 300 },
				new Transaction { RecipientAccountNumber = account, DateCreated = new DateTime (2023, 12, 31), IsDeleted = false, Amount = 400 } // outside year
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetTransactionsByAccountNumberAndYearAsync (account, targetDate, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndMonthAsync_ReturnsNotFound_WhenNoTransactionsExist ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsByAccountNumberAndMonthAsync ("NO_MATCH", new DateTime (2024, 8, 1), CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions", result.Remark);
			Assert.Equal (0, result.TotalCount);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetTransactionsByAccountNumberAndYearAsync_ReturnsNotFound_WhenNoTransactionsExist ()
		{
			using var context = CreateDbContext ();
			var repo = new TransactionRepository (context, _mapper, _loggerMock.Object, null);

			var result = await repo.GetTransactionsByAccountNumberAndYearAsync ("NO_MATCH", new DateTime (2024, 1, 1), CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Transactions", result.Remark);
			Assert.Equal (0, result.TotalCount);
			Assert.Null (result.Data);
		}
	}
}

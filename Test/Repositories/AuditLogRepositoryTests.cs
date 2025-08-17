using Application.Model.AuditLogs.Command;
using Application.Models.Accounts.Response;
using Application.Models.Banks.Response;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.Entities;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Newtonsoft.Json;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class AuditLogRepositoryTests
	{
		private readonly Mock<ILogger<AuditLogRepository>> _logger;
		private readonly IMapper _mapper;

		public AuditLogRepositoryTests ()
		{
			_logger = new Mock<ILogger<AuditLogRepository>> ();

			var config = new MapperConfiguration (cfg => { });
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
		public async Task CreateAuditLogAsync_ValidAccountDetailRequest_ReturnsCreatedResponse ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var accountDetail = new AccountResponse
			{
				PublicId = "acc123",
				AccountNumber = "1234567890",
				AccountType = AccountType.NairaSaving,
				AccountStatus = AccountStatus.ActiveTier1,
				LedgerNumber = "LED001",
				MaximumDailyDepositLimitAmount = 50000,
				MaximumDailyWithdrawalLimitAmount = 25000
			};

			var payload = JsonConvert.SerializeObject (accountDetail);

			var request = new CreateAuditLogCommand
			{
				Name = "Account",
				Payload = payload,
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.CreateAuditLogAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal (201, result.StatusCode);
			Assert.Equal ("Audit log creation successful", result.Remark);
			Assert.NotNull (result.Data);
			Assert.NotNull (result.Data.AccountLog);
			Assert.Equal ("1234567890", result.Data.AccountLog.AccountNumber);
			Assert.Equal ("acc123", result.Data.AccountLog.PublicId);
		}

		[Fact]
		public async Task CreateAuditLogAsync_UnknownName_SkipsDeserialization ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var request = new CreateAuditLogCommand
			{
				Name = "UnknownType",
				Payload = "{}",
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.CreateAuditLogAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal (201, result.StatusCode);
			Assert.Equal ("Audit log creation successful", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Null (result.Data.AccountLog);
			Assert.Null (result.Data.BankLog);
			Assert.Null (result.Data.BranchLog);
			Assert.Null (result.Data.TransactionLog);
			Assert.Null (result.Data.UploadLog);
			Assert.Null (result.Data.UserLog);
		}

		[Fact]
		public async Task CreateMultipleAuditLogAsync_ValidRequests_ReturnsCreatedResponse ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var requests = new List<CreateAuditLogCommand>
			{
				new()
				{
					Name = "Account",
					Payload = JsonConvert.SerializeObject(new AccountResponse { AccountNumber = "123" }),
					CreatedBy = "user1",
					CancellationToken = CancellationToken.None
				},
				new()
				{
					Name = "Bank",
					Payload = JsonConvert.SerializeObject(new BankResponse { Name = "Zenith" }),
					CreatedBy = "user1",
					CancellationToken = CancellationToken.None
				}
			};

			var result = await repo.CreateMultipleAuditLogAsync (requests);

			Assert.True (result.IsSuccessful);
			Assert.Equal (201, result.StatusCode);
			Assert.Equal ("Audit logs creation successful", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal (2, result.Data.AccountLogs.Count + result.Data.BankLogs.Count);
			Assert.Equal ("123", result.Data.AccountLogs.First ().AccountNumber);
			Assert.Equal ("Zenith", result.Data.BankLogs.First ().Name);
		}

		[Fact]
		public async Task CreateMultipleAuditLogAsync_UnknownName_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var requests = new List<CreateAuditLogCommand>
	{
		new()
		{
			Name = "UnknownType", // ❌ Not a valid name
			Payload = "{}",
			CreatedBy = "user1",
			CancellationToken = CancellationToken.None
		}
	};

			var result = await repo.CreateMultipleAuditLogAsync (requests);

			Assert.False (result.IsSuccessful); // ✅ Should fail
			Assert.Equal (400, result.StatusCode); // ✅ Matches repository logic
			Assert.Equal ("Please enter valid details", result.Remark); // ✅ Expected message
			Assert.Null (result.Data); // ✅ No logs returned
		}


		[Fact]
		public async Task GetAuditLogByIdAsync_IdNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var result = await repo.GetAuditLogByIdAsync ("nonexistent-id", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Audit log not found", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAuditLogByIdAsync_ValidId_ReturnsDeserializedPayload ()
		{
			using var context = CreateDbContext ();
			var auditLog = new AuditLog
			{
				PublicId = "test-id",
				Name = "Account",
				Payload = JsonConvert.SerializeObject (new AccountResponse { AccountNumber = "999" }),
				IsDeleted = false,
				CreatedBy = "user123"
			};
			context.AuditLogs.Add (auditLog);
			await context.SaveChangesAsync ();

			var repo = new AuditLogRepository (_logger.Object, context, _mapper);
			var result = await repo.GetAuditLogByIdAsync ("test-id", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Audit log retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
			Assert.Equal ("999", result.Data.AccountLog.AccountNumber);
		}

		[Fact]
		public async Task GetAuditLogsAsync_NoFilters_ReturnsAllLogs ()
		{
			using var context = CreateDbContext ();
			context.AuditLogs.AddRange (new[]
			{
		new AuditLog
		{
			PublicId = Guid.NewGuid().ToString(),
			Name = "Bank",
			Payload = JsonConvert.SerializeObject(new BankResponse { Name = "GTBank" }),
			CreatedBy = "user1",
			IsDeleted = false,
			DateCreated = DateTime.UtcNow
		},
		new AuditLog
		{
			PublicId = Guid.NewGuid().ToString(),
			Name = "User",
			Payload = JsonConvert.SerializeObject(new UserResponse { FirstName = "olabayo" }),
			CreatedBy = "user2",
			IsDeleted = false,
			DateCreated = DateTime.UtcNow
		}
	});
			await context.SaveChangesAsync ();

			var repo = new AuditLogRepository (_logger.Object, context, _mapper);
			var result = await repo.GetAuditLogsAsync (null, null, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Audit logs retrieved successfully", result.Remark);
			Assert.Equal (2, result.TotalCount);
			Assert.Single (result.Data.BankLogs);
			Assert.Single (result.Data.UserLogs);
		}

		[Fact]
		public async Task GetAuditLogsAsync_FilterByUserId_ReturnsUserLogs ()
		{
			using var context = CreateDbContext ();
			context.AuditLogs.Add (new AuditLog
			{
				PublicId = Guid.NewGuid ().ToString (),
				Name = "User",
				Payload = JsonConvert.SerializeObject (new UserResponse { FirstName = "olabayo" }),
				CreatedBy = "user123",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			var repo = new AuditLogRepository (_logger.Object, context, _mapper);
			var result = await repo.GetAuditLogsAsync ("user123", null, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Audit logs retrieved successfully", result.Remark);
			Assert.Single (result.Data.UserLogs);
			Assert.Equal ("olabayo", result.Data.UserLogs.First ().FirstName);
		}

		[Fact]
		public async Task GetAuditLogsAsync_FilterByLogName_ReturnsMatchingLogs ()
		{
			using var context = CreateDbContext ();
			context.AuditLogs.Add (new AuditLog
			{
				PublicId = Guid.NewGuid ().ToString (),
				Name = "Bank",
				Payload = JsonConvert.SerializeObject (new BankResponse { Name = "Access" }),
				CreatedBy = "user456",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			var repo = new AuditLogRepository (_logger.Object, context, _mapper);
			var result = await repo.GetAuditLogsAsync (null, "Bank", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (200, result.StatusCode);
			Assert.Equal ("Audit logs retrieved successfully", result.Remark);
			Assert.Single (result.Data.BankLogs);
			Assert.Equal ("Access", result.Data.BankLogs.First ().Name);
		}

		[Fact]
		public async Task GetAuditLogsAsync_InvalidFilters_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var result = await repo.GetAuditLogsAsync ("nonexistent-user", "NonexistentType", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal (404, result.StatusCode);
			Assert.Equal ("Audit logs not found", result.Remark);
			Assert.Null (result.Data);
		}


		[Fact]
		public async Task GetAuditLogsAsync_NoMatchingLogs_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new AuditLogRepository (_logger.Object, context, _mapper);

			var result = await repo.GetAuditLogsAsync ("nonexistent-user", "NonexistentType", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Audit logs not found", result.Remark);
		}

	}
}

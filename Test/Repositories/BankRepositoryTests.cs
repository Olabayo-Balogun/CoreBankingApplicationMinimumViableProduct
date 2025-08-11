using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Banks.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Banks.Response;

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
	public class BankRepositoryTests
	{
		private readonly Mock<ILogger<BankRepository>> _loggerMock;
		private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
		private readonly IMapper _mapper;

		public BankRepositoryTests ()
		{
			_loggerMock = new Mock<ILogger<BankRepository>> ();
			_auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<BankDto, Bank> ();
				cfg.CreateMap<Bank, BankResponse> ();
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
		public async Task CreateBankAsync_NullBank_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.CreateBankAsync (null);

			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task CreateBankAsync_DuplicateBank_ReturnsAlreadyExists ()
		{
			using var context = CreateDbContext ();
			context.Banks.Add (new Bank { Name = "Zenith", IsDeleted = false });
			await context.SaveChangesAsync ();

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BankDto { Name = "Zenith", CreatedBy = "user1", CancellationToken = CancellationToken.None };
			var result = await repo.CreateBankAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task CreateBankAsync_ValidBank_ReturnsCreated ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BankDto { Name = "Access", CreatedBy = "user1", CancellationToken = CancellationToken.None };
			var result = await repo.CreateBankAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
			Assert.Equal ("Access", result.Data.Name);
		}

		[Fact]
		public async Task DeleteBankAsync_BankNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var request = new DeleteBankCommand { Id = 999, DeletedBy = "user1", CancellationToken = CancellationToken.None };
			var result = await repo.DeleteBankAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task DeleteBankAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			var bank = new Bank { Id = 1, Name = "GTBank", CreatedBy = "user1", IsDeleted = false };
			context.Banks.Add (bank);
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new DeleteBankCommand { Id = 1, DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteBankAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task DeleteBankAsync_ValidRequest_ReturnsDeleted ()
		{
			using var context = CreateDbContext ();
			var bank = new Bank { Id = 1, Name = "GTBank", CreatedBy = "user1", IsDeleted = false };
			context.Banks.Add (bank);
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new DeleteBankCommand { Id = 1, DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteBankAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task GetBankByPublicIdAsync_NotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetBankByPublicIdAsync (999, CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task GetBankByPublicIdAsync_Found_ReturnsBank ()
		{
			using var context = CreateDbContext ();
			context.Banks.Add (new Bank { Id = 1, Name = "UBA", CbnCode = "001", NibssCode = "002" });
			await context.SaveChangesAsync ();

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBankByPublicIdAsync (1, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("UBA", result.Data.Name);
		}

		[Fact]
		public async Task GetBanksByUserIdAsync_NoBanks_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetBanksByUserIdAsync ("userX", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task GetBanksByUserIdAsync_ValidUser_ReturnsBanks ()
		{
			using var context = CreateDbContext ();
			context.Banks.Add (new Bank { Id = 1, Name = "Fidelity", CreatedBy = "user123", IsDeleted = false, DateCreated = DateTime.UtcNow });
			await context.SaveChangesAsync ();

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBanksByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal ("Fidelity", result.Data.First ().Name);
		}


		[Fact]
		public async Task GetBankCountAsync_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Banks.AddRange (
				new Bank { Name = "Zenith" },
				new Bank { Name = "GTBank" }
			);
			await context.SaveChangesAsync ();

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBankCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task GetBankCountByUserIdAsync_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Banks.AddRange (
				new Bank { Name = "Zenith", CreatedBy = "user1" },
				new Bank { Name = "GTBank", CreatedBy = "user2" }
			);
			await context.SaveChangesAsync ();

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBankCountByUserIdAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task UpdateBankAsync_NullBank_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateBankAsync (null);

			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task UpdateBankAsync_BankNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BankDto { Id = 999, LastModifiedBy = "user1", CancellationToken = CancellationToken.None };
			var result = await repo.UpdateBankAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
		}

		[Fact]
		public async Task UpdateBankAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Banks.Add (new Bank { Id = 1, Name = "Access", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BankDto
			{
				Id = 1,
				Name = "Access Updated",
				CbnCode = "001",
				NibssCode = "002",
				LastModifiedBy = "user2",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateBankAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task UpdateBankAsync_ValidUpdate_ReturnsUpdated ()
		{
			using var context = CreateDbContext ();
			context.Banks.Add (new Bank { Id = 1, Name = "Access", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

			var repo = new BankRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BankDto
			{
				Id = 1,
				Name = "Access Updated",
				CbnCode = "001",
				NibssCode = "002",
				LastModifiedBy = "user2",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateBankAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Bank", result.Remark);
			Assert.Equal ("Access Updated", result.Data.Name);
		}
	}

}

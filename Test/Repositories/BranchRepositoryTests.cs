using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Branches.Command;
using Application.Models.Branches.Response;

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
	public class BranchRepositoryTests
	{
		private readonly Mock<ILogger<BranchRepository>> _loggerMock;
		private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
		private readonly IMapper _mapper;

		public BranchRepositoryTests ()
		{
			_loggerMock = new Mock<ILogger<BranchRepository>> ();
			_auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<BranchDto, Branch> ();
				cfg.CreateMap<Branch, BranchResponse> ();
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
		public async Task CreateBranchAsync_NullBranch_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.CreateBranchAsync (null);

			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task CreateBranchAsync_DuplicateBranch_ReturnsAlreadyExists ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { Name = "Main", IsDeleted = false });
			await context.SaveChangesAsync ();

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var dto = new BranchDto { Name = "Main", CreatedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.CreateBranchAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task CreateBranchAsync_ValidBranch_ReturnsCreated ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var dto = new BranchDto { Name = "NewBranch", CreatedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.CreateBranchAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
			Assert.Equal ("NewBranch", result.Data.Name);
		}

		[Fact]
		public async Task DeleteBranchAsync_BranchNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new DeleteBranchCommand { Id = "nonexistent", DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteBranchAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task DeleteBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new DeleteBranchCommand { Id = "branch1", DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteBranchAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task DeleteBranchAsync_ValidRequest_ReturnsDeleted ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new DeleteBranchCommand { Id = "branch1", DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteBranchAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task CloseBranchAsync_BranchNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new CloseBranchCommand { Id = "missing", LastModifiedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.CloseBranchAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task CloseBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch2", Name = "Branch2", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new CloseBranchCommand { Id = "branch2", LastModifiedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.CloseBranchAsync (request);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task CloseBranchAsync_ValidRequest_ReturnsDeleted ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch2", Name = "Branch2", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var request = new CloseBranchCommand { Id = "branch2", LastModifiedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.CloseBranchAsync (request);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task GetBranchByPublicIdAsync_NotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetBranchByPublicIdAsync ("missing", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task GetBranchByPublicIdAsync_Found_ReturnsBranch ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1" });
			await context.SaveChangesAsync ();

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBranchByPublicIdAsync ("branch1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Branch1", result.Data.Name);
		}

		[Fact]
		public async Task GetBranchesByUserIdAsync_NoBranches_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetBranchesByUserIdAsync ("userX", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task GetBranchesByUserIdAsync_Found_ReturnsBranches ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1", CreatedBy = "user123", IsDeleted = false, DateCreated = DateTime.UtcNow });
			await context.SaveChangesAsync ();

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBranchesByUserIdAsync ("user123", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal ("Branch1", result.Data.First ().Name);
		}

		[Fact]
		public async Task GetBranchCountAsync_ReturnsCount ()
		{
			using var context = CreateDbContext ();
			context.Branches.AddRange (
				new Branch { Name = "BranchA" },
				new Branch { Name = "BranchB" }
			);
			await context.SaveChangesAsync ();

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBranchCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetBranchCountByUserIdAsync_ReturnsCount ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { Name = "BranchA", CreatedBy = "user1" });
			await context.SaveChangesAsync ();

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var result = await repo.GetBranchCountByUserIdAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task UpdateBranchAsync_NullBranch_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateBranchAsync (null);

			Assert.False (result.IsSuccessful);
		}

		[Fact]
		public async Task UpdateBranchAsync_BranchNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

			var dto = new BranchDto { PublicId = "missing", LastModifiedBy = "user1", CancellationToken = CancellationToken.None };
			var result = await repo.UpdateBranchAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
		}

		[Fact]
		public async Task UpdateBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var dto = new BranchDto { PublicId = "branch1", Name = "UpdatedBranch", LastModifiedBy = "user2", CancellationToken = CancellationToken.None };

			var result = await repo.UpdateBranchAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task UpdateBranchAsync_ValidUpdate_ReturnsUpdated ()
		{
			using var context = CreateDbContext ();
			context.Branches.Add (new Branch { PublicId = "branch1", Name = "Branch1", CreatedBy = "user1", IsDeleted = false });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

			var repo = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
			var dto = new BranchDto
			{
				PublicId = "branch1",
				Name = "UpdatedBranch",
				State = "Lagos",
				Address = "123 Street",
				Code = 001,
				Country = "Nigeria",
				Lga = "Ikeja",
				LastModifiedBy = "user2",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateBranchAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Branch", result.Remark);
			Assert.Equal ("UpdatedBranch", result.Data.Name);
		}

	}
}

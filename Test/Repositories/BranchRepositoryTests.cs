using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
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
        public async Task CreateBranchAsync_WhenValid_ReturnsCreated ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "branch001",
                Name = "NewBranch",
                Code = 101,
                Address = "123 Main Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CreateBranchAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch creation successful", result.Remark);
            Assert.Equal ("NewBranch", result.Data.Name);
            Assert.Equal ("branch001", result.Data.PublicId);
        }


        [Fact]
        public async Task CreateBranchAsync_WhenDuplicate_ReturnsAlreadyExists ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch001",
                Name = "Main",
                Code = 101,
                Address = "123 Main Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "branch001",
                Name = "Main",
                Code = 101,
                Address = "123 Main Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CreateBranchAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch already exists", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteBranchAsync_WhenNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "nonexistent",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task DeleteBranchAsync_WhenAuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "123 Main Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "branch1",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Update failed please try again later", result.Remark);
        }

        [Fact]
        public async Task DeleteBranchAsync_WhenValid_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "123 Main Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "branch1",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch deleted sucessfully", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_WhenNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "missing",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_WhenAuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch2",
                Name = "Branch2",
                Code = 102,
                Address = "456 Market Road",
                Lga = "Surulere",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "branch2",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Update failed please try again later", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_WhenValid_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch2",
                Name = "Branch2",
                Code = 102,
                Address = "456 Market Road",
                Lga = "Surulere",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "branch2",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch deleted sucessfully", result.Remark);
        }

        [Fact]
        public async Task GetBranchByPublicIdAsync_WhenNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchByPublicIdAsync ("missing", CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task GetBranchByPublicIdAsync_WhenFound_ReturnsBranch ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 100,
                Address = "10 Adeola Odeku",
                Lga = "Eti-Osa",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchByPublicIdAsync ("branch1", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch1", result.Data.Name);
        }

        [Fact]
        public async Task CreateBranchAsync_DuplicateBranch_ReturnsAlreadyExists ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "main001",
                Name = "Main",
                Code = 101,
                Address = "1 Broad Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "main001",
                Name = "Main",
                Code = 101,
                Address = "1 Broad Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CreateBranchAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch already exists", result.Remark);
        }

        [Fact]
        public async Task CreateBranchAsync_ValidBranch_ReturnsCreated ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "new001",
                Name = "NewBranch",
                Code = 102,
                Address = "2 Unity Road",
                Lga = "Alimosho",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CreateBranchAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch creation successful", result.Remark);
            Assert.Equal ("NewBranch", result.Data.Name);
        }

        [Fact]
        public async Task DeleteBranchAsync_BranchNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "nonexistent",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task DeleteBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 103,
                Address = "3 Freedom Way",
                Lga = "Kosofe",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "branch1",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Update failed please try again later", result.Remark);
        }

        [Fact]
        public async Task DeleteBranchAsync_ValidRequest_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 103,
                Address = "3 Freedom Way",
                Lga = "Kosofe",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteBranchCommand
            {
                PublicId = "branch1",
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.DeleteBranchAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch deleted sucessfully", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_BranchNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "missing",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch2",
                Name = "Branch2",
                Code = 200,
                Address = "20 Marina",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "branch2",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Update failed please try again later", result.Remark);
        }

        [Fact]
        public async Task CloseBranchAsync_ValidRequest_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch2",
                Name = "Branch2",
                Code = 200,
                Address = "20 Marina",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new CloseBranchCommand
            {
                Id = "branch2",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.CloseBranchAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch deleted sucessfully", result.Remark);
        }

        [Fact]
        public async Task GetBranchByPublicIdAsync_Found_ReturnsBranch ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "1 Broad Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchByPublicIdAsync ("branch1", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch1", result.Data.Name);
        }

        [Fact]
        public async Task GetBranchesByUserIdAsync_Found_ReturnsBranches ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "1 Broad Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user123",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchesByUserIdAsync ("user123", CancellationToken.None, 1, 10);

            Assert.True (result.IsSuccessful);
            Assert.Single (result.Data);
            Assert.Equal ("Branch1", result.Data.First ().Name);
        }

        [Fact]
        public async Task GetBranchCountAsync_ReturnsCount ()
        {
            using var context = CreateDbContext ();
            context.Branches.AddRange (
                new Branch
                {
                    PublicId = "branchA",
                    Name = "BranchA",
                    Code = 111,
                    Address = "11 Street",
                    Lga = "Ikeja",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedBy = "user1",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                },
                new Branch
                {
                    PublicId = "branchB",
                    Name = "BranchB",
                    Code = 112,
                    Address = "12 Street",
                    Lga = "Ikeja",
                    State = "Lagos",
                    Country = "Nigeria",
                    CreatedBy = "user2",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchCountAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (2, result.TotalCount);
        }

        [Fact]
        public async Task UpdateBranchAsync_ValidUpdate_ReturnsUpdated ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "Old Address",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "branch1",
                Name = "UpdatedBranch",
                Code = 102,
                Address = "123 Street",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.UpdateBranchAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Branch update successful", result.Remark);
        }


        [Fact]
        public async Task GetBranchByPublicIdAsync_NotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchByPublicIdAsync ("missing", CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }


        [Fact]
        public async Task GetBranchesByUserIdAsync_NoBranches_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchesByUserIdAsync ("userX", CancellationToken.None, 1, 10);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task GetBranchCountByUserIdAsync_ReturnsCount ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch2",
                Name = "Branch2",
                Code = 200,
                Address = "20 Marina",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repository.GetBranchCountByUserIdAsync ("user1", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (1, result.TotalCount);
        }


        [Fact]
        public async Task UpdateBranchAsync_BranchNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "missing",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.UpdateBranchAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Branch not found", result.Remark);
        }

        [Fact]
        public async Task UpdateBranchAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();
            context.Branches.Add (new Branch
            {
                PublicId = "branch1",
                Name = "Branch1",
                Code = 101,
                Address = "Old Address",
                Lga = "Ikeja",
                State = "Lagos",
                Country = "Nigeria",
                CreatedBy = "user1",
                IsDeleted = false,
                DateCreated = DateTime.UtcNow
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repository = new BranchRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new BranchDto
            {
                PublicId = "branch1",
                Name = "UpdatedBranch",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None
            };

            var result = await repository.UpdateBranchAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal ("Update failed please try again later", result.Remark);
        }
    }
}

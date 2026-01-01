using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Industry.Command;
using Application.Models.Industry.Response;

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
    public class IndustryRepositoryTests
    {
        private readonly Mock<ILogger<IndustryRepository>> _loggerMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly IMapper _mapper;

        public IndustryRepositoryTests ()
        {
            _loggerMock = new Mock<ILogger<IndustryRepository>> ();
            _auditLogRepoMock = new Mock<IAuditLogRepository> ();

            var config = new MapperConfiguration (cfg =>
            {
                cfg.CreateMap<IndustryDto, Industry> ();
                cfg.CreateMap<Industry, IndustryResponse> ();
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
        public async Task CreateIndustryAsync_ValidIndustry_ReturnsCreated ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryDto
            {
                Name = "Technology",
                Description = "Technology industry",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.CreateIndustryAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal (201, result.StatusCode);
            Assert.Equal ("Industry creation successful", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Technology", result.Data.Name);
            Assert.Equal ("Technology industry", result.Data.Description);
        }

        [Fact]
        public async Task CreateIndustryAsync_DuplicateIndustry_ReturnsAlreadyExists ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Name = "Finance",
                Description = "Finance industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryDto
            {
                Name = "Finance",
                Description = "Finance industry",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.CreateIndustryAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (400, result.StatusCode);
            Assert.Equal ("Industry already exists", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryAsync_IndustryNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryCommand { Id = 999, DeletedBy = "user1", CancellationToken = CancellationToken.None };
            var result = await repo.DeleteIndustryAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();

            var industry = new Industry
            {
                Id = 1,
                Name = "Healthcare",
                Description = "Healthcare industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Industries.Add (industry);
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryCommand
            {
                Id = 1,
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteIndustryAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal (500, result.StatusCode);
            Assert.Equal ("Update failed please try again later", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryAsync_ValidRequest_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();

            var industry = new Industry
            {
                Id = 1,
                Name = "Manufacturing",
                Description = "Manufacturing industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Industries.Add (industry);
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryCommand
            {
                Id = 1,
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteIndustryAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry deleted successfully", result.Remark);
        }

        [Fact]
        public async Task GetIndustryByIdAsync_NotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustryByIdAsync (999, CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustryByIdAsync_Found_ReturnsIndustry ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Id = 1,
                Name = "Energy",
                Description = "Energy industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryByIdAsync (1, CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Energy", result.Data.Name);
            Assert.Equal ("Energy industry", result.Data.Description);
        }

        [Fact]
        public async Task GetIndustryByNameAsync_NotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustryByNameAsync ("NonExistent", CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustryByNameAsync_Found_ReturnsIndustry ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Id = 1,
                Name = "Automotive",
                Description = "Automotive industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryByNameAsync ("Automotive", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Automotive", result.Data.Name);
            Assert.Equal ("Automotive industry", result.Data.Description);
        }

        [Fact]
        public async Task GetIndustriesByUserIdAsync_NoIndustries_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustriesByUserIdAsync ("userX", CancellationToken.None, 1, 10);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustriesByUserIdAsync_ValidUser_ReturnsIndustries ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Id = 1,
                Name = "Education",
                Description = "Education industry",
                CreatedBy = "user123",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustriesByUserIdAsync ("user123", CancellationToken.None, 1, 10);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industries retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Single (result.Data);
            Assert.Equal ("Education", result.Data.First ().Name);
            Assert.Equal ("Education industry", result.Data.First ().Description);
        }

        [Fact]
        public async Task GetAllIndustriesAsync_NoIndustries_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetAllIndustriesAsync (CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industries not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetAllIndustriesAsync_ReturnsAllIndustries ()
        {
            using var context = CreateDbContext ();

            context.Industries.AddRange (
                new Industry
                {
                    Name = "Technology",
                    Description = "Technology industry",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Industry
                {
                    Name = "Healthcare",
                    Description = "Healthcare industry",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetAllIndustriesAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industries retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal (2, result.Data.Count);
        }

        [Fact]
        public async Task GetIndustryCountAsync_ReturnsCorrectCount ()
        {
            using var context = CreateDbContext ();

            context.Industries.AddRange (
                new Industry
                {
                    Name = "Technology",
                    Description = "Technology industry",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Industry
                {
                    Name = "Finance",
                    Description = "Finance industry",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryCountAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry count successful", result.Remark);
            Assert.Equal (2, result.TotalCount);
        }

        [Fact]
        public async Task GetIndustryCountByUserIdAsync_ReturnsCorrectCount ()
        {
            using var context = CreateDbContext ();

            context.Industries.AddRange (
                new Industry
                {
                    Name = "Technology",
                    Description = "Technology industry",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Industry
                {
                    Name = "Healthcare",
                    Description = "Healthcare industry",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryCountByUserIdAsync ("user1", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry count successful", result.Remark);
            Assert.Equal (1, result.TotalCount);
        }

        [Fact]
        public async Task UpdateIndustryAsync_IndustryNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryDto
            {
                Id = 999,
                Name = "Updated Industry",
                Description = "Updated description",
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.UpdateIndustryAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task UpdateIndustryAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Id = 1,
                Name = "Technology",
                Description = "Technology industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryDto
            {
                Id = 1,
                Name = "Technology Updated",
                Description = "Updated technology industry",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.UpdateIndustryAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (500, result.StatusCode);
            Assert.Equal ("Update failed please try again later", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task UpdateIndustryAsync_ValidUpdate_ReturnsUpdated ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Id = 1,
                Name = "Technology",
                Description = "Technology industry",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryDto
            {
                Id = 1,
                Name = "Technology Updated",
                Description = "Updated technology industry",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.UpdateIndustryAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry update successful", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Technology Updated", result.Data.Name);
            Assert.Equal ("Updated technology industry", result.Data.Description);
        }

        [Fact]
        public async Task GetIndustriesByUserIdAsync_IncludesDeletedIndustryInFilter_ReturnsOnlyActiveIndustries ()
        {
            using var context = CreateDbContext ();

            context.Industries.AddRange (
                new Industry
                {
                    Name = "Active Industry",
                    Description = "Active industry",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Industry
                {
                    Name = "Deleted Industry",
                    Description = "Deleted industry",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IsDeleted = true,
                    DeletedBy = "user1",
                    DateDeleted = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustriesByUserIdAsync ("user1", CancellationToken.None, 1, 10);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.NotNull (result.Data);
            Assert.Single (result.Data);
            Assert.Equal ("Active Industry", result.Data.First ().Name);
        }
    }
}
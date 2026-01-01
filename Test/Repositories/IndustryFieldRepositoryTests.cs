using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.IndustryField.Command;
using Application.Models.IndustryField.Response;

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
    public class IndustryFieldRepositoryTests
    {
        private readonly Mock<ILogger<IndustryFieldRepository>> _loggerMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly IMapper _mapper;

        public IndustryFieldRepositoryTests ()
        {
            _loggerMock = new Mock<ILogger<IndustryFieldRepository>> ();
            _auditLogRepoMock = new Mock<IAuditLogRepository> ();

            var config = new MapperConfiguration (cfg =>
            {
                cfg.CreateMap<IndustryFieldDto, IndustryField> ();
                cfg.CreateMap<IndustryField, IndustryFieldResponse> ();
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
        public async Task CreateIndustryFieldAsync_ValidIndustryField_ReturnsCreated ()
        {
            using var context = CreateDbContext ();
            context.Industries.Add (new Industry
            {
                Name = "Technology",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                Description = "String"
            });

            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryFieldDto
            {
                Name = "Technology",
                CreatedBy = "user1",
                CancellationToken = CancellationToken.None,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            };

            var result = await repo.CreateIndustryFieldAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal (201, result.StatusCode);
            Assert.Equal ("Industry field creation successful", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Technology", result.Data.Name);
        }

        [Fact]
        public async Task CreateIndustryFieldAsync_DuplicateIndustryField_ReturnsAlreadyExists ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                Description = "String"
            });
            await context.SaveChangesAsync ();

            context.IndustryFields.Add (new IndustryField
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryFieldDto
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                CancellationToken = CancellationToken.None,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            };

            var result = await repo.CreateIndustryFieldAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (400, result.StatusCode);
            Assert.Equal ("Industry field already exists", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryFieldAsync_IndustryFieldNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryFieldCommand { Id = 999, DeletedBy = "user1", CancellationToken = CancellationToken.None };
            var result = await repo.DeleteIndustryFieldAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry field not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryFieldAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();

            var industryField = new IndustryField
            {
                Id = 1,
                Name = "Healthcare",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            };
            context.IndustryFields.Add (industryField);
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryFieldCommand
            {
                Id = 1,
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteIndustryFieldAsync (request);

            Assert.False (result.IsSuccessful);
            Assert.Equal (500, result.StatusCode);
            Assert.Equal ("Update failed please try again later", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task DeleteIndustryFieldAsync_ValidRequest_ReturnsDeleted ()
        {
            using var context = CreateDbContext ();

            var industryField = new IndustryField
            {
                Id = 1,
                Name = "Manufacturing",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            };
            context.IndustryFields.Add (industryField);
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var request = new DeleteIndustryFieldCommand
            {
                Id = 1,
                DeletedBy = "user1",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteIndustryFieldAsync (request);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field deleted successfully", result.Remark);
        }

        [Fact]
        public async Task GetIndustryFieldByIdAsync_NotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustryFieldByIdAsync (999, CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry field not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustryFieldByIdAsync_Found_ReturnsIndustryField ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.Add (new IndustryField
            {
                Id = 1,
                Name = "Energy",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryFieldByIdAsync (1, CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Energy", result.Data.Name);
        }

        [Fact]
        public async Task GetIndustryFieldByNameAsync_NotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustryFieldByNameAsync ("NonExistent", CancellationToken.None);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry field not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustryFieldByNameAsync_Found_ReturnsIndustryField ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.Add (new IndustryField
            {
                Id = 1,
                Name = "Automotive",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryFieldByNameAsync ("Automotive", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Automotive", result.Data.Name);
        }

        [Fact]
        public async Task GetIndustryFieldsByUserIdAsync_NoFields_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var result = await repo.GetIndustryFieldsByUserIdAsync ("userX", CancellationToken.None, 1, 10);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry fields not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task GetIndustryFieldsByUserIdAsync_ValidUser_ReturnsFields ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.Add (new IndustryField
            {
                Id = 1,
                Name = "Education",
                CreatedBy = "user123",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryFieldsByUserIdAsync ("user123", CancellationToken.None, 1, 10);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry fields retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Single (result.Data);
            Assert.Equal ("Education", result.Data.First ().Name);
        }

        [Fact]
        public async Task GetAllIndustryFieldsAsync_ReturnsAllFields ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.AddRange (
                new IndustryField
                {
                    Name = "Technology",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                },
                new IndustryField
                {
                    Name = "Healthcare",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetAllIndustryFieldsAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry fields retrieved successfully", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal (2, result.Data.Count);
        }

        [Fact]
        public async Task GetIndustryFieldCountAsync_ReturnsCorrectCount ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.AddRange (
                new IndustryField
                {
                    Name = "Technology",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                },
                new IndustryField
                {
                    Name = "Finance",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryFieldCountAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field count successful", result.Remark);
            Assert.Equal (2, result.TotalCount);
        }

        [Fact]
        public async Task GetIndustryFieldCountByUserIdAsync_ReturnsCorrectCount ()
        {
            using var context = CreateDbContext ();

            context.IndustryFields.AddRange (
                new IndustryField
                {
                    Name = "Technology",
                    CreatedBy = "user1",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                },
                new IndustryField
                {
                    Name = "Healthcare",
                    CreatedBy = "user2",
                    DateCreated = DateTime.UtcNow,
                    IndustryId = 1,
                    DataType = "String",
                    IsRequired = true,
                    Order = 1,
                    IsDeleted = false
                }
            );
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);
            var result = await repo.GetIndustryFieldCountByUserIdAsync ("user1", CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field count successful", result.Remark);
            Assert.Equal (1, result.TotalCount);
        }

        [Fact]
        public async Task UpdateIndustryFieldAsync_IndustryFieldNotFound_ReturnsNotFound ()
        {
            using var context = CreateDbContext ();
            context.Industries.Add (new Industry
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                Description = "String"
            });
            await context.SaveChangesAsync ();

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryFieldDto
            {
                Id = 999,
                LastModifiedBy = "user1",
                CancellationToken = CancellationToken.None,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1
            };
            var result = await repo.UpdateIndustryFieldAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (404, result.StatusCode);
            Assert.Equal ("Industry field not found", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task UpdateIndustryFieldAsync_AuditLogFails_ReturnsAuditLogFailed ()
        {
            using var context = CreateDbContext ();

            context.Industries.Add (new Industry
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                Description = "String"
            });
            await context.SaveChangesAsync ();

            context.IndustryFields.Add (new IndustryField
            {
                Id = 1,
                Name = "Technology",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryFieldDto
            {
                Id = 1,
                Name = "Technology Updated",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            };

            var result = await repo.UpdateIndustryFieldAsync (dto);

            Assert.False (result.IsSuccessful);
            Assert.Equal (500, result.StatusCode);
            Assert.Equal ("Update failed please try again later", result.Remark);
            Assert.Null (result.Data);
        }

        [Fact]
        public async Task UpdateIndustryFieldAsync_ValidUpdate_ReturnsUpdated ()
        {
            using var context = CreateDbContext ();
            context.Industries.Add (new Industry
            {
                Name = "Finance",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                Description = "String"
            });
            await context.SaveChangesAsync ();

            context.IndustryFields.Add (new IndustryField
            {
                Id = 1,
                Name = "Technology",
                CreatedBy = "user1",
                DateCreated = DateTime.UtcNow,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            });
            await context.SaveChangesAsync ();

            _auditLogRepoMock
                .Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
                .ReturnsAsync (RequestResponse<AuditLogResponse>.Created (new AuditLogResponse (), 1, "Audit log"));

            var repo = new IndustryFieldRepository (context, _mapper, _loggerMock.Object, _auditLogRepoMock.Object);

            var dto = new IndustryFieldDto
            {
                Id = 1,
                Name = "Technology Updated",
                LastModifiedBy = "user2",
                CancellationToken = CancellationToken.None,
                IndustryId = 1,
                DataType = "String",
                IsRequired = true,
                Order = 1,
                IsDeleted = false
            };

            var result = await repo.UpdateIndustryFieldAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal (200, result.StatusCode);
            Assert.Equal ("Industry field update successful", result.Remark);
            Assert.NotNull (result.Data);
            Assert.Equal ("Technology Updated", result.Data.Name);
        }

    }
}
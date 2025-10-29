using Application.Interface.Persistence;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;

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
    public class EmailRequestRepositoryTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<IEmailRequestRepository>> _loggerMock;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public EmailRequestRepositoryTests ()
        {
            var config = new MapperConfiguration (cfg =>
            {
                cfg.CreateMap<EmailRequestDto, EmailRequest> ();
                cfg.CreateMap<EmailRequest, EmailRequestResponse> ();
            });
            _mapper = config.CreateMapper ();

            _loggerMock = new Mock<ILogger<IEmailRequestRepository>> ();

            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext> ()
                .UseInMemoryDatabase (Guid.NewGuid ().ToString ())
                .Options;
        }

        private EmailRequestRepository CreateRepository ()
        {
            var context = new ApplicationDbContext (_dbOptions);
            return new EmailRequestRepository (context, _mapper, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateEmailRequestAsync_CreatesRequestSuccessfully ()
        {
            var repo = CreateRepository ();

            var dto = new EmailRequestDto
            {
                CreatedBy = "user123",
                ToRecipient = "recipient@example.com",
                Subject = "Test Subject",
                Message = "Hello World",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.CreateEmailRequestAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.NotNull (result.Data);
            Assert.Equal ("recipient@example.com", result.Data.ToRecipient);
        }

        [Fact]
        public async Task CreateMultipleEmailRequestAsync_CreatesAllRequests ()
        {
            var repo = CreateRepository ();

            var dtos = new List<EmailRequestDto>
            {
                new () { CreatedBy = "user123", ToRecipient = "a@example.com", Subject = "A", Message = "Msg A", CancellationToken = CancellationToken.None },
                new () { CreatedBy = "user123", ToRecipient = "b@example.com", Subject = "B", Message = "Msg B", CancellationToken = CancellationToken.None }
            };

            var result = await repo.CreateMultipleEmailRequestAsync (dtos);

            Assert.True (result.IsSuccessful);
            Assert.Equal (2, result.Data.Count);
        }

        [Fact]
        public async Task DeleteEmailRequestAsync_DeletesRequestSuccessfully ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            var request = new EmailRequest
            {
                Id = 1,
                CreatedBy = "user123",
                IsDeleted = false,
                Message = "Test Message",
                ToRecipient = "Sender@example.com",
                Subject = "Test Subject",
                DateCreated = DateTime.UtcNow
            };

            context.EmailRequests.Add (request);
            await context.SaveChangesAsync ();

            var command = new DeleteEmailCommand
            {
                Id = 1,
                DeletedBy = "user123",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteEmailRequestAsync (command);

            Assert.True (result.IsSuccessful);
            Assert.Equal (1, result.TotalCount);
        }

        [Fact]
        public async Task DeleteMultipleEmailRequestsAsync_DeletesAllSpecifiedRequests ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            var requests = new List<EmailRequest>
            {
                new () { Id = 1, CreatedBy = "user123", IsDeleted = false, DateCreated = DateTime.UtcNow, Message = "Test Message",
                ToRecipient = "Sender@example.com",
                Subject = "Test Subject" },
                new () { Id = 2, CreatedBy = "user123", IsDeleted = false, DateCreated = DateTime.UtcNow, Message = "Test Message",
                ToRecipient = "Sender@example.com",
                Subject = "Test Subject" }
            };

            context.EmailRequests.AddRange (requests);
            await context.SaveChangesAsync ();

            var command = new DeleteMultipleEmailCommand
            {
                Ids = [1, 2],
                DeletedBy = "user123",
                CancellationToken = CancellationToken.None
            };

            var result = await repo.DeleteMultipleEmailRequestsAsync (command);

            Assert.True (result.IsSuccessful);
            Assert.Equal (2, result.TotalCount);
        }

        [Fact]
        public async Task GetAllEmailRequestCountAsync_ReturnsCorrectCount ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.AddRange (
                new EmailRequest
                {
                    Id = 1,
                    IsDeleted = false,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                },
                new EmailRequest
                {
                    Id = 2,
                    IsDeleted = true,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                },
                new EmailRequest
                {
                    Id = 3,
                    IsDeleted = false,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                }
            );
            await context.SaveChangesAsync ();

            var result = await repo.GetAllEmailRequestCountAsync (CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal (2, result.TotalCount);
        }

        [Fact]
        public async Task GetEmailRequestByHtmlStatusAsync_ReturnsMatchingRequests ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.AddRange (
                new EmailRequest
                {
                    Id = 1,
                    IsHtml = true,
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                },
                new EmailRequest
                {
                    Id = 2,
                    IsHtml = false,
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                }
            );
            await context.SaveChangesAsync ();

            var result = await repo.GetEmailRequestByHtmlStatusAsync (true, CancellationToken.None, page: 1, pageSize: 10);

            Assert.True (result.IsSuccessful);
            Assert.Single (result.Data);
            Assert.True (result.Data.First ().IsHtml);
        }

        [Fact]
        public async Task GetEmailRequestByIdAsync_ReturnsCorrectRequest ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.Add (new EmailRequest
            {
                Id = 99,
                IsDeleted = false,
                ToRecipient = "target@example.com",
                DateCreated = DateTime.UtcNow,
                Message = "Test Message",
                Subject = "Test Subject",
                CreatedBy = "user123"
            });
            await context.SaveChangesAsync ();

            var result = await repo.GetEmailRequestByIdAsync (99, CancellationToken.None);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("target@example.com", result.Data.ToRecipient);
        }

        [Fact]
        public async Task GetEmailRequestByRecipientAsync_ReturnsMatchingRequests ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.AddRange (
                new EmailRequest
                {
                    Id = 1,
                    ToRecipient = "user@example.com",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                },
                new EmailRequest
                {
                    Id = 2,
                    CcRecipient = "user@example.com",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                },
                new EmailRequest
                {
                    Id = 3,
                    BccRecipient = "user@example.com",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject",
                    CreatedBy = "user123"
                }
            );
            await context.SaveChangesAsync ();

            var result = await repo.GetEmailRequestByRecipientAsync ("user@example.com", CancellationToken.None, page: 1, pageSize: 10);

            Assert.True (result.IsSuccessful);
            Assert.Equal (3, result.Data.Count);
        }

        [Fact]
        public async Task GetEmailRequestByUserIdAsync_ReturnsRequestsForUser ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.AddRange (
                new EmailRequest
                {
                    Id = 1,
                    CreatedBy = "user123",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject"
                },
                new EmailRequest
                {
                    Id = 2,
                    CreatedBy = "user456",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    Message = "Test Message",
                    ToRecipient = "Sender@example.com",
                    Subject = "Test Subject"
                }
            );
            await context.SaveChangesAsync ();

            var result = await repo.GetEmailRequestByUserIdAsync ("user123", CancellationToken.None, page: 1, pageSize: 10);

            Assert.True (result.IsSuccessful);
            Assert.Single (result.Data);
        }

        [Fact]
        public async Task UpdateEmailRequestAsync_UpdatesRequestSuccessfully ()
        {
            var repo = CreateRepository ();
            var context = new ApplicationDbContext (_dbOptions);

            context.EmailRequests.Add (new EmailRequest
            {
                Id = 1,
                IsDeleted = false,
                Subject = "Old Subject",
                DateCreated = DateTime.UtcNow,
                Message = "Test Message",
                ToRecipient = "Sender@example.com",
                CreatedBy = "user123"
            });
            await context.SaveChangesAsync ();

            var dto = new EmailRequestDto
            {
                Id = 1,
                Subject = "Updated Subject",
                LastModifiedBy = "user123",
                CancellationToken = CancellationToken.None,
                Message = "Test Message",
                ToRecipient = "Sender@example.com",
                CreatedBy = "user123"
            };

            var result = await repo.UpdateEmailRequestAsync (dto);

            Assert.True (result.IsSuccessful);
            Assert.Equal ("Updated Subject", result.Data.Subject);
        }


    }
}

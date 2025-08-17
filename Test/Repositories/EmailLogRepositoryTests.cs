using Application.Interface.Persistence;
using Application.Model.EmailLogs.Command;
using Application.Model.EmailLogs.Queries;
using Application.Models;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class EmailLogRepositoryTests
	{
		private readonly IMapper _mapper;
		private readonly Mock<ILogger<IEmailLogRepository>> _loggerMock;
		private readonly AppSettings _appSettings;
		private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

		public EmailLogRepositoryTests ()
		{
			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<EmailLogDto, EmailLog> ();
				cfg.CreateMap<EmailLog, EmailLogResponse> ();
			});
			_mapper = config.CreateMapper ();

			_loggerMock = new Mock<ILogger<IEmailLogRepository>> ();

			_appSettings = new AppSettings
			{
				EmailSender = "noreply@example.com"
			};

			_dbOptions = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;
		}

		private EmailLogRepository CreateRepository ()
		{
			var context = new ApplicationDbContext (_dbOptions);
			return new EmailLogRepository (context, _mapper, _loggerMock.Object, Options.Create (_appSettings));
		}

		[Fact]
		public async Task CreateEmailLogAsync_CreatesEmailLogSuccessfully ()
		{
			var repo = CreateRepository ();

			var dto = new EmailLogDto
			{
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test Subject",
				Message = "Hello World",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.CreateEmailLogAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.NotNull (result.Data);
			Assert.Equal ("recipient@example.com", result.Data.ToRecipient);
		}

		[Fact]
		public async Task CreateMultipleEmailLogsAsync_CreatesAllEmailLogs ()
		{
			var repo = CreateRepository ();

			var dtos = new List<EmailLogDto>
			{
				new () { CreatedBy = "user123", ToRecipient = "a@example.com", Subject = "A", Message = "Msg A", CancellationToken = CancellationToken.None },
				new () { CreatedBy = "user123", ToRecipient = "b@example.com", Subject = "B", Message = "Msg B", CancellationToken = CancellationToken.None }
			};

			var result = await repo.CreateMultipleEmailLogsAsync (dtos);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task DeleteEmailLogAsync_DeletesEmailLogSuccessfully ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			var emailLog = new EmailLog
			{
				Id = 1,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow
			};

			context.EmailLogs.Add (emailLog);
			await context.SaveChangesAsync ();

			var command = new DeleteEmailLogCommand
			{
				Id = 1,
				UserId = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteEmailLogAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task DeleteMultipleEmailLogsAsync_DeletesAllSpecifiedLogs ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			var logs = new List<EmailLog>
			{
				new () { Id = 1,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow },
				new () {Id = 2,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow }
			};

			context.EmailLogs.AddRange (logs);
			await context.SaveChangesAsync ();

			var command = new DeleteMultipleEmailLogsCommand
			{
				Ids = [1, 2],
				UserId = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleEmailLogsAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetAllEmailLogCountAsync_ReturnsCorrectCount ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow
				},
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetAllEmailLogCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetEmailLogByHtmlStatusAsync_ReturnsMatchingLogs ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsHtml = true
				},
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsHtml = false
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailLogByHtmlStatusAsync (true, CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.True (result.Data.First ().IsHtml);
		}

		[Fact]
		public async Task GetEmailLogByIdAsync_ReturnsCorrectLog ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.Add (new EmailLog
			{
				Id = 99,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow,
				IsHtml = false
			});
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailLogByIdAsync (99, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("recipient@example.com", result.Data.ToRecipient);
		}

		[Fact]
		public async Task GetEmailLogBySentStatusAsync_ReturnsSentLogs ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = true
				},
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = false
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailLogBySentStatusAsync (true, CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.True (result.Data.First ().IsSent);
		}

		[Fact]
		public async Task GetEmailLogByUserIdAsync_ReturnsLogsForUser ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow
				},
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user456",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailLogByUserIdAsync ("user123", CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal ("sender@example.com", result.Data.First ().Sender);
		}

		[Fact]
		public async Task GetUnsentEmailLogCountAsync_ReturnsCorrectCount ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = false
				},
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = true
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetUnsentEmailLogCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task UpdateEmailLogAsync_UpdatesLogSuccessfully ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.Add (new EmailLog
			{
				Id = 1,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			var dto = new EmailLogDto
			{
				ToRecipient = "recipient@example.com",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				LastModifiedDate = DateTime.UtcNow,
				Id = 1,
				Subject = "Updated Subject",
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateEmailLogAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Updated Subject", result.Data.Subject);
		}

		[Fact]
		public async Task UpdateEmailLogSentStatusAsync_UpdatesSentStatus ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.Add (new EmailLog
			{
				Id = 1,
				CreatedBy = "user123",
				ToRecipient = "recipient@example.com",
				Subject = "Test",
				Sender = "sender@example.com",
				Message = "Body",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow,
				IsSent = false
			});
			await context.SaveChangesAsync ();

			var command = new UpdateEmailLogSentStatusCommand
			{
				Id = 1,

				IsSent = true,
				DateSent = DateTime.UtcNow,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateEmailLogSentStatusAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.True (result.Data.IsSent);
		}

		[Fact]
		public async Task UpdateMultipleEmailLogSentStatusAsync_UpdatesAllLogs ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailLogs.AddRange (
				new EmailLog
				{
					Id = 1,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = false
				},
				new EmailLog
				{
					Id = 2,
					CreatedBy = "user123",
					ToRecipient = "recipient@example.com",
					Subject = "Test",
					Sender = "sender@example.com",
					Message = "Body",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					IsSent = false
				}
			);
			await context.SaveChangesAsync ();

			var commands = new List<UpdateEmailLogSentStatusCommand>
	{
		new () { Id = 1, IsSent = true, DateSent = DateTime.UtcNow, LastModifiedBy = "user123", CancellationToken = CancellationToken.None },
		new () { Id = 2, IsSent = true, DateSent = DateTime.UtcNow, LastModifiedBy = "user123", CancellationToken = CancellationToken.None }
	};

			var result = await repo.UpdateMultipleEmailLogSentStatusAsync (commands);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
			Assert.All (result.Data, log => Assert.True (log.IsSent));
		}


	}
}

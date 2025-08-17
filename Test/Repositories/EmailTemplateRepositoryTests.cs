using Application.Interface.Persistence;
using Application.Model.EmailTemplates.Command;
using Application.Model.EmailTemplates.Queries;

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
	public class EmailTemplateRepositoryTests
	{
		private readonly IMapper _mapper;
		private readonly Mock<ILogger<IEmailTemplateRepository>> _loggerMock;
		private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

		public EmailTemplateRepositoryTests ()
		{
			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<EmailTemplateDto, EmailTemplate> ();
				cfg.CreateMap<EmailTemplate, EmailTemplateResponse> ();
			});
			_mapper = config.CreateMapper ();

			_loggerMock = new Mock<ILogger<IEmailTemplateRepository>> ();

			_dbOptions = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;
		}

		private EmailTemplateRepository CreateRepository ()
		{
			var context = new ApplicationDbContext (_dbOptions);
			return new EmailTemplateRepository (context, _mapper, _loggerMock.Object);
		}

		[Fact]
		public async Task CreateEmailTemplateAsync_CreatesTemplateSuccessfully ()
		{
			var repo = CreateRepository ();

			var dto = new EmailTemplateDto
			{
				CreatedBy = "user123",
				TemplateName = "Welcome",
				Template = "<h1>Welcome!</h1>",
				Channel = "Marketing",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.CreateEmailTemplateAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Welcome", result.Data.TemplateName);
		}

		[Fact]
		public async Task DeleteEmailTemplateAsync_DeletesTemplateSuccessfully ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.Add (new EmailTemplate
			{
				Id = 1,
				IsDeleted = false,
				TemplateName = "Promo",
				CreatedBy = "user123",
				Template = "<h1>Welcome!</h1>",
				Channel = "Marketing",
				DateCreated = DateTime.UtcNow
			});
			await context.SaveChangesAsync ();

			var command = new DeleteEmailTemplateCommand
			{
				Id = 1,
				UserId = "admin",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteEmailTemplateAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task DeleteMultipleEmailTemplatesAsync_DeletesAllSpecifiedTemplates ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.AddRange (
				new EmailTemplate
				{
					Id = 1,
					IsDeleted = false,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
					TemplateName = "Template1",
				},
				new EmailTemplate
				{
					Id = 2,
					IsDeleted = false,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
					TemplateName = "Template2",
				}
			);
			await context.SaveChangesAsync ();

			var command = new DeleteMultipleEmailTemplatesCommand
			{
				Ids = [1, 2],
				UserId = "admin",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleEmailTemplatesAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetAllEmailTemplateCountAsync_ReturnsCorrectCount ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.AddRange (
				new EmailTemplate
				{
					Id = 1,
					IsDeleted = false,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
					TemplateName = "Template1"
				},
				new EmailTemplate
				{
					Id = 2,
					IsDeleted = true,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
					TemplateName = "Template1"
				},
				new EmailTemplate
				{
					Id = 3,
					IsDeleted = false,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
					TemplateName = "Template1"
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetAllEmailTemplateCountAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetEmailTemplateByChannelNameAsync_ReturnsMatchingTemplates ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.AddRange (
				new EmailTemplate
				{
					Id = 1,
					Channel = "Support",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					TemplateName = "Template1"
				},
				new EmailTemplate
				{
					Id = 2,
					Channel = "Marketing",
					IsDeleted = false,
					DateCreated = DateTime.UtcNow,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					TemplateName = "Template1"
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailTemplateByChannelNameAsync ("Support", CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
			Assert.Equal ("Support", result.Data.First ().Channel);
		}

		[Fact]
		public async Task GetEmailTemplateByIdAsync_ReturnsCorrectTemplate ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.Add (new EmailTemplate
			{
				Id = 99,
				IsDeleted = false,
				TemplateName = "Welcome",
				Channel = "Marketing",
				Template = "<h1>Hello</h1>",
				DateCreated = DateTime.UtcNow,
				CreatedBy = "user123"
			});
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailTemplateByIdAsync (99, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Welcome", result.Data.TemplateName);
		}

		[Fact]
		public async Task GetEmailTemplateByTemplateNameAsync_ReturnsMatchingTemplate ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.Add (new EmailTemplate
			{
				Id = 1,
				TemplateName = "ResetPassword",
				Channel = "Support",
				Template = "<p>Reset your password</p>",
				IsDeleted = false,
				DateCreated = DateTime.UtcNow,
				CreatedBy = "user123"
			});
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailTemplateByTemplateNameAsync ("ResetPassword", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("ResetPassword", result.Data.TemplateName);
		}

		[Fact]
		public async Task GetEmailTemplateByUserIdAsync_ReturnsTemplatesForUser ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.AddRange (
				new EmailTemplate
				{
					Id = 1,
					CreatedBy = "user123",
					IsDeleted = false,
					TemplateName = "A",
					DateCreated = DateTime.UtcNow,
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
				},
				new EmailTemplate
				{
					Id = 2,
					CreatedBy = "user456",
					IsDeleted = false,
					TemplateName = "B",
					DateCreated = DateTime.UtcNow,
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetEmailTemplateByUserIdAsync ("user123", CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllEmailTemplateAsync_ReturnsAllTemplates ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.AddRange (
				new EmailTemplate
				{
					Id = 1,
					IsDeleted = false,
					TemplateName = "A",
					DateCreated = DateTime.UtcNow,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
				},
				new EmailTemplate
				{
					Id = 2,
					IsDeleted = false,
					TemplateName = "B",
					DateCreated = DateTime.UtcNow,
					CreatedBy = "user123",
					Template = "<h1>Welcome!</h1>",
					Channel = "Marketing",
				}
			);
			await context.SaveChangesAsync ();

			var result = await repo.GetAllEmailTemplateAsync (CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task UpdateEmailTemplateAsync_UpdatesTemplateSuccessfully ()
		{
			var repo = CreateRepository ();
			var context = new ApplicationDbContext (_dbOptions);

			context.EmailTemplates.Add (new EmailTemplate
			{
				Id = 1,
				IsDeleted = false,
				TemplateName = "OldName",
				Channel = "OldChannel",
				Template = "OldContent",
				DateCreated = DateTime.UtcNow,
				CreatedBy = "user123"
			});
			await context.SaveChangesAsync ();

			var dto = new EmailTemplateDto
			{
				Id = 1,
				TemplateName = "NewName",
				Channel = "NewChannel",
				Template = "NewContent",
				LastModifiedBy = "user123",
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.UpdateEmailTemplateAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("NewName", result.Data.TemplateName);
			Assert.Equal ("NewChannel", result.Data.Channel);
		}
	}
}

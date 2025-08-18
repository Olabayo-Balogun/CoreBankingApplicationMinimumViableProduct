using System.Security.Claims;

using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.EmailRequests.Command;
using Application.Model.EmailRequests.Queries;
using Application.Model.EmailTemplates.Queries;
using Application.Models;
using Application.Models.AuditLogs.Response;
using Application.Models.Users.Command;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class UserRepositoryTests
	{
		private readonly Mock<ILogger<UserRepository>> _loggerMock;
		private readonly Mock<IEmailTemplateService> _emailTemplateServiceMock;
		private readonly Mock<IEmailRequestService> _emailRequestServiceMock;
		private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
		private readonly IMapper _mapper;
		private readonly IOptions<AppSettings> _appSettings;

		public UserRepositoryTests ()
		{
			_loggerMock = new Mock<ILogger<UserRepository>> ();
			_emailTemplateServiceMock = new Mock<IEmailTemplateService> ();
			_emailRequestServiceMock = new Mock<IEmailRequestService> ();
			_auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var config = new MapperConfiguration (cfg => { /* Add mappings if needed */ });
			_mapper = config.CreateMapper ();

			_appSettings = Options.Create (new AppSettings ());
		}

		private ApplicationDbContext CreateDbContext ()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;

			return new ApplicationDbContext (options);
		}

		[Fact]
		public async Task DeleteUserAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var command = new DeleteUserCommand { DeletedBy = "nonexistent", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteUserAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task DeleteUserAsync_UserNotAdmin_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, UserRole = "User", Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var command = new DeleteUserCommand { DeletedBy = "user1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteUserAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Unauthorized to delete user", result.Remark);
		}

		[Fact]
		public async Task DeleteUserAsync_AdminUser_DeletesSuccessfully ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "admin1", IsDeleted = false, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var command = new DeleteUserCommand { DeletedBy = "admin1", CancellationToken = CancellationToken.None };

			var result = await repo.DeleteUserAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User deleted sucessfully", result.Remark);
		}

		[Fact]
		public async Task DeleteMultipleUserAsync_OneUserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "admin1", IsDeleted = false, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var command = new DeleteUsersCommand
			{
				DeletedBy = "admin1",
				UserIds = ["userX"],
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleUserAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllDeletedUserByDateAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var date = DateTime.UtcNow.Date;

			var result = await repo.GetAllDeletedUserByDateAsync (date, CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllDeletedUserByDateAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, DateDeleted = DateTime.UtcNow.Date, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var date = DateTime.UtcNow.Date;

			var result = await repo.GetAllDeletedUserByDateAsync (date, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllDeletedUsersAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllDeletedUsersAsync (CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllDeletedUsersAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, DateDeleted = DateTime.UtcNow.Date, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllDeletedUsersAsync (CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllUserByDateAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var date = DateTime.UtcNow.Date;

			var result = await repo.GetAllUserByDateAsync (date, CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllUserByDateAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, DateCreated = DateTime.UtcNow.Date, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var date = DateTime.UtcNow.Date;

			var result = await repo.GetAllUserByDateAsync (date, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllUserByCountryAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUserByCountryAsync ("Nigeria", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllUserByCountryAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, CountryOfOrigin = "Nigeria", Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUserByCountryAsync ("Nigeria", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllUserByRoleAsync_DeletedUsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, UserRole = "Admin", DateDeleted = DateTime.UtcNow, Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUserByRoleAsync ("Admin", true, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllUserByRoleAsync_ActiveUsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user2", IsDeleted = false, UserRole = "User", DateCreated = DateTime.UtcNow, Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUserByRoleAsync ("User", false, CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetAllUsersAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUsersAsync (CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetAllUsersAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, DateCreated = DateTime.UtcNow.Date, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllUsersAsync (CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetCountOfCreatedUserAsync_UsersExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Users.AddRange (
				new User { PublicId = "user1", IsDeleted = false, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User },
				new User { PublicId = "user2", IsDeleted = false, Email = "example2@gmail.com", Password = "Password1!", UserRole = UserRoles.User }
			);
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUserAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfCreatedUserByDateAsync_UsersExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			var today = DateTime.UtcNow.Date;
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, DateCreated = today, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUserByDateAsync (today, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfActiveUsersByDateAsync_DailyPeriod_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			var today = DateTime.UtcNow.Date;
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, LastLoggedInDate = today, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfActiveUsersByDateAsync (today, "daily", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfUserByRoleAsync_UsersExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfUserByRoleAsync ("Admin", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfDeletedUserAsync_UsersExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfDeletedUserAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfDeletedUsersByDateAsync_UsersExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			var today = DateTime.UtcNow.Date;
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, DateDeleted = today, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfDeletedUsersByDateAsync (today, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);
		}

		[Fact]
		public async Task GetDeletedUsersByUserIdAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetDeletedUsersByUserIdAsync ("deleter1", CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetDeletedUsersByUserIdAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = true, DeletedBy = "deleter1", DateDeleted = DateTime.UtcNow, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetDeletedUsersByUserIdAsync ("deleter1", CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetLatestCreatedUsersAsync_NoUsersFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetLatestCreatedUsersAsync (CancellationToken.None, 1, 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Users not found", result.Remark);
		}

		[Fact]
		public async Task GetLatestCreatedUsersAsync_UsersExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, DateCreated = DateTime.UtcNow, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetLatestCreatedUsersAsync (CancellationToken.None, 1, 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Users retrieved successfully", result.Remark);
			Assert.Single (result.Data);
		}

		[Fact]
		public async Task GetUserByIdAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserByIdAsync ("nonexistent", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task GetUserByIdAsync_UserExists_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserByIdAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
		}

		[Fact]
		public async Task GetUserByEmailAddressAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserByEmailAddressAsync ("test@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task GetUserByEmailAddressAsync_UserExists_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Email = "test@example.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserByEmailAddressAsync ("test@example.com", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User retrieved successfully", result.Remark);
			Assert.NotNull (result.Data);
		}

		[Fact]
		public void GetToken_ValidClaims_ReturnsJwtToken ()
		{
			var claims = new List<Claim> { new (ClaimTypes.Name, "user1") };
			var appSettings = new AppSettings
			{
				Secret = "supersecretkey1234567890",
				ValidIssuer = "issuer",
				ValidAudience = "audience"
			};

			var repo = new UserRepository (null, _mapper, Options.Create (appSettings), _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var token = repo.GetToken (claims);

			Assert.NotNull (token);
			Assert.Equal ("issuer", token.Issuer);
		}

		[Fact]
		public void GetLogoutToken_ValidClaims_ReturnsExpiredJwtToken ()
		{
			var claims = new List<Claim> { new (ClaimTypes.Name, "user1") };
			var appSettings = new AppSettings
			{
				Secret = "supersecretkey1234567890",
				ValidIssuer = "issuer",
				ValidAudience = "audience"
			};

			var repo = new UserRepository (null, _mapper, Options.Create (appSettings), _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var token = repo.GetLogoutToken (claims);

			Assert.NotNull (token);
			Assert.Equal ("issuer", token.Issuer);
			Assert.True (token.ValidTo < DateTime.UtcNow);
		}

		[Fact]
		public async Task GetUserLocationByIdAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserLocationByIdAsync ("nonexistent", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task GetUserLocationByIdAsync_UserExists_ReturnsLocation ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User
			{
				PublicId = "user1",
				IsDeleted = false,
				CountryOfOrigin = "Nigeria",
				CountryOfResidence = "UK",
				StateOfOrigin = "Lagos",
				StateOfResidence = "London",
				Email = "example@gmail.com",
				Password = "Password1!",
				UserRole = UserRoles.User
			});
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserLocationByIdAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User retrieved successfully", result.Remark);
			Assert.Equal ("Nigeria", result.Data.CountryOfOrigin);
			Assert.Equal ("UK", result.Data.CountryOfResidence);
		}

		[Fact]
		public async Task GetUserFullNameByIdAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserFullNameByIdAsync ("nonexistent", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task GetUserFullNameByIdAsync_UserExists_ReturnsFullName ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, FirstName = "Ola", LastName = "Bayo", Email = "example@gmail.com", Password = "Password1!", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetUserFullNameByIdAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User retrieved successfully", result.Remark);
			Assert.Equal ("Ola", result.Data.FirstName);
			Assert.Equal ("Bayo", result.Data.LastName);
		}

		[Fact]
		public async Task LoginAsync_UserDoesNotExist_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			var login = new LoginCommand { Email = "test@example.com", Password = "password", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.LoginAsync (login);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User does not exist", result.Remark);
		}

		[Fact]
		public async Task LoginAsync_EmailNotConfirmed_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "test@example.com", Password = "hashed", IsDeleted = false, EmailConfirmed = false, PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var login = new LoginCommand { Email = "test@example.com", Password = "password", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.LoginAsync (login);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Please verify your user email", result.Remark);
		}

		[Fact]
		public async Task LogoutAsync_ReturnsLogoutToken ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.LogoutAsync ("user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Logout successful", result.Remark);
			Assert.False (string.IsNullOrWhiteSpace (result.Data.Token));
		}


		[Fact]
		public async Task RegisterAsync_DuplicateEmail_ReturnsAlreadyExists ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "test@cbamvp.com", IsDeleted = false, EmailConfirmed = true, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var dto = new UserDto { Email = "test@cbamvp.com", Password = "password", CancellationToken = CancellationToken.None };

			var result = await repo.RegisterAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User already exists", result.Remark);
		}

		[Fact]
		public async Task RegisterAsync_AdminEmail_CreatesAdminUser ()
		{
			using var context = CreateDbContext ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var dto = new UserDto
			{
				Email = "admin@cbamvp.com",
				Password = "password",
				FirstName = "Ola",
				LastName = "Bayo",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.RegisterAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal (UserRoles.Admin, result.Data.UserRole);
		}

		[Fact]
		public async Task RegisterAsync_StaffEmail_CreatesStaffUser ()
		{
			using var context = CreateDbContext ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var dto = new UserDto
			{
				Email = "staff@cbamvp.com",
				Password = "password",
				FirstName = "Ola",
				LastName = "Bayo",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.RegisterAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal (UserRoles.Staff, result.Data.UserRole);
		}

		[Fact]
		public async Task RegisterAsync_GeneralEmail_CreatesUser ()
		{
			using var context = CreateDbContext ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var dto = new UserDto
			{
				Email = "user@example.com",
				Password = "password",
				FirstName = "Ola",
				LastName = "Bayo",
				CancellationToken = CancellationToken.None
			};

			var result = await repo.RegisterAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal (UserRoles.User, result.Data.UserRole);
		}

		[Fact]
		public async Task UpdateUserAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var dto = new UserDto { PublicId = "nonexistent", LastModifiedBy = "admin", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
		}

		[Fact]
		public async Task UpdateUserAsync_ModifierNotAdminOrSelf_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Password = "Password1!", Email = "Password2@gmail.com", UserRole = UserRoles.User });
			context.Users.Add (new User { PublicId = "user2", IsDeleted = false, UserRole = "User", Password = "Password1!", Email = "Password1!" });
			await context.SaveChangesAsync ();

			var dto = new UserDto { PublicId = "user1", LastModifiedBy = "user2", Email = "new@example.com", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Unauthorized to update user", result.Remark);
		}

		[Fact]
		public async Task UpdateUserAsync_EmailAlreadyExists_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Email = "old@example.com", Password = "Password1!", UserRole = UserRoles.User });
			context.Users.Add (new User { PublicId = "user2", IsDeleted = false, Email = "new@example.com", EmailConfirmed = true, Password = "Password1!", UserRole = UserRoles.User });
			context.Users.Add (new User { PublicId = "admin", IsDeleted = false, UserRole = "Admin", Password = "Password1!", Email = "example@gmail.com" });
			await context.SaveChangesAsync ();

			var dto = new UserDto
			{
				PublicId = "user1",
				LastModifiedBy = "admin",
				Email = "new@example.com",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserAsync (dto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User email already exists, you cannot update your email address to the email address of an existing user", result.Remark);
		}

		[Fact]
		public async Task UpdateUserAsync_ValidUpdate_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Email = "old@example.com", Password = "Password1!", UserRole = UserRoles.User });
			context.Users.Add (new User { PublicId = "admin", IsDeleted = false, UserRole = "Admin", Password = "Password1!", Email = "example@gmail.com" });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var dto = new UserDto
			{
				PublicId = "user1",
				LastModifiedBy = "admin",
				Email = "updated@example.com",
				FirstName = "Ola",
				LastName = "Bayo",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserAsync (dto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal ("updated@example.com", result.Data.Email);
		}


		[Fact]
		public async Task UpdateUserRoleAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new UpdateUserRoleCommand { UserId = "nonexistent", LastModifiedBy = "admin", UserRole = "Staff", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserRoleAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task UpdateUserRoleAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Password = "Password1!", Email = "user1@example.com", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var command = new UpdateUserRoleCommand { UserId = "user1", LastModifiedBy = "admin", UserRole = "Staff", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserRoleAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("AuditLogFailed", result.Remark);
		}

		[Fact]
		public async Task UpdateUserRoleAsync_ValidUpdate_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, UserRole = "User", Email = "example@gmail.com", Password = "Password1!" });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			var command = new UpdateUserRoleCommand { UserId = "user1", LastModifiedBy = "admin", UserRole = "Admin", CancellationToken = CancellationToken.None };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserRoleAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal ("Admin", result.Data.UserRole);
		}

		[Fact]
		public async Task UpdateUserProfileImageAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserProfileImageAsync ("image.png", "nonexistent", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task UpdateUserProfileImageAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Password = "Password1!", Email = "user1@example.com", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.AuditLogFailed (null));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserProfileImageAsync ("image.png", "user1", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Update failed please try again later", result.Remark);
		}

		[Fact]
		public async Task UpdateUserProfileImageAsync_ValidUpdate_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user1", IsDeleted = false, Password = "Password2!", Email = "user1@example.com", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUserProfileImageAsync ("image.png", "user1", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("User", result.Remark);
			Assert.Equal ("image.png", result.Data.ProfileImage);
		}

		[Fact]
		public async Task VerifyUserEmailAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new EmailVerificationCommand { Email = "missing@example.com", Token = "token123" };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.VerifyUserEmailAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task VerifyUserEmailAsync_EmailAlreadyVerified_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "verified@example.com", IsDeleted = false, EmailConfirmed = true, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new EmailVerificationCommand { Email = "verified@example.com", Token = "anytoken" };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.VerifyUserEmailAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email is already verified", result.Remark);
		}

		[Fact]
		public async Task VerifyUserEmailAsync_IncorrectToken_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = false, EmailVerificationToken = "correct-token", Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new EmailVerificationCommand { Email = "user@example.com", Token = "wrong-token" };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.VerifyUserEmailAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email verification failed due to incorrect token", result.Remark);
		}

		[Fact]
		public async Task VerifyUserEmailAsync_CorrectToken_VerifiesEmail ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = false, EmailVerificationToken = "token123", Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new EmailVerificationCommand { Email = "user@example.com", Token = "token123" };

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.VerifyUserEmailAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Email is verification successful", result.Remark);
			Assert.True (result.Data.IsVerified);
		}

		[Fact]
		public void HashPassword_And_VerifyPassword_WorkCorrectly ()
		{
			var repo = new UserRepository (null, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var plainPassword = "MySecurePassword123!";
			var hashed = repo.HashPassword (plainPassword);

			Assert.False (string.IsNullOrWhiteSpace (hashed));
			Assert.True (repo.VerifyPassword (plainPassword, hashed));
			Assert.False (repo.VerifyPassword ("WrongPassword", hashed));
		}

		[Fact]
		public async Task ForgotPasswordAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ForgotPasswordAsync ("missing@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task ForgotPasswordAsync_EmailNotConfirmed_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ForgotPasswordAsync ("user@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email is unverified", result.Remark);
		}

		[Fact]
		public async Task ForgotPasswordAsync_EmailRequestFails_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = true, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("PasswordReset", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, reset here: {resetLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Failed (null, 500, "Email failed"));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ForgotPasswordAsync ("user@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Password reset failed", result.Remark);
		}

		[Fact]
		public async Task ForgotPasswordAsync_ValidRequest_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = true, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("PasswordReset", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hello {userName}, reset here: {resetLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ForgotPasswordAsync ("user@example.com", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Password reset successful", result.Remark);
		}

		[Fact]
		public async Task ChangePasswordAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new ChangePasswordCommand
			{
				Email = "missing@example.com",
				NewPassword = "Password1!",
				ConfirmPassword = "Password1!",
				Token = Guid.NewGuid (),
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ChangePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task ChangePasswordAsync_EmailNotConfirmed_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new ChangePasswordCommand
			{
				Email = "user@example.com",
				NewPassword = "Password1!",
				ConfirmPassword = "Password1!",
				Token = Guid.NewGuid (),
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ChangePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email is unverified", result.Remark);
		}

		[Fact]
		public async Task ChangePasswordAsync_PasswordsDoNotMatch_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = true, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new ChangePasswordCommand
			{
				Email = "user@example.com",
				NewPassword = "Newpass1!",
				ConfirmPassword = "Wrongpass1!",
				Token = Guid.NewGuid (),
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ChangePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Password does not match", result.Remark);
		}

		[Fact]
		public async Task ChangePasswordAsync_InvalidToken_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = true, PasswordResetToken = Guid.NewGuid (), Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new ChangePasswordCommand
			{
				Email = "user@example.com",
				NewPassword = "Newpass1!",
				ConfirmPassword = "Newpass1!",
				Token = Guid.NewGuid (), // mismatched token
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ChangePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Please input valid token", result.Remark);
		}

		[Fact]
		public async Task ChangePasswordAsync_ValidRequest_UpdatesPassword ()
		{
			using var context = CreateDbContext ();
			var token = Guid.NewGuid ();
			context.Users.Add (new User { Email = "user@example.com", IsDeleted = false, EmailConfirmed = true, PasswordResetToken = token, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new ChangePasswordCommand
			{
				Email = "user@example.com",
				NewPassword = "Newpass1!",
				ConfirmPassword = "Newpass1!",
				Token = token,
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ChangePasswordAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Password update successful", result.Remark);
		}

		[Fact]
		public async Task UpdatePasswordAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var command = new UpdatePasswordCommand
			{
				LastModifiedBy = Guid.NewGuid ().ToString (),
				NewPassword = "Newpass1!",
				ConfirmPassword = "Newpass1!",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdatePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task UpdatePasswordAsync_EmailNotConfirmed_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = Guid.NewGuid ().ToString (), EmailConfirmed = false, IsDeleted = false, Password = "Password2!", Email = "example@gmail.com", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var command = new UpdatePasswordCommand
			{
				LastModifiedBy = context.Users.First ().PublicId,
				NewPassword = "Newpass1!",
				ConfirmPassword = "Newpass1!",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdatePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email is unverified", result.Remark);
		}

		[Fact]
		public async Task UpdatePasswordAsync_PasswordsDoNotMatch_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			var userId = Guid.NewGuid ().ToString ();
			context.Users.Add (new User { PublicId = userId, EmailConfirmed = true, IsDeleted = false, Password = "Password2!", UserRole = UserRoles.User, Email = "example@gmail.com" });
			await context.SaveChangesAsync ();

			var command = new UpdatePasswordCommand
			{
				LastModifiedBy = userId,
				NewPassword = "newpass",
				ConfirmPassword = "wrongpass",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdatePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Password does not match", result.Remark);
		}

		[Fact]
		public async Task UpdatePasswordAsync_NewPasswordMatchesOld_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			var userId = Guid.NewGuid ().ToString ();
			var password = "Password2!";
			context.Users.Add (new User
			{
				PublicId = userId,
				EmailConfirmed = true,
				IsDeleted = false,
				Password = password,
				Email = "example@gmail.com",
				UserRole = UserRoles.User
			});
			await context.SaveChangesAsync ();

			var command = new UpdatePasswordCommand
			{
				LastModifiedBy = userId,
				NewPassword = password,
				ConfirmPassword = password,
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdatePasswordAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Your old and new password must not match", result.Remark);
		}

		[Fact]
		public async Task UpdatePasswordAsync_ValidRequest_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			var userId = Guid.NewGuid ().ToString ();
			context.Users.Add (new User
			{
				PublicId = userId,
				EmailConfirmed = true,
				IsDeleted = false,
				Password = "Password2!",
				UserRole = UserRoles.User,
				Email = "example@gmail.com"
			});
			await context.SaveChangesAsync ();

			var command = new UpdatePasswordCommand
			{
				LastModifiedBy = userId,
				NewPassword = "Password1!",
				ConfirmPassword = "Password1!",
				CancellationToken = CancellationToken.None
			};

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdatePasswordAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Password update successful", result.Remark);
		}

		[Fact]
		public async Task ResendEmailVerificationTokenAsync_UserNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ResendEmailVerificationTokenAsync ("missing@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("User not found", result.Remark);
		}

		[Fact]
		public async Task ResendEmailVerificationTokenAsync_EmailAlreadyVerified_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", EmailConfirmed = true, IsDeleted = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ResendEmailVerificationTokenAsync ("user@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Email is already verified", result.Remark);
		}

		[Fact]
		public async Task ResendEmailVerificationTokenAsync_TemplateFetchFails_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", EmailConfirmed = false, IsDeleted = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Failed (null, 500, "Template missing"));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ResendEmailVerificationTokenAsync ("user@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Token resend unsuccessful", result.Remark);
		}

		[Fact]
		public async Task ResendEmailVerificationTokenAsync_EmailRequestFails_ReturnsFailed ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", EmailConfirmed = false, IsDeleted = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hi {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Failed (null, 500, "Email failed"));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ResendEmailVerificationTokenAsync ("user@example.com", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Token resend unsuccessful", result.Remark);
		}

		[Fact]
		public async Task ResendEmailVerificationTokenAsync_ValidRequest_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { Email = "user@example.com", EmailConfirmed = false, IsDeleted = false, Password = "Password2!", PublicId = "example", UserRole = UserRoles.User });
			await context.SaveChangesAsync ();

			_emailTemplateServiceMock.Setup (x => x.GetEmailTemplateByTemplateNameAsync ("Registration", It.IsAny<CancellationToken> ()))
				.ReturnsAsync (RequestResponse<EmailTemplateResponse>.Success (new EmailTemplateResponse { Template = "Hi {userName}, verify here: {verificationLink}" }, 1, ""));

			_emailRequestServiceMock.Setup (x => x.CreateEmailRequestAsync (It.IsAny<CreateEmailCommand> ()))
				.ReturnsAsync (RequestResponse<EmailRequestResponse>.Success (new EmailRequestResponse (), 1, ""));

			var repo = new UserRepository (context, _mapper, _appSettings, _loggerMock.Object,
				_emailTemplateServiceMock.Object, _emailRequestServiceMock.Object, _auditLogRepoMock.Object);

			var result = await repo.ResendEmailVerificationTokenAsync ("user@example.com", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Token resend successful", result.Remark);
		}
	}
}

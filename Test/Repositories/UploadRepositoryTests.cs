using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Uploads.Command;
using Application.Model.Uploads.Queries;
using Application.Models;
using Application.Models.AuditLogs.Response;

using AutoMapper;

using CloudinaryDotNet.Actions;

using Domain.DTO;
using Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Persistence;
using Persistence.Repositories;

namespace Test.Repositories
{
	public class UploadRepositoryTests
	{
		private readonly Mock<ILogger<UploadRepository>> _loggerMock;
		private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
		private readonly IMapper _mapper;
		private readonly IOptions<AppSettings> _appSettings;

		public UploadRepositoryTests ()
		{
			_loggerMock = new Mock<ILogger<UploadRepository>> ();
			_auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var config = new MapperConfiguration (cfg =>
			{
				cfg.CreateMap<Upload, UploadResponse> ();
			});

			_mapper = config.CreateMapper ();

			_appSettings = Options.Create (new AppSettings
			{
				BaseUrl = "https://localhost/",
				Secret = "supersecretkey1234567890",
				ValidIssuer = "issuer",
				ValidAudience = "audience",
				AcceptableFileFormats = ".jpg, .jpeg, .png, .gif, .svg, .pdf, .webp" // Add this line
			});
		}

		private ApplicationDbContext CreateDbContext ()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;

			return new ApplicationDbContext (options);
		}

		[Fact]
		public async Task GetAllDeletedUploadsAsync_NoDeletedUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllDeletedUploadsAsync (CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
			Assert.Null (result.Data);
		}

		[Fact]
		public async Task GetAllDeletedUploadsAsync_DeletedUploadsExist_ReturnsSuccess ()
		{
			using var context = CreateDbContext ();
			context.Uploads.AddRange (new List<Upload>
			{
				new () { IsDeleted = true, DateDeleted = DateTime.UtcNow.AddDays(-1), FilePath = "file1.jpg", FileFormat = "jpg", FileSize = 1024, PublicId = Guid.NewGuid().ToString(), CreatedBy = "Sample", RootFilePath = "Sample" },
				new () { IsDeleted = true, DateDeleted = DateTime.UtcNow, FilePath = "file2.png", FileFormat = "png", FileSize = 2048, PublicId = Guid.NewGuid().ToString(), CreatedBy = "Sample", RootFilePath = "Sample" }
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetAllDeletedUploadsAsync (CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
			Assert.Equal ("Uploads retrieved successfully", result.Remark);
			Assert.NotEmpty (result.Data);
		}

		[Fact]
		public async Task GetCountOfCreatedUploadsAsync_NoCreatedUploads_ReturnsZeroCount ()
		{
			using var context = CreateDbContext ();
			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUploadsAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
			Assert.Equal ("Uploads count successful", result.Remark);
		}

		[Fact]
		public async Task GetCountOfCreatedUploadsAsync_CreatedUploadsExist_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			context.Uploads.AddRange (new List<Upload>
			{
				new () { IsDeleted = false, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg",  CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new () { IsDeleted = false, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg",  CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new () { IsDeleted = true, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg",  CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" } // should be excluded
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUploadsAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfCreatedUploadsByDateAsync_NoUploadsOnDate_ReturnsZero ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload { IsDeleted = false, DateCreated = DateTime.UtcNow.AddDays (-1), FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" });
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUploadsByDateAsync (DateTime.UtcNow.Date, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (0, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfCreatedUploadsByDateAsync_UploadsExistOnDate_ReturnsCorrectCount ()
		{
			using var context = CreateDbContext ();
			var today = DateTime.UtcNow.Date;

			context.Uploads.AddRange (new List<Upload>
			{
				new () { IsDeleted = false, DateCreated = today, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" , CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new () { IsDeleted = false, DateCreated = today, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" , CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new () { IsDeleted = false, DateCreated = today.AddDays(-1), FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" , CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" } // excluded
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.GetCountOfCreatedUploadsByDateAsync (today, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetCountOfDeletedUploadsAsync_ReturnsCorrectCountAndLogs ()
		{
			using var context = CreateDbContext ();
			context.Uploads.AddRange (
				new Upload { IsDeleted = true, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new Upload { IsDeleted = false, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new Upload { IsDeleted = true, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" }
			);
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetCountOfDeletedUploadsAsync (CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);

			_loggerMock.VerifyLog (LogLevel.Information, Times.AtLeastOnce (), "GetCountOfDeletedUploads begins");
			_loggerMock.VerifyLog (LogLevel.Information, Times.AtLeastOnce (), "GetCountOfDeletedUploads ends");
		}

		[Fact]
		public async Task GetCountOfDeletedUploadsByDateAsync_ReturnsCorrectCount ()
		{
			var targetDate = DateTime.UtcNow.Date;

			using var context = CreateDbContext ();
			context.Uploads.AddRange (
				new Upload { IsDeleted = true, DateDeleted = targetDate, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new Upload { IsDeleted = true, DateDeleted = targetDate.AddDays (-1), FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" },
				new Upload { IsDeleted = false, DateDeleted = targetDate, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample" }
			);
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetCountOfDeletedUploadsByDateAsync (targetDate, CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal (1, result.TotalCount);

			_loggerMock.VerifyLog (LogLevel.Information, Times.AtLeastOnce (), "GetCountOfDeletedUploadsByDate");
		}

		[Fact]
		public async Task GetAllDeletedUploadsByUserIdAsync_ReturnsPagedResults ()
		{
			using var context = CreateDbContext ();
			var userId = "user123";

			context.Uploads.AddRange (
				new Upload { IsDeleted = true, DeletedBy = userId, DateDeleted = DateTime.UtcNow, FilePath = "file1.jpg", FileFormat = "jpg", FileSize = 100, CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample3" },
				new Upload { IsDeleted = true, DeletedBy = userId, DateDeleted = DateTime.UtcNow, FilePath = "file2.jpg", FileFormat = "jpg", FileSize = 200, CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample2" },
				new Upload { IsDeleted = true, DeletedBy = "otherUser", DateDeleted = DateTime.UtcNow, CreatedBy = "Sample", RootFilePath = "Sample", PublicId = "sample", FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" }
			);
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllDeletedUploadsByUserIdAsync (userId, CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
			Assert.Equal (2, result.Data.Count);

			_loggerMock.VerifyLog (LogLevel.Information, Times.AtLeastOnce (), "GetAllDeletedUploadsByUserId");
		}

		private IFormFile CreateMockFormFile (string fileName, string contentType, byte[] content)
		{
			var stream = new MemoryStream (content);
			var formFile = new Mock<IFormFile> ();
			formFile.Setup (f => f.FileName).Returns (fileName);
			formFile.Setup (f => f.Length).Returns (content.Length);
			formFile.Setup (f => f.ContentType).Returns (contentType);
			formFile.Setup (f => f.CopyToAsync (It.IsAny<Stream> (), It.IsAny<CancellationToken> ()))
				.Returns ((Stream target, CancellationToken _) => stream.CopyToAsync (target));

			return formFile.Object;
		}

		[Fact]
		public async Task CreateUploadAsync_ValidUpload_ReturnsCreatedResponse ()
		{
			using var context = CreateDbContext ();

			var loggerMock = new Mock<ILogger<UploadRepository>> ();
			var auditLogRepoMock = new Mock<IAuditLogRepository> ();

			var uploadFile = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				UploadFile = uploadFile,
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var appSettings = Options.Create (new AppSettings
			{
				BaseUrl = "https://localhost",
				MaxFileSizeInBytes = 1024 * 1024 * 5,
				AcceptableFileFormats = ".jpg,.png",
				IsSavingFilesToLocalStorage = true,
				IsSavingFilesToCloudStorage = false
			});

			var repo = new UploadRepository (context, _mapper, appSettings, loggerMock.Object, auditLogRepoMock.Object);

			var result = await repo.CreateUploadAsync (uploadDto);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Upload creation successful", result.Remark);
			Assert.NotNull (result.Data);
		}

		[Fact]
		public async Task DeleteUploadAsync_ValidRequest_DeletesUpload ()
		{
			using var context = CreateDbContext ();

			var userId = "admin123";
			context.Users.Add (new User { PublicId = userId, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });

			var upload = new Upload
			{
				PublicId = "upload123",
				CreatedBy = userId,
				IsDeleted = false,
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg"
			};
			context.Uploads.Add (upload);
			await context.SaveChangesAsync ();

			var auditLogRepoMock = new Mock<IAuditLogRepository> ();
			auditLogRepoMock.Setup (x => x.CreateAuditLogAsync (It.IsAny<CreateAuditLogCommand> ()))
				.ReturnsAsync (RequestResponse<AuditLogResponse>.Success (new AuditLogResponse (), 1, ""));

			var loggerMock = new Mock<ILogger<UploadRepository>> ();
			var repo = new UploadRepository (context, _mapper, _appSettings, loggerMock.Object, auditLogRepoMock.Object);

			var command = new DeleteUploadCommand
			{
				Id = "upload123",
				DeletedBy = userId,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteUploadAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("Upload deleted sucessfully", result.Remark);
		}

		[Fact]
		public async Task DeleteMultipleUploadsAsync_ValidAdminRequest_DeletesAll ()
		{
			using var context = CreateDbContext ();
			var userId = "admin123";

			context.Users.Add (new User { PublicId = userId, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			context.Uploads.AddRange (
				new Upload { PublicId = "file1", CreatedBy = userId, IsDeleted = false, RootFilePath = Path.GetTempFileName (), FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" },
				new Upload { PublicId = "file2", CreatedBy = userId, IsDeleted = false, RootFilePath = Path.GetTempFileName (), FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg" }
			);
			await context.SaveChangesAsync ();

			var auditLogRepoMock = new Mock<IAuditLogRepository> ();
			auditLogRepoMock.Setup (x => x.CreateMultipleAuditLogAsync (It.IsAny<List<CreateAuditLogCommand>> ()))
				.ReturnsAsync (RequestResponse<AuditLogsQueryResponse>.Success (new AuditLogsQueryResponse (), 1, ""));

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, auditLogRepoMock.Object);

			var command = new DeleteMultipleUploadsCommand
			{
				Ids = ["file1", "file2"],
				DeletedBy = userId,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleUploadsAsync (command);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
		}

		[Fact]
		public async Task GetAllUploadsAsync_ReturnsPagedUploads ()
		{
			using var context = CreateDbContext ();
			context.Uploads.AddRange (
				new Upload { PublicId = "1", IsDeleted = false, DateCreated = DateTime.UtcNow, FileFormat = "jpg", FileSize = 100, FilePath = "file1.jpg", CreatedBy = "sample", RootFilePath = "sample" },
				new Upload { PublicId = "2", IsDeleted = false, DateCreated = DateTime.UtcNow, FileFormat = "jpg", FileSize = 100, FilePath = "file2.jpg", CreatedBy = "sample", RootFilePath = "sample" }
			);
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllUploadsAsync (CancellationToken.None, page: 1, pageSize: 10);

			Assert.True (result.IsSuccessful);
			Assert.Equal (2, result.TotalCount);
			Assert.Equal (2, result.Data.Count);
		}

		[Fact]
		public async Task GetUploadByIdAsync_ValidId_ReturnsUpload ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				FilePath = "path.jpg",
				FileFormat = "jpg",
				FileSize = 100,
				CreatedBy = "Sample",
				RootFilePath = "Sample"
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetUploadByIdAsync ("upload123", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("upload123", result.Data.PublicId);
		}

		[Fact]
		public async Task GetUploadByFilePathAsync_ValidPath_ReturnsUpload ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "upload456",
				IsDeleted = false,
				FilePath = "https://localhost/files/image.jpg",
				FileFormat = "jpg",
				FileSize = 200,
				CreatedBy = "Sample",
				RootFilePath = "Sample"
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetUploadByFilePathAsync ("https://localhost/files/image.jpg", CancellationToken.None);

			Assert.True (result.IsSuccessful);
			Assert.Equal ("upload456", result.Data.PublicId);
		}

		[Fact]
		public async Task DeleteMultipleUploadsAsync_UnauthorizedUser_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			var userId = "user456";

			context.Users.Add (new User { PublicId = userId, UserRole = "User", Email = "example@gmail.com", Password = "Password1!" });
			context.Uploads.Add (new Upload
			{
				PublicId = "file1",
				CreatedBy = "otherUser",
				IsDeleted = false,
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg"
			});
			await context.SaveChangesAsync ();

			var auditLogRepoMock = new Mock<IAuditLogRepository> ();
			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, auditLogRepoMock.Object);

			var command = new DeleteMultipleUploadsCommand
			{
				Ids = ["file1"],
				DeletedBy = userId,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleUploadsAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Unauthorized to delete upload", result.Remark);
		}

		[Fact]
		public async Task DeleteMultipleUploadsAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			var userId = "admin123";

			context.Users.Add (new User { PublicId = userId, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			context.Uploads.Add (new Upload
			{
				PublicId = "file1",
				CreatedBy = userId,
				IsDeleted = false,
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg"
			});
			await context.SaveChangesAsync ();

			var auditLogRepoMock = new Mock<IAuditLogRepository> ();
			auditLogRepoMock.Setup (x => x.CreateMultipleAuditLogAsync (It.IsAny<List<CreateAuditLogCommand>> ()))
				.ReturnsAsync (RequestResponse<AuditLogsQueryResponse>.AuditLogFailed (null));

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, auditLogRepoMock.Object);

			var command = new DeleteMultipleUploadsCommand
			{
				Ids = ["file1"],
				DeletedBy = userId,
				CancellationToken = CancellationToken.None
			};

			var result = await repo.DeleteMultipleUploadsAsync (command);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Update failed please try again later", result.Remark);
		}

		[Fact]
		public async Task GetAllUploadsAsync_NoUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No uploads added

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllUploadsAsync (CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}

		[Fact]
		public async Task GetUploadByIdAsync_InvalidId_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching upload

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetUploadByIdAsync ("nonexistent-id", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Upload not found", result.Remark);
		}

		[Fact]
		public async Task GetUploadByFilePathAsync_InvalidPath_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching file path

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetUploadByFilePathAsync ("https://localhost/files/invalid.jpg", CancellationToken.None);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Upload not found", result.Remark);
		}


		[Fact]
		public async Task CreateMultipleUploadsAsync_UnsupportedFormat_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();
			var file = CreateMockFormFile ("test.exe", "application/octet-stream", new byte[100]);

			var uploadDto = new UploadDto
			{
				UploadFile = file,
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.CreateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("formats are allowed", result.Remark);
		}

		[Fact]
		public async Task CreateMultipleUploadsAsync_FileTooLarge_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();
			var largeFile = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[_appSettings.Value.MaxFileSizeInBytes + 1]);

			var uploadDto = new UploadDto
			{
				UploadFile = largeFile,
				CreatedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.CreateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("Maximum allowed file size", result.Remark);
		}

		[Fact]
		public async Task GetCreatedUploadsByUserIdAsync_NoUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No uploads added

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetCreatedUploadsByUserIdAsync ("user123", CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}

		[Fact]
		public async Task UpdateUploadAsync_UnsupportedFormat_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();
			var file = CreateMockFormFile ("test.exe", "application/octet-stream", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUploadAsync (uploadDto);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("formats are allowed", result.Remark);
		}

		[Fact]
		public async Task UpdateUploadAsync_FileTooLarge_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();
			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[_appSettings.Value.MaxFileSizeInBytes + 1]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUploadAsync (uploadDto);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("Please enter valid details", result.Remark);
		}

		[Fact]
		public async Task UpdateUploadAsync_UploadNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching upload

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "nonexistent-id",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUploadAsync (uploadDto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Upload not found", result.Remark);
		}

		[Fact]
		public async Task UpdateUploadAsync_UserNotFound_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				CreatedBy = "creator123",
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg"
			});
			await context.SaveChangesAsync ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "unknownUser",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUploadAsync (uploadDto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Cannot verify user identity", result.Remark);
		}

		[Fact]
		public async Task UpdateUploadAsync_UnauthorizedUser_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user123", UserRole = "User", Email = "user123@gmail.com", Password = "Password1!" });
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				CreatedBy = "creator456",
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg",
			});
			await context.SaveChangesAsync ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateUploadAsync (uploadDto);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Unauthorized to update upload", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UploadWithNullPublicId_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = null,
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Please enter valid details", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UploadFileIsNull_ReturnsNullPayload ()
		{
			using var context = CreateDbContext ();

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = null,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Please enter valid details", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UnsupportedFormat_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();

			var file = CreateMockFormFile ("test.exe", "application/octet-stream", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("formats are allowed", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_FileTooLarge_ReturnsBadRequest ()
		{
			using var context = CreateDbContext ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[_appSettings.Value.MaxFileSizeInBytes + 1]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Contains ("Maximum allowed file size", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UploadNotFound_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No matching upload

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "nonexistent-id",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Upload not found", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UserNotFound_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				CreatedBy = "creator123",
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg"
			});
			await context.SaveChangesAsync ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "unknownUser",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Cannot verify user identity", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_UnauthorizedUser_ReturnsUnauthorized ()
		{
			using var context = CreateDbContext ();
			context.Users.Add (new User { PublicId = "user123", UserRole = "User", Email = "user123@gmail.com", Password = "Password1!" });
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				CreatedBy = "creator456",
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg",
			});
			await context.SaveChangesAsync ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = "user123",
				CancellationToken = CancellationToken.None
			};

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Unauthorized to update upload", result.Remark);
		}

		[Fact]
		public async Task UpdateMultipleUploadsAsync_AuditLogFails_ReturnsAuditLogFailed ()
		{
			using var context = CreateDbContext ();
			var userId = "admin123";

			context.Users.Add (new User { PublicId = userId, UserRole = "Admin", Email = "example@gmail.com", Password = "Password1!" });
			context.Uploads.Add (new Upload
			{
				PublicId = "upload123",
				IsDeleted = false,
				CreatedBy = userId,
				RootFilePath = Path.GetTempFileName (),
				FileFormat = "jpg",
				FileSize = 100,
				FilePath = "file1.jpg",
			});
			await context.SaveChangesAsync ();

			var file = CreateMockFormFile ("test.jpg", "image/jpeg", new byte[100]);

			var uploadDto = new UploadDto
			{
				PublicId = "upload123",
				UploadFile = file,
				LastModifiedBy = userId,
				CancellationToken = CancellationToken.None
			};

			_auditLogRepoMock.Setup (x => x.CreateMultipleAuditLogAsync (It.IsAny<List<CreateAuditLogCommand>> ()))
				.ReturnsAsync (RequestResponse<AuditLogsQueryResponse>.AuditLogFailed (null));

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, _auditLogRepoMock.Object);

			var result = await repo.UpdateMultipleUploadsAsync ([uploadDto]);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Audit log creation failed", result.Remark);
		}

		[Fact]
		public async Task GetAllUploadByDateAsync_NoUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No uploads added

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllUploadByDateAsync (DateTime.UtcNow.Date, CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}

		[Fact]
		public async Task GetAllUploadByDateAsync_OnlyDeletedUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "deleted123",
				IsDeleted = true,
				DateCreated = DateTime.UtcNow.Date,
				FilePath = "deleted.jpg",
				FileFormat = "jpg",
				FileSize = 100,
				CreatedBy = "Sample",
				RootFilePath = "Sample"
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllUploadByDateAsync (DateTime.UtcNow.Date, CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}

		[Fact]
		public async Task GetAllDeletedUploadByDateAsync_NoDeletedUploads_ReturnsNotFound ()
		{
			using var context = CreateDbContext (); // No deleted uploads added

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllDeletedUploadByDateAsync (DateTime.UtcNow.Date, CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}

		[Fact]
		public async Task GetAllDeletedUploadByDateAsync_DeletedWithoutDate_ReturnsNotFound ()
		{
			using var context = CreateDbContext ();
			context.Uploads.Add (new Upload
			{
				PublicId = "deleted123",
				IsDeleted = true,
				DateDeleted = null,
				FilePath = "deleted.jpg",
				FileFormat = "jpg",
				FileSize = 100,
				CreatedBy = "Sample",
				RootFilePath = "Sample"
			});
			await context.SaveChangesAsync ();

			var repo = new UploadRepository (context, _mapper, _appSettings, _loggerMock.Object, null);

			var result = await repo.GetAllDeletedUploadByDateAsync (DateTime.UtcNow.Date, CancellationToken.None, page: 1, pageSize: 10);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Uploads not found", result.Remark);
		}


	}

	public interface ICloudStorageService
	{
		Task<ImageUploadResult> UploadAsync (string base64, string publicId);
		Task<DeletionResult> DeleteAsync (string publicId);
	}

	public class CloudStorageServiceMock : ICloudStorageService
	{
		private readonly Mock<ICloudStorageService> _mock;
		public CloudStorageServiceMock ()
		{
			_mock = new Mock<ICloudStorageService> ();
		}
		public Task<ImageUploadResult> UploadAsync (string base64, string publicId)
		{
			return _mock.Object.UploadAsync (base64, publicId);
		}
		public Task<DeletionResult> DeleteAsync (string publicId)
		{
			return _mock.Object.DeleteAsync (publicId);
		}
		public Mock<ICloudStorageService> GetMock ()
		{
			return _mock;
		}
	}

	public static class LoggerMockExtensions
	{
		public static void VerifyLog (this Mock<ILogger> loggerMock, LogLevel level, Times times, string containsMessage)
		{
			loggerMock.Verify (x => x.Log (
				level,
				It.IsAny<EventId> (),
				It.Is<It.IsAnyType> ((v, t) => v.ToString ().Contains (containsMessage)),
				It.IsAny<Exception> (),
				It.IsAny<Func<It.IsAnyType, Exception, string>> ()), times);
		}

		public static void VerifyLog<T> (this Mock<ILogger<T>> loggerMock, LogLevel level, Times times, string containsMessage)
		{
			loggerMock.Verify (x => x.Log (
				level,
				It.IsAny<EventId> (),
				It.Is<It.IsAnyType> ((v, t) => v.ToString ().Contains (containsMessage)),
				It.IsAny<Exception> (),
				It.IsAny<Func<It.IsAnyType, Exception, string>> ()), times);
		}
	}
}

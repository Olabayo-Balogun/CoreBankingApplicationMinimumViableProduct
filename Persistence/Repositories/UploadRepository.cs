using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Uploads.Command;
using Application.Model.Uploads.Queries;
using Application.Models;
using Application.Models.AuditLogs.Response;
using Application.Utility;

using AutoMapper;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using Domain.DTO;
using Domain.Entities;

using dotenv.net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
	public class UploadRepository : IUploadRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly AppSettings _appSettings;
		private readonly ILogger<UploadRepository> _logger;
		private readonly IAuditLogRepository _auditLogRepository;

		public UploadRepository (ApplicationDbContext context, IMapper mapper, IOptions<AppSettings> appsettings, ILogger<UploadRepository> logger, IAuditLogRepository auditLogRepository)
		{
			_mapper = mapper;
			_context = context;
			_appSettings = appsettings.Value;
			_logger = logger;
			_auditLogRepository = auditLogRepository;
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsAsync (CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllDeletedUploads begins at {DateTime.UtcNow.AddHours (1)}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true)
					.OrderByDescending (x => x.DateDeleted)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetAllDeletedUploads ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");

					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true)
				.LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetAllDeletedUploads ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllDeletedUploads exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfCreatedUploads begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");
				_logger.LogInformation ($"GetCountOfCreatedUploads ends at {DateTime.UtcNow.AddHours (1)} and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfCreatedUploads exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfCreatedUploadsByDate for date: {date} begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");
				_logger.LogInformation ($"GetCountOfCreatedUploadsByDate for date: {date} ends at {DateTime.UtcNow.AddHours (1)} and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfCreatedUploads for date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfDeletedUploads begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");
				_logger.LogInformation ($"GetCountOfDeletedUploads ends at {DateTime.UtcNow.AddHours (1)} and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfDeletedUploads exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfDeletedUploadsByDate for date: {date} begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");
				_logger.LogInformation ($"GetCountOfDeletedUploadsByDate for date: {date} ends at {DateTime.UtcNow.AddHours (1)} and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfDeletedUploadsByDate for date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllDeletedUploadsByUserId begins at {DateTime.UtcNow.AddHours (1)} for userId: {userId}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true && x.DeletedBy == userId)
					.OrderByDescending (x => x.DateDeleted)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetAllDeletedUploadsByUserId for userId: {userId} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true && x.DeletedBy == userId).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetAllDeletedUploadsByUserId for userId: {userId} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllDeletedUploadsByUserId for userId: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> CreateUploadAsync (UploadDto upload)
		{
			try
			{
				_logger.LogInformation ($"CreateUpload begins at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy}");
				long maxFileSize = _appSettings.MaxFileSizeInBytes;
				if (upload == null || upload.UploadFile.Length <= 0)
				{
					var badRequest = RequestResponse<UploadResponse>.NullPayload (null);
					_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				// Check if the file extension is allowed
				var allowedExtensions = _appSettings.AcceptableFileFormats;
				var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
				if (!allowedExtensions.Contains (fileExtension))
				{
					var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");
					_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				// Check the file size
				if (upload.UploadFile.Length > maxFileSize)
				{
					var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Maximum allowed file size is {maxFileSize / (1024 * 1024)} MB.");
					_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				string rootFilePath = "";
				string filePath = "";
				string publicId = Guid.NewGuid ().ToString ();
				var base64String = Utility.ConvertToBase64 (upload.UploadFile);
				if (_appSettings.IsSavingFilesToLocalStorage)
				{
					var uploadsFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Uploads");
					Directory.CreateDirectory (uploadsFolder);

					var uniqueFileName = $"{publicId}_{upload.UploadFile.FileName}";
					rootFilePath = Path.Combine (uploadsFolder, uniqueFileName);

					using (var stream = new FileStream (rootFilePath, FileMode.Create))
					{
						await upload.UploadFile.CopyToAsync (stream, upload.CancellationToken);
					}

					filePath = $"{_appSettings.BaseUrl}/files/{uniqueFileName}";
				}

				if (_appSettings.IsSavingFilesToCloudStorage)
				{
					if (base64String == null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					string mimeType = Utility.GetMimeTypeFromBase64 (base64String);
					string fileDataUrl = $"data:{mimeType};base64,{base64String}";

					DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
					Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
					cloudinary.Api.Secure = true;

					var uploadParams = new ImageUploadParams ()
					{
						File = new FileDescription (fileDataUrl),
						UseFilename = true,
						UniqueFilename = false,
						Overwrite = true,
						PublicId = publicId
					};

					var uploadResult = cloudinary.Upload (uploadParams);
					if (uploadResult == null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (uploadResult.Error != null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"CreateUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark} and error: {uploadResult.Error.Message}");
						return badRequest;
					}

					filePath = uploadResult.SecureUrl.ToString ();
				}

				var image = new Upload
				{
					FileFormat = upload.UploadFile.ContentType,
					FilePath = filePath,
					RootFilePath = rootFilePath,
					FileSize = upload.UploadFile.Length,
					CreatedBy = upload.CreatedBy ?? "Unknown",
					DateCreated = DateTime.UtcNow.AddHours (1),
					DateDeleted = null,
					IsDeleted = false,
					LastModifiedBy = null,
					LastModifiedDate = null,
					DeletedBy = null,
					PublicId = publicId
				};

				await _context.Uploads.AddAsync (image, upload.CancellationToken);
				await _context.SaveChangesAsync (upload.CancellationToken);

				var result = _mapper.Map<UploadResponse> (image);
				var response = RequestResponse<UploadResponse>.Created (result, 1, "Upload");
				_logger.LogInformation ($"CreateUpload for userId: {upload.CreatedBy} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateUpload for userId: {upload.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> DeleteUploadAsync (DeleteUploadCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteUpload begins at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy}");
				var uploadCheck = await _context.Uploads.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (uploadCheck == null)
				{
					var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");
					_logger.LogInformation ($"DeleteUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
					;
				}

				var userCheck = await _context.Users
					.Where (x => x.PublicId == request.DeletedBy)
					.Select (x => x.UserRole)
					.FirstOrDefaultAsync (request.CancellationToken);

				if (userCheck == null)
				{
					var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");
					_logger.LogInformation ($"DeleteUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!userCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && uploadCheck.CreatedBy != request.DeletedBy)
				{
					var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to delete upload");
					_logger.LogInformation ($"DeleteUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = request.CancellationToken,
					CreatedBy = uploadCheck.CreatedBy,
					Name = "Upload",
					Payload = JsonConvert.SerializeObject (uploadCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"DeleteUpload ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				using (var stream = new FileStream (uploadCheck.RootFilePath, FileMode.OpenOrCreate, FileAccess.Write))
				{
					stream.SetLength (0);
				}

				DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
				Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
				cloudinary.Api.Secure = true;

				var uploadResult = await cloudinary.DeleteResourcesAsync (request.Id);
				uploadCheck.IsDeleted = true;
				uploadCheck.DeletedBy = request.DeletedBy;
				uploadCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Update (uploadCheck);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<UploadResponse>.Deleted (null, 1, "Upload");
				_logger.LogInformation ($"DeleteUpload begins at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteUpload for userId: {request.DeletedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> DeleteMultipleUploadsAsync (DeleteMultipleUploadsCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteMultipleUploads begins at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy}");
				List<CreateAuditLogCommand> auditLogs = [];
				List<Upload> uploads = [];

				foreach (string id in request.Ids)
				{
					var uploadCheck = await _context.Uploads.Where (x => x.PublicId == id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

					if (uploadCheck == null)
					{
						var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");
						_logger.LogInformation ($"DeleteMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					var userCheck = await _context.Users
					.Where (x => x.PublicId == request.DeletedBy)
					.Select (x => x.UserRole)
					.FirstOrDefaultAsync (request.CancellationToken);

					if (userCheck == null)
					{
						var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");
						_logger.LogInformation ($"DeleteMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (!userCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && uploadCheck.CreatedBy != request.DeletedBy)
					{
						var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to delete upload");
						_logger.LogInformation ($"DeleteMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					using (var stream = new FileStream (uploadCheck.RootFilePath, FileMode.OpenOrCreate, FileAccess.Write))
					{
						stream.SetLength (0);
					}

					CreateAuditLogCommand createAuditLogRequestViewModel = new ()
					{
						CancellationToken = request.CancellationToken,
						CreatedBy = uploadCheck.CreatedBy,
						Name = "Upload",
						Payload = JsonConvert.SerializeObject (uploadCheck)
					};

					auditLogs.Add (createAuditLogRequestViewModel);

					DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
					Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
					cloudinary.Api.Secure = true;

					var uploadResult = await cloudinary.DeleteResourcesAsync (id);

					uploadCheck.IsDeleted = true;
					uploadCheck.DeletedBy = request.DeletedBy;
					uploadCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

					uploads.Add (uploadCheck);
				}

				RequestResponse<AuditLogsQueryResponse> createAuditLog = await _auditLogRepository.CreateMultipleAuditLogAsync (auditLogs);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"DeleteMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_context.UpdateRange (uploads);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<UploadResponse>.Deleted (null, uploads.Count, "Uploads");
				_logger.LogInformation ($"DeleteMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {request.DeletedBy}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteMultipleUploads for userId: {request.DeletedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetAllUploadsAsync (CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUploads begins at {DateTime.UtcNow.AddHours (1)}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetAllUploads ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetAllUploads ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUploads exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetUploadByIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetUploadById begins at {DateTime.UtcNow.AddHours (1)} for ProductPublicId: {id}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == id)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badResponse = RequestResponse<UploadResponse>.NotFound (null, "Upload");
					_logger.LogInformation ($"GetUploadById for ProductPublicId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var response = RequestResponse<UploadResponse>.SearchSuccessful (result, 1, "Upload");
				_logger.LogInformation ($"GetUploadById for ProductPublicId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUploadById for ProductPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> GetUploadByFilePathAsync (string filePath, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetUploadByFilePath begins at {DateTime.UtcNow.AddHours (1)} for filePath: {filePath}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.FilePath == filePath)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badResponse = RequestResponse<UploadResponse>.NotFound (null, "Upload");
					_logger.LogInformation ($"GetUploadByFilePath for filePath: {filePath} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var response = RequestResponse<UploadResponse>.SearchSuccessful (result, 1, "Upload");
				_logger.LogInformation ($"GetUploadByFilePath for filePath: {filePath} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUploadByFilePath for filePath: {filePath} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> CreateMultipleUploadsAsync (List<UploadDto> uploads)
		{
			try
			{
				_logger.LogInformation ($"CreateMultipleUploads begins at {DateTime.UtcNow.AddHours (1)} for userId: {uploads.First ().CreatedBy}");

				if (uploads == null)
				{
					var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);
					_logger.LogInformation ($"CreateMultipleUploads with remark: {badRequest.Remark} ends at {DateTime.UtcNow.AddHours (1)}");
					return badRequest;
				}

				foreach (UploadDto upload in uploads)
				{
					if (upload.UploadFile == null || upload.UploadFile.Length <= 0)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);
						_logger.LogInformation ($"CreateMultipleUploads for userId: {uploads.First ().CreatedBy} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					// Check if the file extension is allowed
					var allowedExtensions = _appSettings.AcceptableFileFormats;
					var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
					if (!allowedExtensions.Contains (fileExtension))
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");
						_logger.LogInformation ($"CreateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					// Check the file size
					if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");
						_logger.LogInformation ($"CreateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					string rootFilePath = "";
					string filePath = "";
					string publicId = Guid.NewGuid ().ToString ();
					if (_appSettings.IsSavingFilesToLocalStorage)
					{
						var uploadsFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Uploads");
						Directory.CreateDirectory (uploadsFolder);

						var uniqueFileName = $"{publicId}_{upload.UploadFile.FileName}";
						rootFilePath = Path.Combine (uploadsFolder, uniqueFileName);

						using var stream = new FileStream (rootFilePath, FileMode.Create);
						await upload.UploadFile.CopyToAsync (stream, upload.CancellationToken);

						filePath = $"{_appSettings.BaseUrl}/files/{uniqueFileName}";
					}

					if (_appSettings.IsSavingFilesToCloudStorage)
					{
						var base64String = Utility.ConvertToBase64 (upload.UploadFile);

						if (base64String == null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"CreateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
							return badRequest;
						}

						string mimeType = Utility.GetMimeTypeFromBase64 (base64String);
						string fileDataUrl = $"data:{mimeType};base64,{base64String}";

						DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
						Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
						cloudinary.Api.Secure = true;

						var uploadParams = new ImageUploadParams ()
						{
							File = new FileDescription (fileDataUrl),
							UseFilename = true,
							UniqueFilename = false,
							Overwrite = true,
							PublicId = publicId
						};

						var uploadResult = cloudinary.Upload (uploadParams);
						if (uploadResult == null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"CreateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark}");
							return badRequest;
						}

						if (uploadResult.Error != null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"CreateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.CreatedBy} with remark: {badRequest.Remark} with error: {uploadResult.Error.Message}");
							return badRequest;
						}

						filePath = uploadResult.SecureUrl.ToString ();
					}

					upload.FileSize = upload.UploadFile.Length;
					upload.FileFormat = upload.UploadFile.ContentType;
					upload.FilePath = filePath;
					upload.RootFilePath = rootFilePath;
					upload.IsDeleted = false;
					upload.DateDeleted = null;
					upload.LastModifiedBy = null;
					upload.LastModifiedDate = null;
					upload.DeletedBy = null;
					upload.CreatedBy = upload.CreatedBy;
					upload.DateCreated = DateTime.UtcNow.AddHours (1);
					upload.PublicId = publicId;
				}


				var payload = _mapper.Map<List<Upload>> (uploads);

				await _context.Uploads.AddRangeAsync (payload, uploads.First ().CancellationToken);
				await _context.SaveChangesAsync (uploads.First ().CancellationToken);

				var result = _mapper.Map<List<UploadResponse>> (uploads);
				var response = RequestResponse<List<UploadResponse>>.Created (result, uploads.Count, "Uploads");

				_logger.LogInformation ($"CreateMultipleUploads for userId: {uploads.First ().CreatedBy} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateMultipleUploads for userId: {uploads.First ().CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetCreatedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetCreatedUploadsByUserId begins at {DateTime.UtcNow.AddHours (1)} for userId: {userId}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.CreatedBy == userId)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetCreatedUploadsByUserId for userId: {userId} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.CreatedBy == userId).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetCreatedUploadsByUserId for userId: {userId} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCreatedUploadsByUserId for userId: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UploadResponse>> UpdateUploadAsync (UploadDto upload)
		{
			try
			{
				_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} begins at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy}");
				if (upload == null)
				{
					var badRequest = RequestResponse<UploadResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateUpload for upload ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				if (upload.UploadFile == null || upload.UploadFile.Length <= 1)
				{
					var badRequest = RequestResponse<UploadResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				// Check if the file extension is allowed
				var allowedExtensions = _appSettings.AcceptableFileFormats;
				var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
				if (!allowedExtensions.Contains (fileExtension))
				{
					var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				// Check the file size
				if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
				{
					var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var updateUploadRequest = await _context.Uploads.Where (x => x.PublicId == upload.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (upload.CancellationToken);
				if (updateUploadRequest == null)
				{
					var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var userCheck = await _context.Users
				.Where (x => x.PublicId == upload.LastModifiedBy)
				.Select (x => x.UserRole)
				.FirstOrDefaultAsync (upload.CancellationToken);

				if (userCheck == null)
				{
					var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!userCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && updateUploadRequest.CreatedBy != upload.LastModifiedBy)
				{
					var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to update upload");
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				using (var stream = new FileStream (updateUploadRequest.RootFilePath, FileMode.OpenOrCreate, FileAccess.Write))
				{
					stream.SetLength (0);
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = upload.CancellationToken,
					CreatedBy = updateUploadRequest.CreatedBy,
					Name = "Upload",
					Payload = JsonConvert.SerializeObject (updateUploadRequest)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var base64String = Utility.ConvertToBase64 (upload.UploadFile);
				string rootFilePath = "";
				string filePath = "";

				if (_appSettings.IsSavingFilesToLocalStorage)
				{
					var uploadsFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Uploads");
					Directory.CreateDirectory (uploadsFolder);

					var uniqueFileName = Guid.NewGuid ().ToString () + "_" + upload.UploadFile.FileName;
					rootFilePath = Path.Combine (uploadsFolder, uniqueFileName);

					using (var stream = new FileStream (rootFilePath, FileMode.Create))
					{
						await upload.UploadFile.CopyToAsync (stream, upload.CancellationToken);
					}

					filePath = $"{_appSettings.BaseUrl}/files/{uniqueFileName}";
				}

				if (_appSettings.IsSavingFilesToCloudStorage)
				{
					if (base64String == null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					string mimeType = Utility.GetMimeTypeFromBase64 (base64String);
					string fileDataUrl = $"data:{mimeType};base64,{base64String}";

					DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
					Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
					cloudinary.Api.Secure = true;
					var uploadParams = new ImageUploadParams ()
					{
						File = new FileDescription (fileDataUrl),
						UseFilename = true,
						UniqueFilename = false,
						Overwrite = true,
						PublicId = updateUploadRequest.PublicId
					};

					var uploadResult = cloudinary.Upload (uploadParams);
					if (uploadResult == null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (uploadResult.Error != null)
					{
						var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");
						_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark} with error: {uploadResult.Error.Message}");
						return badRequest;
					}
					filePath = uploadResult.SecureUrl.ToString ();
				}

				updateUploadRequest.LastModifiedBy = upload.LastModifiedBy;
				updateUploadRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateUploadRequest.FileFormat = upload.UploadFile.ContentType;
				updateUploadRequest.RootFilePath = rootFilePath;
				updateUploadRequest.FileSize = upload.UploadFile.Length;
				updateUploadRequest.FilePath = filePath;

				_context.Uploads.Update (updateUploadRequest);
				await _context.SaveChangesAsync (upload.CancellationToken);

				var result = _mapper.Map<UploadResponse> (updateUploadRequest);
				var response = RequestResponse<UploadResponse>.Updated (result, 1, "Upload");

				_logger.LogInformation ($"UpdateUpload for upload with Id: {upload.PublicId} and userId: {upload.LastModifiedBy} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;

			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateUpload for upload with Id: {upload.PublicId} and userId: {upload.LastModifiedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> UpdateMultipleUploadsAsync (List<UploadDto> uploads)
		{
			try
			{
				_logger.LogInformation ($"UpdateMultipleUploads begins at {DateTime.UtcNow.AddHours (1)} for userId: {uploads.First ().LastModifiedBy}");
				List<Upload> uploadList = [];
				List<CreateAuditLogCommand> auditLogs = [];
				foreach (var upload in uploads)
				{
					if (upload.PublicId == null)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (upload.UploadFile == null || upload.UploadFile.Length <= 0)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					// Check if the file extension is allowed
					var allowedExtensions = _appSettings.AcceptableFileFormats;
					var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
					if (!allowedExtensions.Contains (fileExtension))
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					// Check the file size
					if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					var updateUploadRequest = await _context.Uploads.Where (x => x.PublicId == upload.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (upload.CancellationToken);
					if (updateUploadRequest == null)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Upload");
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					var userCheck = await _context.Users
					.Where (x => x.PublicId == upload.LastModifiedBy)
					.Select (x => x.UserRole)
					.FirstOrDefaultAsync (upload.CancellationToken);

					if (userCheck == null)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Unauthorized (null, "Cannot verify user identity");
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (!userCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && updateUploadRequest.CreatedBy != upload.LastModifiedBy)
					{
						var badRequest = RequestResponse<List<UploadResponse>>.Unauthorized (null, "Unauthorized to update upload");
						_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					CreateAuditLogCommand createAuditLogRequestViewModel = new ()
					{
						CancellationToken = upload.CancellationToken,
						CreatedBy = updateUploadRequest.CreatedBy,
						Name = "Upload",
						Payload = JsonConvert.SerializeObject (updateUploadRequest)
					};

					using (var stream = new FileStream (updateUploadRequest.RootFilePath, FileMode.OpenOrCreate, FileAccess.Write))
					{
						stream.SetLength (0);
					}

					string rootFilePath = "";
					string filePath = "";
					if (_appSettings.IsSavingFilesToLocalStorage)
					{
						var uploadsFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Uploads");
						Directory.CreateDirectory (uploadsFolder);

						var uniqueFileName = $"{Guid.NewGuid ()}_{upload.UploadFile.FileName}";
						rootFilePath = Path.Combine (uploadsFolder, uniqueFileName);

						using var stream = new FileStream (rootFilePath, FileMode.Create);
						await upload.UploadFile.CopyToAsync (stream, upload.CancellationToken);

						filePath = $"{_appSettings.BaseUrl}/files/{uniqueFileName}";
					}

					if (_appSettings.IsSavingFilesToCloudStorage)
					{
						var base64String = Utility.ConvertToBase64 (upload.UploadFile);

						if (base64String == null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
							return badRequest;
						}

						string mimeType = Utility.GetMimeTypeFromBase64 (base64String);
						string fileDataUrl = $"data:{mimeType};base64,{base64String}";

						DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
						Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
						cloudinary.Api.Secure = true;

						var uploadParams = new ImageUploadParams ()
						{
							File = new FileDescription (fileDataUrl),
							UseFilename = true,
							UniqueFilename = false,
							Overwrite = true,
							PublicId = updateUploadRequest.PublicId
						};
						var uploadResult = cloudinary.Upload (uploadParams);
						if (uploadResult == null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark}");
							return badRequest;
						}

						if (uploadResult.Error != null)
						{
							var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");
							_logger.LogInformation ($"UpdateMultipleUploads for upload with Id: {upload.PublicId} ends at {DateTime.UtcNow.AddHours (1)} for userId: {upload.LastModifiedBy} with remark: {badRequest.Remark} with error: {uploadResult.Error.Message}");
							return badRequest;
						}

						filePath = uploadResult.SecureUrl.ToString ();
					}

					updateUploadRequest.LastModifiedBy = upload.LastModifiedBy;
					updateUploadRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
					updateUploadRequest.FileFormat = upload.UploadFile.ContentType;
					updateUploadRequest.RootFilePath = rootFilePath;
					updateUploadRequest.FileSize = upload.UploadFile.Length;
					updateUploadRequest.FilePath = filePath;

					uploadList.Add (updateUploadRequest);
				}

				RequestResponse<AuditLogsQueryResponse> createAuditLog = await _auditLogRepository.CreateMultipleAuditLogAsync (auditLogs);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<List<UploadResponse>>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateMultipleUploads ends at {DateTime.UtcNow.AddHours (1)} for userId: {uploads.First ().LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_context.Uploads.UpdateRange (uploadList);
				await _context.SaveChangesAsync (uploads.First ().CancellationToken);

				var result = _mapper.Map<List<UploadResponse>> (uploadList);
				var response = RequestResponse<List<UploadResponse>>.Updated (result, uploadList.Count, "Uploads");
				_logger.LogInformation ($"UpdateMultipleUploads for userId: {uploads.First ().LastModifiedBy} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;

			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateMultipleUploads for userId: {uploads.First ().LastModifiedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetAllUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUploadByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
					.OrderByDescending (x => x.DateDeleted)
					.Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetAllUploadByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetAllUploadByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUploadByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllDeletedUploadByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
				var result = await _context.Uploads
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date)
					.OrderByDescending (x => x.DateDeleted)
					 .Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");
					_logger.LogInformation ($"GetAllDeletedUploadByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} uploads retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Uploads
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");
				_logger.LogInformation ($"GetAllDeletedUploadByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} uploads retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllDeletedUploadByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

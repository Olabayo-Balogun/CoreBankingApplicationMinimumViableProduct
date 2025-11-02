using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Uploads.Command;
using Application.Models.Uploads.Response;
using Application.Utility;

using AutoMapper;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using Domain.DTO;
using Domain.Entities;
using Domain.Enums;

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

        public async Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfCreatedUploadsAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfCreatedUploadsAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfCreatedUploadsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfCreatedUploadsByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                long count = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfCreatedUploadsByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfCreatedUploadsByDateAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfDeletedUploadsAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfDeletedUploadsAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfDeletedUploadsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfDeletedUploadsByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                long count = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<UploadResponse>.CountSuccessful (null, count, "Uploads");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfDeletedUploadsByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfDeletedUploadsByDateAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> UpdateUploadAsync (UploadDto upload)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (upload == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (upload.UploadFile == null || upload.UploadFile.Length <= 1)
                {
                    var badRequest = RequestResponse<UploadResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                // Check if the file extension is allowed
                var allowedExtensions = _appSettings.AcceptableFileFormats;
                var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
                if (!allowedExtensions.Contains (fileExtension))
                {
                    var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                // Check the file size
                if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
                {
                    var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateUploadRequest = await _context.Uploads.Where (x => x.PublicId == upload.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (upload.CancellationToken);

                if (updateUploadRequest == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var userCheck = await _context.Users
                .Where (x => x.PublicId == upload.LastModifiedBy)
                .Select (x => x.UserRole)
                .FirstOrDefaultAsync (upload.CancellationToken);

                if (userCheck == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!userCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && updateUploadRequest.CreatedBy != upload.LastModifiedBy)
                {
                    var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to update upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

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

                var base64String = Utility.ConvertToBase64 (upload.UploadFile);
                string rootFilePath = "";
                string filePath = "";

                if (_appSettings.IsSavingFilesToLocalStorage)
                {
                    var uploadsFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Uploads");
                    Directory.CreateDirectory (uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid ().ToString ()}_{upload.UploadFile.FileName}";
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

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

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

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (uploadResult.Error != null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, uploadResult.Error.Message);
                        _logger.LogInformation (closingLog);

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

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (upload.CancellationToken);

                var result = _mapper.Map<UploadResponse> (updateUploadRequest);
                var response = RequestResponse<UploadResponse>.Updated (result, 1, "Upload");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUploadAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;

            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateUploadAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> CreateUploadAsync (UploadDto upload)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy);
                _logger.LogInformation (openingLog);

                long maxFileSize = _appSettings.MaxFileSizeInBytes;
                if (upload == null || upload.UploadFile.Length <= 0)
                {
                    var badRequest = RequestResponse<UploadResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                // Check if the file extension is allowed
                var allowedExtensions = _appSettings.AcceptableFileFormats;
                var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
                if (!allowedExtensions.Contains (fileExtension))
                {
                    var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                // Check the file size
                if (upload.UploadFile.Length > maxFileSize)
                {
                    var badRequest = RequestResponse<UploadResponse>.Failed (null, 400, $"Maximum allowed file size is {maxFileSize / (1024 * 1024)} MB.");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

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

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

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

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (uploadResult.Error != null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Upload failed please try again later");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), uploadResult.Error.Message);
                        _logger.LogInformation (closingLog);


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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateUploadAsync), nameof (upload.CreatedBy), upload.CreatedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateUploadAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> DeleteUploadAsync (DeleteUploadCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var uploadCheck = await _context.Uploads.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (uploadCheck == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var userCheck = await _context.Users
                    .Where (x => x.PublicId == request.DeletedBy)
                    .Select (x => x.UserRole)
                    .FirstOrDefaultAsync (request.CancellationToken);

                if (userCheck == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!userCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && uploadCheck.CreatedBy != request.DeletedBy)
                {
                    var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to delete upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = uploadCheck.CreatedBy,
                    Name = "Upload",
                    Payload = JsonConvert.SerializeObject (uploadCheck)
                };

                using (var stream = new FileStream (uploadCheck.RootFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.SetLength (0);
                }

                if (uploadCheck.FilePath.StartsWith ("https://res.cloudinary.com/"))
                {
                    DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
                    Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
                    cloudinary.Api.Secure = true;

                    var uploadResult = await cloudinary.DeleteResourcesAsync (request.Id);

                    if (uploadResult == null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Delete failed please try again later");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (uploadResult.Error != null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Delete failed please try again later");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, uploadResult.Error.Message);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }
                }

                uploadCheck.IsDeleted = true;
                uploadCheck.DeletedBy = request.DeletedBy;
                uploadCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<UploadResponse>.Deleted (null, 1, "Upload");

                string conlcusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUploadAsync), nameof (request.Id), request.Id, nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conlcusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteUploadAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }
        public async Task<RequestResponse<UploadResponse>> DeleteMultipleUploadsAsync (DeleteMultipleUploadsCommand request)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleUploadsAsync), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (initiationLog);

                List<CreateAuditLogCommand> auditLogs = [];
                List<Upload> uploads = [];

                foreach (string id in request.Ids)
                {
                    var uploadCheck = await _context.Uploads.Where (x => x.PublicId == id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

                    if (uploadCheck == null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var userCheck = await _context.Users
                    .Where (x => x.PublicId == request.DeletedBy)
                    .Select (x => x.UserRole)
                    .FirstOrDefaultAsync (request.CancellationToken);

                    if (userCheck == null)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Cannot verify user identity");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (!userCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && uploadCheck.CreatedBy != request.DeletedBy)
                    {
                        var badRequest = RequestResponse<UploadResponse>.Unauthorized (null, "Unauthorized to delete upload");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

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

                    if (_appSettings.IsSavingFilesToCloudStorage)
                    {
                        DotEnv.Load (options: new DotEnvOptions (probeForEnv: false));
                        Cloudinary cloudinary = new (_appSettings.CloudinaryUrl);
                        cloudinary.Api.Secure = true;

                        var uploadResult = await cloudinary.DeleteResourcesAsync (id);

                        if (uploadResult == null)
                        {
                            var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Delete failed please try again later");

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                            _logger.LogInformation (closingLog);

                            return badRequest;
                        }

                        if (uploadResult.Error != null)
                        {
                            var badRequest = RequestResponse<UploadResponse>.Failed (null, 500, "Delete failed please try again later");

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, uploadResult.Error.Message);
                            _logger.LogInformation (closingLog);

                            return badRequest;
                        }
                    }

                    uploadCheck.IsDeleted = true;
                    uploadCheck.DeletedBy = request.DeletedBy;
                    uploadCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                    uploads.Add (uploadCheck);
                }

                RequestResponse<AuditLogsQueryResponse> createAuditLog = await _auditLogRepository.CreateMultipleAuditLogAsync (auditLogs);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UploadResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<UploadResponse>.Deleted (null, uploads.Count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUploadsAsync), nameof (request.DeletedBy), request.DeletedBy, nameof (result.TotalCount), result.TotalCount.ToString (), result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteMultipleUploadsAsync), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> GetUploadByIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUploadByIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.PublicId == id)
                    .Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUploadByIdAsync), nameof (id), id, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UploadResponse>.SearchSuccessful (result, 1, "Upload");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUploadByIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUploadByIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UploadResponse>> GetUploadByFilePathAsync (string filePath, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUploadByFilePathAsync), nameof (filePath), filePath);
                _logger.LogInformation (openingLog);

                var result = await _context.Uploads
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.FilePath == filePath)
                    .Select (x => new UploadResponse { PublicId = x.PublicId, FilePath = x.FilePath, FileFormat = x.FileFormat, FileSize = x.FileSize })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<UploadResponse>.NotFound (null, "Upload");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUploadByFilePathAsync), nameof (filePath), filePath, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UploadResponse>.SearchSuccessful (result, 1, "Upload");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUploadByFilePathAsync), nameof (filePath), filePath, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUploadByFilePathAsync), nameof (filePath), filePath, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UploadResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetAllUploadsAsync (CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUploadsAsync));
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUploadsAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUploadsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsAsync (CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllDeletedUploadsAsync));
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true)
                .LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadsAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUploadsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> CreateMultipleUploadsAsync (List<UploadDto> uploads)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleUploadsAsync));
                _logger.LogInformation (initiationLog);

                if (uploads == null)
                {
                    var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                foreach (UploadDto upload in uploads)
                {
                    if (upload.UploadFile == null || upload.UploadFile.Length <= 0)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    // Check if the file extension is allowed
                    var allowedExtensions = _appSettings.AcceptableFileFormats;
                    var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
                    if (!allowedExtensions.Contains (fileExtension))
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    // Check the file size
                    if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

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

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                            _logger.LogInformation (closingLog);

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

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                            _logger.LogInformation (closingLog);

                            return badRequest;
                        }

                        if (uploadResult.Error != null)
                        {
                            var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), uploadResult.Error.Message);
                            _logger.LogInformation (closingLog);

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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleUploadsAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateMultipleUploadsAsync), ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetCreatedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCreatedUploadsByUserIdAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCreatedUploadsByUserIdAsync), nameof (userId), userId, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.CreatedBy == userId).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetCreatedUploadsByUserIdAsync), nameof (userId), userId, nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCreatedUploadsByUserIdAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllDeletedUploadsByUserIdAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadsByUserIdAsync), nameof (userId), userId, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true && x.DeletedBy == userId).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadsByUserIdAsync), nameof (userId), userId, nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllDeletedUploadsByUserIdAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> UpdateMultipleUploadsAsync (List<UploadDto> uploads)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (UpdateMultipleUploadsAsync));
                _logger.LogInformation (initiationLog);

                List<Upload> uploadList = [];
                List<CreateAuditLogCommand> auditLogs = [];

                foreach (var upload in uploads)
                {
                    if (upload.PublicId == null)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (upload.UploadFile == null || upload.UploadFile.Length <= 0)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.NullPayload (null);

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    // Check if the file extension is allowed
                    var allowedExtensions = _appSettings.AcceptableFileFormats;
                    var fileExtension = Path.GetExtension (upload.UploadFile.FileName).ToLower ();
                    if (!allowedExtensions.Contains (fileExtension))
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Only {_appSettings.AcceptableFileFormats.ToUpper ()} formats are allowed.");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    // Check the file size
                    if (upload.UploadFile.Length > _appSettings.MaxFileSizeInBytes)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 400, $"Maximum allowed file size is {_appSettings.MaxFileSizeInBytes / (1024 * 1024)} MB.");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var updateUploadRequest = await _context.Uploads.Where (x => x.PublicId == upload.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (upload.CancellationToken);
                    if (updateUploadRequest == null)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Upload");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var userCheck = await _context.Users
                    .Where (x => x.PublicId == upload.LastModifiedBy)
                    .Select (x => x.UserRole)
                    .FirstOrDefaultAsync (upload.CancellationToken);

                    if (userCheck == null)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Unauthorized (null, "Cannot verify user identity");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (!userCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && updateUploadRequest.CreatedBy != upload.LastModifiedBy)
                    {
                        var badRequest = RequestResponse<List<UploadResponse>>.Unauthorized (null, "Unauthorized to update upload");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

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

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);
                            _logger.LogInformation (closingLog);

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

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, badRequest.Remark);

                            _logger.LogInformation (closingLog);
                            return badRequest;
                        }

                        if (uploadResult.Error != null)
                        {
                            var badRequest = RequestResponse<List<UploadResponse>>.Failed (null, 500, "Upload failed please try again later");

                            string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), nameof (upload.PublicId), upload.PublicId, nameof (upload.LastModifiedBy), upload.LastModifiedBy, uploadResult.Error.Message);
                            _logger.LogInformation (closingLog);

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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (uploads.First ().CancellationToken);

                var result = _mapper.Map<List<UploadResponse>> (uploadList);
                var response = RequestResponse<List<UploadResponse>>.Updated (result, uploadList.Count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleUploadsAsync), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;

            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateMultipleUploadsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetAllUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllDeletedUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<UploadResponse>>.NotFound (null, "Uploads");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Uploads
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<UploadResponse>>.SearchSuccessful (result, count, "Uploads");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllDeletedUploadByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UploadResponse>>.Error (null);
            }
        }
    }
}

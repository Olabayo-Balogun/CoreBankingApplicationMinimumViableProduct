using Application.Model;
using Application.Model.Uploads.Command;
using Application.Model.Uploads.Queries;
using Application.Models.Uploads.Command;

namespace Application.Interface.Infrastructure
{
	public interface IUploadService
	{
		Task<RequestResponse<UploadResponse>> CreateUploadAsync (CreateUploadCommand uploadFile);
		Task<RequestResponse<List<UploadResponse>>> CreateMultipleUploadsAsync (CreateMultipleUploadsCommand uploadFiles);
		Task<RequestResponse<UploadResponse>> UpdateUploadAsync (UpdateUploadCommand uploadFile);
		Task<RequestResponse<List<UploadResponse>>> UpdateMultipleUploadAsync (UpdateMultipleUploadCommand uploadFile);
		Task<RequestResponse<UploadResponse>> DeleteUploadAsync (DeleteUploadCommand request);
		Task<RequestResponse<UploadResponse>> DeleteMultipleUploadAsync (DeleteMultipleUploadsCommand request);
		Task<RequestResponse<UploadResponse>> GetUploadByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<List<UploadResponse>>> GetUploadsByProductPublicIdAsync (string productPublicId, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<UploadResponse>> GetUploadByFilePathAsync (string filePath, CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<List<UploadResponse>>> GetAllUploadsAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetCreatedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetDeletedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize);

	}
}

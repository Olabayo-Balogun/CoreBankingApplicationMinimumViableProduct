using Application.Model;
using Application.Model.Uploads.Command;
using Application.Model.Uploads.Queries;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IUploadRepository
	{
		Task<RequestResponse<UploadResponse>> CreateUploadAsync (UploadDto uploadFile);
		Task<RequestResponse<List<UploadResponse>>> CreateMultipleUploadsAsync (List<UploadDto> uploadFiles);
		Task<RequestResponse<UploadResponse>> UpdateUploadAsync (UploadDto uploadFile);
		Task<RequestResponse<List<UploadResponse>>> UpdateMultipleUploadsAsync (List<UploadDto> uploadFile);
		Task<RequestResponse<UploadResponse>> DeleteUploadAsync (DeleteUploadCommand request);
		Task<RequestResponse<UploadResponse>> DeleteMultipleUploadsAsync (DeleteMultipleUploadsCommand request);
		Task<RequestResponse<UploadResponse>> GetUploadByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsAsync (CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfCreatedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetCountOfDeletedUploadsByDateAsync (DateTime date, CancellationToken cancellationToken);
		Task<RequestResponse<UploadResponse>> GetUploadByFilePathAsync (string filePath, CancellationToken cancellationToken);
		Task<RequestResponse<List<UploadResponse>>> GetAllUploadsAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsAsync (CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetCreatedUploadsByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<UploadResponse>>> GetAllDeletedUploadByDateAsync (DateTime date, CancellationToken cancellationToken, int page, int pageSize);
	}
}

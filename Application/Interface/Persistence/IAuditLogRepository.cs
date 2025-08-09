using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Models.AuditLogs.Response;

namespace Application.Interface.Persistence
{
	public interface IAuditLogRepository
	{
		Task<RequestResponse<AuditLogResponse>> CreateAuditLogAsync (CreateAuditLogCommand request);
		Task<RequestResponse<AuditLogsQueryResponse>> CreateMultipleAuditLogAsync (List<CreateAuditLogCommand> requests);
		Task<RequestResponse<AuditLogResponse>> GetAuditLogByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<AuditLogsQueryResponse>> GetAuditLogsAsync (string? userId, string? logName, CancellationToken cancellationToken, int pageNumber, int pageSize);

	}
}

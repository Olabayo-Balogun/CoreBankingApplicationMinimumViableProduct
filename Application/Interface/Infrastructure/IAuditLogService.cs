using Application.Model;
using Application.Models.AuditLogs.Response;

namespace Application.Interface.Infrastructure
{
	public interface IAuditLogService
	{
		Task<RequestResponse<AuditLogResponse>> GetAuditLogByIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<AuditLogsQueryResponse>> GetAuditLogsAsync (string? userId, string? logName, CancellationToken cancellationToken, int pageNumber, int pageSize);
	}
}

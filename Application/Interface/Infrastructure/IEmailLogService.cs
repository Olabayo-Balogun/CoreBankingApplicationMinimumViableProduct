using Application.Model;
using Application.Model.EmailLogs.Command;
using Application.Model.EmailLogs.Queries;

namespace Application.Interface.Infrastructure
{
	public interface IEmailLogService
	{
		Task<RequestResponse<EmailLogResponse>> CreateEmailLogAsync (CreateEmailLogCommand emailLog);
		Task<RequestResponse<EmailLogResponse>> DeleteEmailLogAsync (DeleteEmailLogCommand request);
		Task<RequestResponse<List<EmailLogResponse>>> CreateMultipleEmailLogsAsync (List<CreateEmailLogCommand> emailLogs);

		Task<RequestResponse<EmailLogResponse>> DeleteMultipleEmailLogsAsync (DeleteMultipleEmailLogsCommand request);
		Task<RequestResponse<EmailLogResponse>> GetEmailLogByIdAsync (long id, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogBySentStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<EmailLogResponse>> GetAllEmailLogCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<EmailLogResponse>> GetUnsentEmailLogCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<EmailLogResponse>> UpdateEmailLogSentStatusAsync (UpdateEmailLogSentStatusCommand request);
	}
}

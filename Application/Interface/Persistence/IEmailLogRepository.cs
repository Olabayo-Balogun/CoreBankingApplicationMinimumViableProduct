using Application.Models;
using Application.Models.EmailLogs.Command;
using Application.Models.EmailLogs.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IEmailLogRepository
	{
		Task<RequestResponse<EmailLogResponse>> CreateEmailLogAsync (EmailLogDto emailLog);
		Task<RequestResponse<EmailLogResponse>> UpdateEmailLogAsync (EmailLogDto emailLog);
		Task<RequestResponse<List<EmailLogResponse>>> CreateMultipleEmailLogsAsync (List<EmailLogDto> emailLogs);
		Task<RequestResponse<List<EmailLogResponse>>> UpdateMultipleEmailLogSentStatusAsync (List<UpdateEmailLogSentStatusCommand> requests);
		Task<RequestResponse<EmailLogResponse>> DeleteEmailLogAsync (DeleteEmailLogCommand request);
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

using Application.Models;
using Application.Models.EmailTemplates.Command;
using Application.Models.EmailTemplates.Response;

namespace Application.Interface.Infrastructure
{
	public interface IEmailTemplateService
	{
		Task<RequestResponse<EmailTemplateResponse>> CreateEmailTemplateAsync (CreateEmailTemplateCommand emailTemplate);
		Task<RequestResponse<EmailTemplateResponse>> DeleteEmailTemplateAsync (DeleteEmailTemplateCommand request);
		Task<RequestResponse<EmailTemplateResponse>> DeleteMultipleEmailTemplatesAsync (DeleteMultipleEmailTemplatesCommand requests);
		Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByIdAsync (long id, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByTemplateNameAsync (string name, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByChannelNameAsync (string name, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<EmailTemplateResponse>> GetAllEmailTemplateCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetAllEmailTemplateAsync (CancellationToken cancellationToken, int pageNumber, int pageSize);
	}
}

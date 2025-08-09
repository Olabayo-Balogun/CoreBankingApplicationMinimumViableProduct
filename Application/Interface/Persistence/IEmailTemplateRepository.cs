using Application.Model;
using Application.Model.EmailTemplates.Command;
using Application.Model.EmailTemplates.Queries;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IEmailTemplateRepository
	{
		Task<RequestResponse<EmailTemplateResponse>> CreateEmailTemplateAsync (EmailTemplateDto emailTemplate);
		Task<RequestResponse<EmailTemplateResponse>> UpdateEmailTemplateAsync (EmailTemplateDto emailTemplate);
		Task<RequestResponse<EmailTemplateResponse>> DeleteEmailTemplateAsync (DeleteEmailTemplateCommand request);
		Task<RequestResponse<EmailTemplateResponse>> DeleteMultipleEmailTemplatesAsync (DeleteMultipleEmailTemplatesCommand request);
		Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByIdAsync (long id, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByTemplateNameAsync (string name, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByChannelNameAsync (string name, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<EmailTemplateResponse>> GetAllEmailTemplateCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailTemplateResponse>>> GetAllEmailTemplateAsync (CancellationToken cancellationToken, int pageNumber, int pageSize);
	}
}

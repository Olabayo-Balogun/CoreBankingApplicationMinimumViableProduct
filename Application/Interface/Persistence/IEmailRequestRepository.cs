using Application.Models;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IEmailRequestRepository
	{
		Task<RequestResponse<EmailRequestResponse>> CreateEmailRequestAsync (EmailRequestDto emailRequest);
		Task<RequestResponse<EmailRequestResponse>> UpdateEmailRequestAsync (EmailRequestDto emailRequest);
		Task<RequestResponse<List<EmailRequestResponse>>> CreateMultipleEmailRequestAsync (List<EmailRequestDto> emailRequests);
		Task<RequestResponse<EmailRequestResponse>> DeleteEmailRequestAsync (DeleteEmailCommand request);
		Task<RequestResponse<EmailRequestResponse>> DeleteMultipleEmailRequestsAsync (DeleteMultipleEmailCommand request);
		Task<RequestResponse<EmailRequestResponse>> GetEmailRequestByIdAsync (long id, CancellationToken cancellationToken);
		Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByRecipientAsync (string recipientEmailAddress, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize);
		Task<RequestResponse<EmailRequestResponse>> GetAllEmailRequestCountAsync (CancellationToken cancellationToken);
	}
}

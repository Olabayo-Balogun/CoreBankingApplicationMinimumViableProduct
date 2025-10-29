using Application.Models;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;

namespace Application.Interface.Infrastructure
{
    public interface IEmailRequestService
    {
        Task<RequestResponse<EmailRequestResponse>> CreateEmailRequestAsync (CreateEmailCommand emailRequest);
        Task<RequestResponse<List<EmailRequestResponse>>> CreateMultipleEmailRequestAsync (List<CreateEmailCommand> emailRequests);
        Task<RequestResponse<EmailRequestResponse>> DeleteEmailRequestAsync (DeleteEmailCommand request);
        Task<RequestResponse<EmailRequestResponse>> DeleteMultipleEmailRequestsAsync (DeleteMultipleEmailCommand request);
        Task<RequestResponse<EmailRequestResponse>> GetEmailRequestByIdAsync (long id, CancellationToken cancellationToken);
        Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByRecipientAsync (string recipientEmailAddress, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<EmailRequestResponse>> GetAllEmailRequestCountAsync (CancellationToken cancellationToken);

    }
}

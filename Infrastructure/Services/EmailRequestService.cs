using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailLogs.Command;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EmailRequestService : IEmailRequestService
    {
        private readonly IEmailRequestRepository _emailRequestRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<IEmailRequestService> _logger;
        private readonly IEmailLogService _emailLogService;
        public EmailRequestService (IEmailRequestRepository emailRequestRepository, IMapper mapper, ILogger<IEmailRequestService> logger, IEmailLogService emailLogService)
        {
            _emailRequestRepository = emailRequestRepository;
            _mapper = mapper;
            _logger = logger;
            _emailLogService = emailLogService;
        }
        public async Task<RequestResponse<EmailRequestResponse>> CreateEmailRequestAsync (CreateEmailCommand emailRequest)
        {
            try
            {
                _logger.LogInformation ($"CreateEmailRequest begins service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailRequest.CreatedBy} to recipient: {emailRequest.ToRecipient}");
                var payload = _mapper.Map<EmailRequestDto> (emailRequest);

                RequestResponse<EmailRequestResponse> response = await _emailRequestRepository.CreateEmailRequestAsync (payload);
                _logger.LogInformation ($"CreateEmailRequest ends service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailRequest.CreatedBy} to recipient: {emailRequest.ToRecipient}");

                if (response.IsSuccessful && response.Data != null)
                {
                    _logger.LogInformation ($"Email logging begins at {DateTime.UtcNow.AddHours (1)} for email request ID: {response.Data.Id} by userId: {emailRequest.CreatedBy}");
                    var emailLog = new CreateEmailLogCommand { IsHtml = response.Data.IsHtml, Message = response.Data.Message, Subject = response.Data.Subject, ToRecipient = response.Data.ToRecipient, CreatedBy = emailRequest.CreatedBy, CancellationToken = emailRequest.CancellationToken, BccRecipient = response.Data.BccRecipient, CcRecipient = response.Data.CcRecipient };
                    var createEmailLogRequest = await _emailLogService.CreateEmailLogAsync (emailLog);
                    _logger.LogInformation ($"Email logging ends at {DateTime.UtcNow.AddHours (1)} for email request ID: {response.Data.Id} by userId: {emailRequest.CreatedBy}");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError ($"CreateEmailRequest error occurred service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailRequest.CreatedBy} to recipient: {emailRequest.ToRecipient} with message: {ex.Message}");
                throw;
            }
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> CreateMultipleEmailRequestAsync (List<CreateEmailCommand> emailRequests)
        {
            try
            {
                _logger.LogInformation ($"CreateEmailRequest begins service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for DeletedBy: {emailRequests.First ().CreatedBy}");
                var payload = _mapper.Map<List<EmailRequestDto>> (emailRequests);

                RequestResponse<List<EmailRequestResponse>> responses = await _emailRequestRepository.CreateMultipleEmailRequestAsync (payload);
                _logger.LogInformation ($"CreateEmailRequest ends service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for DeletedBy: {emailRequests.First ().CreatedBy}");

                if (responses.IsSuccessful && responses.Data != null)
                {
                    _logger.LogInformation ($"Email logging begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequests.First ().CreatedBy}");

                    List<CreateEmailLogCommand> emailLogs = [];
                    foreach (var response in responses.Data)
                    {
                        var emailLog = new CreateEmailLogCommand { IsHtml = response.IsHtml, Message = response.Message, Subject = response.Subject, ToRecipient = response.ToRecipient, CreatedBy = emailRequests.First ().CreatedBy, BccRecipient = response.BccRecipient, CcRecipient = response.CcRecipient, CancellationToken = emailRequests.First ().CancellationToken };

                        emailLogs.Add (emailLog);
                    }

                    var createEmailLogRequest = await _emailLogService.CreateMultipleEmailLogsAsync (emailLogs);
                    _logger.LogInformation ($"Email logging ends at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequests.First ().CreatedBy}");
                }
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError ($"CreateEmailRequest error occurred service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for DeletedBy: {emailRequests.First ().CreatedBy} with message: {ex.Message}");
                throw;
            }
        }
        public async Task<RequestResponse<EmailRequestResponse>> DeleteEmailRequestAsync (DeleteEmailCommand request)
        {
            var response = await _emailRequestRepository.DeleteEmailRequestAsync (request);
            return response;
        }

        public async Task<RequestResponse<EmailRequestResponse>> DeleteMultipleEmailRequestsAsync (DeleteMultipleEmailCommand request)
        {
            var response = await _emailRequestRepository.DeleteMultipleEmailRequestsAsync (request);
            return response;
        }

        public async Task<RequestResponse<EmailRequestResponse>> GetAllEmailRequestCountAsync (CancellationToken cancellationToken)
        {
            var response = await _emailRequestRepository.GetAllEmailRequestCountAsync (cancellationToken);
            return response;
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            ValidationResponse validatePagination = Utility.ValidatePagination (pageNumber, pageSize);
            if (!validatePagination.IsValid)
            {
                return RequestResponse<List<EmailRequestResponse>>.Failed (null, 400, validatePagination.Remark);
            }
            var response = await _emailRequestRepository.GetEmailRequestByHtmlStatusAsync (status, cancellationToken, pageNumber, pageSize);
            return response;
        }

        public async Task<RequestResponse<EmailRequestResponse>> GetEmailRequestByIdAsync (long id, CancellationToken cancellationToken)
        {
            var response = await _emailRequestRepository.GetEmailRequestByIdAsync (id, cancellationToken);
            return response;
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByRecipientAsync (string recipientEmailAddress, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.ValidateQueryParameterAndPagination (recipientEmailAddress, null, pageNumber, pageSize);
            if (!validateQueryAndPagination.IsValid)
            {
                return RequestResponse<List<EmailRequestResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
            }
            recipientEmailAddress = validateQueryAndPagination.DecodedString;
            var response = await _emailRequestRepository.GetEmailRequestByRecipientAsync (recipientEmailAddress, cancellationToken, pageNumber, pageSize);
            return response;
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.ValidateQueryParameterAndPagination (id, null, pageNumber, pageSize);
            if (!validateQueryAndPagination.IsValid)
            {
                return RequestResponse<List<EmailRequestResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
            }
            id = validateQueryAndPagination.DecodedString;
            var response = await _emailRequestRepository.GetEmailRequestByUserIdAsync (id, cancellationToken, pageNumber, pageSize);
            return response;
        }

    }
}

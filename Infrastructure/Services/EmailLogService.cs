using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailLogs.Command;
using Application.Models.EmailLogs.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EmailLogService : IEmailLogService
    {
        private readonly IEmailLogRepository _emailLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<IEmailLogService> _logger;
        public EmailLogService (IEmailLogRepository emailLogRepository, IMapper mapper, ILogger<IEmailLogService> logger)
        {
            _emailLogRepository = emailLogRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RequestResponse<EmailLogResponse>> CreateEmailLogAsync (CreateEmailLogCommand emailLog)
        {
            try
            {
                var payload = _mapper.Map<EmailLogDto> (emailLog);

                var response = await _emailLogRepository.CreateEmailLogAsync (payload);
                return response;
            }
            catch (Exception ex)
            {
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateEmailLogAsync), nameof (emailLog.CreatedBy), emailLog.CreatedBy, ex.Message);
				_logger.LogError (errorLog);
				return RequestResponse<EmailLogResponse>.Error (null);
			}
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> CreateMultipleEmailLogsAsync (List<CreateEmailLogCommand> emailLogs)
        {
            try
            {
                var payload = _mapper.Map<List<EmailLogDto>> (emailLogs);

                var response = await _emailLogRepository.CreateMultipleEmailLogsAsync (payload);

                return response;
            }
            catch (Exception ex)
            {
				string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateMultipleEmailLogsAsync), ex.Message);
				_logger.LogError (errorLog);
				return RequestResponse<List<EmailLogResponse>>.Error (null);
			}
        }

        public async Task<RequestResponse<EmailLogResponse>> DeleteEmailLogAsync (DeleteEmailLogCommand request)
        {
            var response = await _emailLogRepository.DeleteEmailLogAsync (request);
            return response;
        }

        public async Task<RequestResponse<EmailLogResponse>> DeleteMultipleEmailLogsAsync (DeleteMultipleEmailLogsCommand request)
        {
            var response = await _emailLogRepository.DeleteMultipleEmailLogsAsync (request);
            return response;
        }

        public async Task<RequestResponse<EmailLogResponse>> GetAllEmailLogCountAsync (CancellationToken cancellationToken)
        {
            var response = await _emailLogRepository.GetAllEmailLogCountAsync (cancellationToken);
            return response;
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
        {
            ValidationResponse validatePagination = Utility.ValidatePagination (page, pageSize);
            if (!validatePagination.IsValid)
            {
                return RequestResponse<List<EmailLogResponse>>.Failed (null, 400, validatePagination.Remark);
            }
            var response = await _emailLogRepository.GetEmailLogByHtmlStatusAsync (status, cancellationToken, page, pageSize);
            return response;
        }

        public async Task<RequestResponse<EmailLogResponse>> GetEmailLogByIdAsync (long id, CancellationToken cancellationToken)
        {
            var response = await _emailLogRepository.GetEmailLogByIdAsync (id, cancellationToken);
            return response;
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogBySentStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
        {
            ValidationResponse validatePagination = Utility.ValidatePagination (page, pageSize);
            if (!validatePagination.IsValid)
            {
                return RequestResponse<List<EmailLogResponse>>.Failed (null, 400, validatePagination.Remark);
            }
            var response = await _emailLogRepository.GetEmailLogBySentStatusAsync (status, cancellationToken, page, pageSize);
            return response;
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
        {
            ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.ValidateQueryParameterAndPagination (id, null, page, pageSize);
            if (!validateQueryAndPagination.IsValid)
            {
                return RequestResponse<List<EmailLogResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
            }
            id = validateQueryAndPagination.DecodedString;
            var response = await _emailLogRepository.GetEmailLogByUserIdAsync (id, cancellationToken, page, pageSize);
            return response;
        }

        public async Task<RequestResponse<EmailLogResponse>> GetUnsentEmailLogCountAsync (CancellationToken cancellationToken)
        {
            var response = await _emailLogRepository.GetUnsentEmailLogCountAsync (cancellationToken);
            return response;
        }

        public async Task<RequestResponse<EmailLogResponse>> UpdateEmailLogSentStatusAsync (UpdateEmailLogSentStatusCommand request)
        {
            var response = await _emailLogRepository.UpdateEmailLogSentStatusAsync (request);

            return response;
        }
    }
}

using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailTemplates.Command;
using Application.Models.EmailTemplates.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
	public class EmailTemplateService : IEmailTemplateService
	{
		private readonly IEmailTemplateRepository _emailTemplateRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<IEmailTemplateService> _logger;

		public EmailTemplateService (IEmailTemplateRepository emailTemplateRepository, IMapper mapper, ILogger<IEmailTemplateService> logger)
		{
			_emailTemplateRepository = emailTemplateRepository;
			_mapper = mapper;
			_logger = logger;
		}
		public async Task<RequestResponse<EmailTemplateResponse>> CreateEmailTemplateAsync (CreateEmailTemplateCommand emailTemplate)
		{
			try
			{
				_logger.LogInformation ($"CreateEmailTemplate begins service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailTemplate.UserId} for template name: {emailTemplate.TemplateName}");
				var payload = _mapper.Map<EmailTemplateDto> (emailTemplate);
				var response = await _emailTemplateRepository.CreateEmailTemplateAsync (payload);
				_logger.LogInformation ($"CreateEmailTemplate ends service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailTemplate.UserId} for template name: {emailTemplate.TemplateName}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateEmailTemplate error occurred service-level mapping to DTO at {DateTime.UtcNow.AddHours (1)} for UserPublicId: {emailTemplate.UserId} for template name: {emailTemplate.TemplateName} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> DeleteEmailTemplateAsync (DeleteEmailTemplateCommand request)
		{
			var response = await _emailTemplateRepository.DeleteEmailTemplateAsync (request);
			return response;
		}

		public async Task<RequestResponse<EmailTemplateResponse>> DeleteMultipleEmailTemplatesAsync (DeleteMultipleEmailTemplatesCommand request)
		{
			var response = await _emailTemplateRepository.DeleteMultipleEmailTemplatesAsync (request);
			return response;
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetAllEmailTemplateCountAsync (CancellationToken cancellationToken)
		{
			var response = await _emailTemplateRepository.GetAllEmailTemplateCountAsync (cancellationToken);
			return response;
		}

		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByChannelNameAsync (string name, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.ValidateQueryParameterAndPagination (name, null, pageNumber, pageSize);
			if (!validateQueryAndPagination.IsValid)
			{
				return RequestResponse<List<EmailTemplateResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
			}
			name = validateQueryAndPagination.DecodedString;
			var response = await _emailTemplateRepository.GetEmailTemplateByChannelNameAsync (name, cancellationToken, pageNumber, pageSize);
			return response;
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByIdAsync (long id, CancellationToken cancellationToken)
		{
			var response = await _emailTemplateRepository.GetEmailTemplateByIdAsync (id, cancellationToken);
			return response;
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByTemplateNameAsync (string name, CancellationToken cancellationToken)
		{
			ValidateQueryParameterAndPaginationResponse validateQueryParameter = Utility.ValidateQueryParameter (name, null);
			if (!validateQueryParameter.IsValid)
			{
				return RequestResponse<EmailTemplateResponse>.Failed (null, 400, validateQueryParameter.Remark);
			}
			name = validateQueryParameter.DecodedString;
			var response = await _emailTemplateRepository.GetEmailTemplateByTemplateNameAsync (name, cancellationToken);
			return response;
		}

		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			ValidateQueryParameterAndPaginationResponse validateQueryAndPagination = Utility.ValidateQueryParameterAndPagination (id, null, pageNumber, pageSize);
			if (!validateQueryAndPagination.IsValid)
			{
				return RequestResponse<List<EmailTemplateResponse>>.Failed (null, 400, validateQueryAndPagination.Remark);
			}
			id = validateQueryAndPagination.DecodedString;
			var response = await _emailTemplateRepository.GetEmailTemplateByUserIdAsync (id, cancellationToken, pageNumber, pageSize);
			return response;
		}
		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetAllEmailTemplateAsync (CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			ValidationResponse validatePagination = Utility.ValidatePagination (pageNumber, pageSize);
			if (!validatePagination.IsValid)
			{
				return RequestResponse<List<EmailTemplateResponse>>.Failed (null, 400, validatePagination.Remark);
			}
			var response = await _emailTemplateRepository.GetAllEmailTemplateAsync (cancellationToken, pageNumber, pageSize);
			return response;
		}
	}
}

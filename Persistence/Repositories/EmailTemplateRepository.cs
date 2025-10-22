using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailTemplates.Command;
using Application.Models.EmailTemplates.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories
{
	public class EmailTemplateRepository : IEmailTemplateRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<IEmailTemplateRepository> _logger;
		public EmailTemplateRepository (ApplicationDbContext context, IMapper mapper, ILogger<IEmailTemplateRepository> logger)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
		}

		public async Task<RequestResponse<EmailTemplateResponse>> CreateEmailTemplateAsync (EmailTemplateDto emailTemplate)
		{
			try
			{
				_logger.LogInformation ($"CreateEmailTemplate begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailTemplate.CreatedBy}");

				var payload = _mapper.Map<EmailTemplate> (emailTemplate);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);

				await _context.EmailTemplates.AddAsync (payload, emailTemplate.CancellationToken);
				await _context.SaveChangesAsync (emailTemplate.CancellationToken);

				var response = _mapper.Map<EmailTemplateResponse> (payload);
				var result = RequestResponse<EmailTemplateResponse>.Created (response, 1, "Email template");
				_logger.LogInformation ($"CreateEmailTemplate at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailTemplate.CreatedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateEmailTemplate by UserPublicId: {emailTemplate.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> DeleteEmailTemplateAsync (DeleteEmailTemplateCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteEmailTemplate begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				var check = await _context.EmailTemplates
					.Where (x => x.Id == request.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (request.CancellationToken);

				if (check == null)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");
					_logger.LogInformation ($"DeleteEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				check.IsDeleted = true;
				check.DeletedBy = request.UserId;
				check.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.EmailTemplates.Update (check);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<EmailTemplateResponse>.Deleted (null, 1, "Email template");
				_logger.LogInformation ($"DeleteEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteEmailTemplate by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> DeleteMultipleEmailTemplatesAsync (DeleteMultipleEmailTemplatesCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteMultipleEmailTemplates begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				List<EmailTemplate> emailTemplates = [];
				foreach (long id in request.Ids)
				{
					var check = await _context.EmailTemplates
						.Where (x => x.Id == id && x.IsDeleted == false)
						.FirstOrDefaultAsync (request.CancellationToken);

					if (check == null)
					{
						var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");
						_logger.LogInformation ($"DeleteMultipleEmailTemplates ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
						return badRequest;
					}
					check.IsDeleted = true;
					check.DeletedBy = request.UserId;
					check.DateDeleted = DateTime.UtcNow.AddHours (1);

					emailTemplates.Add (check);
				}

				if (emailTemplates.Count < 1)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");
					_logger.LogInformation ($"DeleteMultipleEmailTemplates ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_context.EmailTemplates.UpdateRange (emailTemplates);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<EmailTemplateResponse>.Deleted (null, emailTemplates.Count, "Email templates");
				_logger.LogInformation ($"DeleteMultipleEmailTemplates ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteMultipleEmailTemplates by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetAllEmailTemplateCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAllEmailTemplateCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<EmailTemplateResponse>.CountSuccessful (null, count, "Email templates");
				_logger.LogInformation ($"GetAllEmailTemplateCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllEmailTemplateCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByChannelNameAsync (string name, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailTemplateById begins at {DateTime.UtcNow.AddHours (1)} for channel name: {name}");
				var result = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Channel == name)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<EmailTemplateResponse>>.NotFound (null, "Email templates");
					_logger.LogInformation ($"GetEmailTemplateById ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for channel name: {name}");
					return badRequest;
				}

				var count = await _context.EmailTemplates
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.Channel == name).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");
				_logger.LogInformation ($"GetEmailTemplateById ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for channel name: {name}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailTemplateById exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for channel name: {name}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByIdAsync (long id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetEmailTemplateById begins at {DateTime.UtcNow.AddHours (1)} for Id: {id}");
				var result = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Id == id)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");
					_logger.LogInformation ($"GetEmailTemplateById ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for Id: {id}");
					return badRequest;
				}

				var response = RequestResponse<EmailTemplateResponse>.SearchSuccessful (result, 1, "Email templates");
				_logger.LogInformation ($"GetEmailTemplateById ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for Id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailTemplateById exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for Id: {id}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByTemplateNameAsync (string name, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetEmailTemplateByTemplateName begins at {DateTime.UtcNow.AddHours (1)} for template name: {name}");
				var result = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.TemplateName == name)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");
					_logger.LogInformation ($"GetEmailTemplateByTemplateName ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for template name: {name}");
					return badRequest;
				}

				var response = RequestResponse<EmailTemplateResponse>.SearchSuccessful (result, 1, "Email template");
				_logger.LogInformation ($"GetEmailTemplateByTemplateName ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for template name: {name}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailTemplateByTemplateName exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for template name: {name}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailTemplateByUserId begins at {DateTime.UtcNow.AddHours (1)} for Id: {id}");
				var result = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.CreatedBy == id)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<EmailTemplateResponse>>.NotFound (null, "Email templates");
					_logger.LogInformation ($"GetEmailTemplateByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for Id: {id}");
					return badRequest;
				}

				var count = await _context.EmailTemplates
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.CreatedBy == id).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");
				_logger.LogInformation ($"GetEmailTemplateByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for Id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailTemplateByUserId exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for Id: {id}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailTemplateResponse>>> GetAllEmailTemplateAsync (CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllEmailTemplate begins at {DateTime.UtcNow.AddHours (1)}");
				var result = await _context.EmailTemplates
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<EmailTemplateResponse>>.NotFound (null, "Email templates");
					_logger.LogInformation ($"GetAllEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var count = await _context.EmailTemplates
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");
				_logger.LogInformation ($"GetAllEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllEmailTemplate exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailTemplateResponse>> UpdateEmailTemplateAsync (EmailTemplateDto emailTemplate)
		{
			try
			{
				_logger.LogInformation ($"UpdateEmailTemplate begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailTemplate.LastModifiedBy} for email log with Id: {emailTemplate.Id}");
				if (emailTemplate == null)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var updateTemplate = await _context.EmailTemplates
					.Where (x => x.Id == emailTemplate.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (emailTemplate.CancellationToken);

				if (updateTemplate == null)
				{
					var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");
					_logger.LogInformation ($"UpdateEmailTemplate ends at {DateTime.UtcNow.AddHours (1)} by userId: {emailTemplate.LastModifiedBy} for email Template with Id: {emailTemplate.Id}");
					return badRequest;
				}

				updateTemplate.Channel = emailTemplate.Channel;
				updateTemplate.Template = emailTemplate.Template;
				updateTemplate.TemplateName = emailTemplate.TemplateName;
				updateTemplate.LastModifiedBy = emailTemplate.LastModifiedBy;
				updateTemplate.LastModifiedDate = DateTime.UtcNow.AddHours (1);

				_context.EmailTemplates.Update (updateTemplate);
				await _context.SaveChangesAsync ();

				var result = _mapper.Map<EmailTemplateResponse> (updateTemplate);
				var response = RequestResponse<EmailTemplateResponse>.Updated (result, 1, "Email template");
				_logger.LogInformation ($"UpdateEmailTemplate at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by userId: {emailTemplate.LastModifiedBy} for email Template with Id: {emailTemplate.Id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateEmailTemplate exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

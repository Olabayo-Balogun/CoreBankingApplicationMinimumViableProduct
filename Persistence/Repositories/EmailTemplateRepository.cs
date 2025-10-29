using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailTemplates.Command;
using Application.Models.EmailTemplates.Response;
using Application.Utility;

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
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateEmailTemplateAsync), nameof (emailTemplate.CreatedBy), emailTemplate.CreatedBy);
                _logger.LogInformation (openingLog);

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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateEmailTemplateAsync), nameof (emailTemplate.CreatedBy), emailTemplate.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateEmailTemplateAsync), nameof (emailTemplate.CreatedBy), emailTemplate.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> DeleteEmailTemplateAsync (DeleteEmailTemplateCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteEmailTemplateAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var check = await _context.EmailTemplates
                    .Where (x => x.Id == request.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (request.CancellationToken);

                if (check == null)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailTemplateAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                check.IsDeleted = true;
                check.DeletedBy = request.DeletedBy;
                check.DateDeleted = DateTime.UtcNow.AddHours (1);

                _context.EmailTemplates.Update (check);
                await _context.SaveChangesAsync ();

                var result = RequestResponse<EmailTemplateResponse>.Deleted (null, 1, "Email template");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailTemplateAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteEmailTemplateAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> DeleteMultipleEmailTemplatesAsync (DeleteMultipleEmailTemplatesCommand request)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleEmailTemplatesAsync), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (initiationLog);

                List<EmailTemplate> emailTemplates = [];
                foreach (long id in request.Ids)
                {
                    var check = await _context.EmailTemplates
                        .Where (x => x.Id == id && x.IsDeleted == false)
                        .FirstOrDefaultAsync (request.CancellationToken);

                    if (check == null)
                    {
                        var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailTemplatesAsync), nameof (id), id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    check.IsDeleted = true;
                    check.DeletedBy = request.DeletedBy;
                    check.DateDeleted = DateTime.UtcNow.AddHours (1);

                    emailTemplates.Add (check);
                }

                if (emailTemplates.Count < 1)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailTemplatesAsync), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                _context.EmailTemplates.UpdateRange (emailTemplates);
                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<EmailTemplateResponse>.Deleted (null, emailTemplates.Count, "Email templates");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailTemplatesAsync), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteMultipleEmailTemplatesAsync), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> GetAllEmailTemplateCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllEmailTemplateCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.EmailTemplates
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<EmailTemplateResponse>.CountSuccessful (null, count, "Email templates");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllEmailTemplateCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllEmailTemplateCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByChannelNameAsync (string name, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailTemplateByChannelNameAsync), nameof (name), name);
                _logger.LogInformation (openingLog);

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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByChannelNameAsync), nameof (name), name, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailTemplates
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.Channel == name).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByChannelNameAsync), nameof (name), name, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailTemplateByChannelNameAsync), nameof (name), name, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailTemplateResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailTemplateByIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailTemplates
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.Id == id)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email templates");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<EmailTemplateResponse>.SearchSuccessful (result, 1, "Email templates");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailTemplateByIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> GetEmailTemplateByTemplateNameAsync (string name, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailTemplateByTemplateNameAsync), nameof (name), name);
                _logger.LogInformation (openingLog);

                var result = await _context.EmailTemplates
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.TemplateName == name)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailTemplateResponse { Channel = x.Channel, Id = x.Id, Template = x.Template, TemplateName = x.TemplateName })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByTemplateNameAsync), nameof (name), name, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<EmailTemplateResponse>.SearchSuccessful (result, 1, "Email template");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByTemplateNameAsync), nameof (name), name, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailTemplateByTemplateNameAsync), nameof (name), name, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailTemplateResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailTemplateResponse>>> GetEmailTemplateByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailTemplateByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailTemplates
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.CreatedBy == id).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailTemplateByUserIdAsync), nameof (id), id, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailTemplateByUserIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailTemplateResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailTemplateResponse>>> GetAllEmailTemplateAsync (CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllEmailTemplateAsync));
                _logger.LogInformation (openingLog);

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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllEmailTemplateAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailTemplates
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailTemplateResponse>>.SearchSuccessful (result, count, "Email templates");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllEmailTemplateAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllEmailTemplateAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailTemplateResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailTemplateResponse>> UpdateEmailTemplateAsync (EmailTemplateDto emailTemplate)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateEmailTemplateAsync), nameof (emailTemplate.Id), emailTemplate.Id.GetValueOrDefault ().ToString (), nameof (emailTemplate.LastModifiedBy), emailTemplate.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (emailTemplate == null)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailTemplateAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateTemplate = await _context.EmailTemplates
                    .Where (x => x.Id == emailTemplate.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (emailTemplate.CancellationToken);

                if (updateTemplate == null)
                {
                    var badRequest = RequestResponse<EmailTemplateResponse>.NotFound (null, "Email template");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailTemplateAsync), nameof (emailTemplate.Id), emailTemplate.Id.GetValueOrDefault ().ToString (), nameof (emailTemplate.LastModifiedBy), emailTemplate.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailTemplateAsync), nameof (emailTemplate.Id), emailTemplate.Id.GetValueOrDefault ().ToString (), nameof (emailTemplate.LastModifiedBy), emailTemplate.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateEmailTemplateAsync), nameof (emailTemplate.Id), emailTemplate.Id.GetValueOrDefault ().ToString (), nameof (emailTemplate.LastModifiedBy), emailTemplate.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                throw;
            }
        }
    }
}

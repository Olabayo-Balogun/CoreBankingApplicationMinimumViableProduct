using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailLogs.Command;
using Application.Models.EmailLogs.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Repositories
{
    public class EmailLogRepository : IEmailLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IEmailLogRepository> _logger;
        private readonly AppSettings _appSettings;

        public EmailLogRepository (ApplicationDbContext context, IMapper mapper, ILogger<IEmailLogRepository> logger, IOptions<AppSettings> appsettings)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _appSettings = appsettings.Value;
        }

        public async Task<RequestResponse<EmailLogResponse>> CreateEmailLogAsync (EmailLogDto emailLog)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateEmailLogAsync), nameof (emailLog.CreatedBy), emailLog.CreatedBy);
                _logger.LogInformation (openingLog);

                var payload = _mapper.Map<EmailLog> (emailLog);

                payload.IsDeleted = false;
                payload.DateDeleted = null;
                payload.LastModifiedBy = null;
                payload.LastModifiedDate = null;
                payload.DeletedBy = null;
                payload.DateCreated = DateTime.UtcNow.AddHours (1);
                payload.IsSent = false;
                payload.BccRecipient = string.IsNullOrEmpty (payload.BccRecipient) ? null : payload.BccRecipient;
                payload.CcRecipient = string.IsNullOrEmpty (payload.CcRecipient) ? null : payload.CcRecipient;
                payload.ToRecipient = string.IsNullOrEmpty (payload.ToRecipient) ? "null" : payload.ToRecipient;
                payload.Sender = _appSettings.EmailSender;

                await _context.EmailLogs.AddAsync (payload, emailLog.CancellationToken);
                await _context.SaveChangesAsync (emailLog.CancellationToken);

                var response = _mapper.Map<EmailLogResponse> (payload);
                var result = RequestResponse<EmailLogResponse>.Created (response, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateEmailLogAsync), nameof (emailLog.CreatedBy), emailLog.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateEmailLogAsync), nameof (emailLog.CreatedBy), emailLog.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> CreateMultipleEmailLogsAsync (List<EmailLogDto> emailLogs)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleEmailLogsAsync));
                _logger.LogInformation (initiationLog);

                foreach (var emailLog in emailLogs)
                {
                    string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleEmailLogsAsync), nameof (emailLog.CreatedBy), emailLog.CreatedBy);
                    _logger.LogInformation (openingLog);

                    emailLog.DateCreated = DateTime.UtcNow.AddHours (1);
                    emailLog.Sender = _appSettings.EmailSender;
                    emailLog.IsSent = false;
                }

                var payload = _mapper.Map<List<EmailLog>> (emailLogs);

                await _context.EmailLogs.AddRangeAsync (payload);
                await _context.SaveChangesAsync (emailLogs.First ().CancellationToken);

                var response = _mapper.Map<List<EmailLogResponse>> (payload);
                var result = RequestResponse<List<EmailLogResponse>>.Created (response, response.Count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleEmailLogsAsync), result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
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
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteEmailLogAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var check = await _context.EmailLogs
                    .Where (x => x.Id == request.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (request.CancellationToken);

                if (check == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailLogAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                check.IsDeleted = true;
                check.DeletedBy = request.DeletedBy;
                check.DateDeleted = DateTime.UtcNow.AddHours (1);

                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<EmailLogResponse>.Deleted (null, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailLogAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteEmailLogAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> DeleteMultipleEmailLogsAsync (DeleteMultipleEmailLogsCommand request)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleEmailLogsAsync), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (initiationLog);

                List<EmailLog> emailLogs = [];
                foreach (long id in request.Ids)
                {
                    string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleEmailLogsAsync), nameof (id), id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                    _logger.LogInformation (openingLog);

                    var check = await _context.EmailLogs.Where (x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                    if (check == null)
                    {
                        var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email logs");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailLogsAsync), nameof (id), id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }
                    check.IsDeleted = true;
                    check.DeletedBy = request.DeletedBy;
                    check.DateDeleted = DateTime.UtcNow.AddHours (1);
                    emailLogs.Add (check);
                }

                if (emailLogs.Count < 1)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailLogsAsync), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }


                await _context.SaveChangesAsync ();

                var result = RequestResponse<EmailLogResponse>.Deleted (null, emailLogs.Count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailLogsAsync), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteMultipleEmailLogsAsync), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> GetAllEmailLogCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllEmailLogCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<EmailLogResponse>.CountSuccessful (null, count, "Email logs");


                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllEmailLogCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllEmailLogCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailLogByHtmlStatusAsync), nameof (status), status.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.IsHtml == status)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByHtmlStatusAsync), nameof (status), status.ToString (), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.IsHtml == status).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByHtmlStatusAsync), nameof (status), status.ToString (), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailLogByHtmlStatusAsync), nameof (status), status.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailLogResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> GetEmailLogByIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailLogByIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.Id == id)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<EmailLogResponse>.SearchSuccessful (result, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailLogByIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogBySentStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailLogBySentStatusAsync), nameof (status), status.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.IsSent == status)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogBySentStatusAsync), nameof (status), status.ToString (), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.IsSent == status).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogBySentStatusAsync), nameof (status), status.ToString (), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailLogBySentStatusAsync), nameof (status), status.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailLogResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByUserIdAsync (string userId, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailLogByUserIdAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

                var result = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.CreatedBy == userId)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByUserIdAsync), nameof (userId), userId, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.CreatedBy == userId).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailLogByUserIdAsync), nameof (userId), userId, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailLogByUserIdAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailLogResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> GetUnsentEmailLogCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUnsentEmailLogCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.EmailLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.IsSent == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<EmailLogResponse>.CountSuccessful (null, count, "Email logs");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUnsentEmailLogCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUnsentEmailLogCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> UpdateEmailLogAsync (EmailLogDto emailLog)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateEmailLogAsync), nameof (emailLog.Id), emailLog.Id.GetValueOrDefault ().ToString (), nameof (emailLog.LastModifiedBy), emailLog.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (emailLog == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateRequest = await _context.EmailLogs
                    .Where (x => x.Id == emailLog.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (emailLog.CancellationToken);

                if (updateRequest == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogAsync), nameof (emailLog.Id), emailLog.Id.GetValueOrDefault ().ToString (), nameof (emailLog.LastModifiedBy), emailLog.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                updateRequest.Sender = emailLog.Sender;
                updateRequest.Subject = emailLog.Subject;
                updateRequest.IsHtml = emailLog.IsHtml;
                updateRequest.BccRecipient = emailLog.BccRecipient;
                updateRequest.CcRecipient = emailLog.CcRecipient;
                updateRequest.Message = emailLog.Message;
                updateRequest.ToRecipient = emailLog.ToRecipient;
                updateRequest.LastModifiedBy = emailLog.LastModifiedBy;
                updateRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateRequest.IsSent = false;

                await _context.SaveChangesAsync (emailLog.CancellationToken);

                var result = _mapper.Map<EmailLogResponse> (updateRequest);
                var response = RequestResponse<EmailLogResponse>.Updated (result, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogAsync), nameof (emailLog.Id), emailLog.Id.GetValueOrDefault ().ToString (), nameof (emailLog.LastModifiedBy), emailLog.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateEmailLogAsync), nameof (emailLog.Id), emailLog.Id.GetValueOrDefault ().ToString (), nameof (emailLog.LastModifiedBy), emailLog.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailLogResponse>> UpdateEmailLogSentStatusAsync (UpdateEmailLogSentStatusCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString ());
                _logger.LogInformation (openingLog);

                if (request == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogSentStatusAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateRequest = await _context.EmailLogs
                    .Where (x => x.Id == request.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (request.CancellationToken);

                if (updateRequest == null)
                {
                    var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                updateRequest.IsSent = request.IsSent;
                updateRequest.DateSent = request.IsSent == true ? request.DateSent : null;

                updateRequest.LastModifiedBy = request.LastModifiedBy;

                await _context.SaveChangesAsync (request.CancellationToken);

                var result = _mapper.Map<EmailLogResponse> (updateRequest);
                var response = RequestResponse<EmailLogResponse>.Updated (result, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailLogResponse>>> UpdateMultipleEmailLogSentStatusAsync (List<UpdateEmailLogSentStatusCommand> requests)
        {
            try
            {
                List<EmailLog> emailLogs = [];
                foreach (var request in requests)
                {
                    string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateMultipleEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString ());
                    _logger.LogInformation (openingLog);

                    if (request == null)
                    {
                        var badRequest = RequestResponse<List<EmailLogResponse>>.NullPayload (null);

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleEmailLogSentStatusAsync), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var updateRequest = await _context.EmailLogs
                        .Where (x => x.Id == request.Id && x.IsDeleted == false)
                        .FirstOrDefaultAsync (request.CancellationToken);

                    if (updateRequest == null)
                    {
                        var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleEmailLogSentStatusAsync), nameof (request.Id), request.Id.ToString (), nameof (request.IsSent), request.IsSent.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    updateRequest.IsSent = request.IsSent;
                    updateRequest.DateSent = request.IsSent == true ? request.DateSent : null;

                    updateRequest.LastModifiedBy = request.LastModifiedBy;
                    updateRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);

                    emailLogs.Add (updateRequest);
                }

                if (emailLogs.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logS");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleEmailLogSentStatusAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (requests.First ().CancellationToken);
                var result = _mapper.Map<List<EmailLogResponse>> (emailLogs);
                var response = RequestResponse<List<EmailLogResponse>>.Updated (result, 1, "Email log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateMultipleEmailLogSentStatusAsync), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateMultipleEmailLogSentStatusAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailLogResponse>>.Error (null);
            }
        }
    }
}

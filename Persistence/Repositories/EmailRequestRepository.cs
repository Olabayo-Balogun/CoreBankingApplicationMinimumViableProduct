using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories
{
    public class EmailRequestRepository : IEmailRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IEmailRequestRepository> _logger;
        public EmailRequestRepository (ApplicationDbContext context, IMapper mapper, ILogger<IEmailRequestRepository> logger)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }
        public async Task<RequestResponse<EmailRequestResponse>> CreateEmailRequestAsync (EmailRequestDto emailRequest)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateEmailRequestAsync), nameof (emailRequest.CreatedBy), emailRequest.CreatedBy);
                _logger.LogInformation (openingLog);

                var payload = _mapper.Map<EmailRequest> (emailRequest);

                payload.IsDeleted = false;
                payload.DateDeleted = null;
                payload.LastModifiedBy = null;
                payload.LastModifiedDate = null;
                payload.DeletedBy = null;
                payload.BccRecipient = string.IsNullOrEmpty (payload.BccRecipient) ? null : payload.BccRecipient;
                payload.CcRecipient = string.IsNullOrEmpty (payload.CcRecipient) ? null : payload.CcRecipient;
                payload.ToRecipient = string.IsNullOrEmpty (payload.ToRecipient) ? "null" : payload.ToRecipient;
                payload.DateCreated = DateTime.UtcNow.AddHours (1);

                await _context.EmailRequests.AddAsync (payload, emailRequest.CancellationToken);
                await _context.SaveChangesAsync (emailRequest.CancellationToken);

                var response = _mapper.Map<EmailRequestResponse> (payload);
                var result = RequestResponse<EmailRequestResponse>.Created (response, 1, "Email request");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateEmailRequestAsync), nameof (emailRequest.CreatedBy), emailRequest.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateEmailRequestAsync), nameof (emailRequest.CreatedBy), emailRequest.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<EmailRequestResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> CreateMultipleEmailRequestAsync (List<EmailRequestDto> emailRequests)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleEmailRequestAsync));
                _logger.LogInformation (initiationLog);

                emailRequests.ForEach (x => x.DateCreated = DateTime.UtcNow.AddHours (1));
                var payload = _mapper.Map<List<EmailRequest>> (emailRequests);

                await _context.EmailRequests.AddRangeAsync (payload);
                await _context.SaveChangesAsync (emailRequests.First ().CancellationToken);

                var response = _mapper.Map<List<EmailRequestResponse>> (payload);
                var result = RequestResponse<List<EmailRequestResponse>>.Created (response, response.Count, "Email requests");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleEmailRequestAsync), result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateMultipleEmailRequestAsync), ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<List<EmailRequestResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailRequestResponse>> DeleteEmailRequestAsync (DeleteEmailCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteEmailRequestAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var check = await _context.EmailRequests
                    .Where (x => x.Id == request.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (request.CancellationToken);

                if (check == null)
                {
                    var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailRequestAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                check.IsDeleted = true;
                check.DeletedBy = request.DeletedBy;
                check.DateDeleted = DateTime.UtcNow.AddHours (1);

                _context.EmailRequests.Update (check);
                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<EmailRequestResponse>.Deleted (null, 1, "Email request");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteEmailRequestAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteEmailRequestAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailRequestResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailRequestResponse>> DeleteMultipleEmailRequestsAsync (DeleteMultipleEmailCommand request)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleEmailRequestsAsync), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (initiationLog);

                List<EmailRequest> emailRequests = [];
                foreach (long id in request.Ids)
                {
                    var check = await _context.EmailRequests
                        .Where (x => x.Id == id && x.IsDeleted == false)
                        .FirstOrDefaultAsync (request.CancellationToken);

                    if (check == null)
                    {
                        var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email requests");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailRequestsAsync), nameof (id), id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }
                    check.IsDeleted = true;
                    check.DeletedBy = request.DeletedBy;
                    check.DateDeleted = DateTime.UtcNow.AddHours (1);

                    emailRequests.Add (check);
                }

                if (emailRequests.Count < 1)
                {
                    var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailRequestsAsync), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                _context.EmailRequests.UpdateRange (emailRequests);
                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<EmailRequestResponse>.Deleted (null, emailRequests.Count, "Email requests");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleEmailRequestsAsync), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteMultipleEmailRequestsAsync), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailRequestResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailRequestResponse>> GetAllEmailRequestCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllEmailRequestCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.EmailRequests
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<EmailRequestResponse>.CountSuccessful (null, count, "Email requests");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllEmailRequestCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllEmailRequestCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailRequestResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailRequestByHtmlStatusAsync), nameof (status), status.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailRequests
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.IsHtml == status)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailRequestResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, Id = x.Id, IsHtml = x.IsHtml, Message = x.Message, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailRequestResponse>>.NotFound (null, "Email requests");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByHtmlStatusAsync), nameof (status), status.ToString (), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.IsHtml == status).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email requests");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByHtmlStatusAsync), nameof (status), status.ToString (), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailRequestByHtmlStatusAsync), nameof (status), status.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailRequestResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailRequestResponse>> GetEmailRequestByIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailRequestByIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.EmailRequests
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.Id == id)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailRequestResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, Id = x.Id, IsHtml = x.IsHtml, Message = x.Message, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<EmailRequestResponse>.SearchSuccessful (result, 1, "Email request");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailRequestByIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailRequestResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByRecipientAsync (string recipientEmailAddress, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailRequestByRecipientAsync), nameof (recipientEmailAddress), recipientEmailAddress);
                _logger.LogInformation (openingLog);

                var result = await _context.EmailRequests
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && (x.ToRecipient == recipientEmailAddress.ToLower ().Trim () || x.CcRecipient == recipientEmailAddress.ToLower ().Trim () || x.BccRecipient == recipientEmailAddress.ToLower ().Trim ()))
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailRequestResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, Id = x.Id, IsHtml = x.IsHtml, Message = x.Message, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailRequestResponse>>.NotFound (null, "Email requests");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByRecipientAsync), nameof (recipientEmailAddress), recipientEmailAddress, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && (x.ToRecipient == recipientEmailAddress.ToLower ().Trim () || x.CcRecipient == recipientEmailAddress.ToLower ().Trim () || x.BccRecipient == recipientEmailAddress.ToLower ().Trim ())).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email requests");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByRecipientAsync), nameof (recipientEmailAddress), recipientEmailAddress, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailRequestByRecipientAsync), nameof (recipientEmailAddress), recipientEmailAddress, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailRequestResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetEmailRequestByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.EmailRequests
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.CreatedBy == id)
                    .OrderByDescending (y => y.DateCreated)
                    .Select (x => new EmailRequestResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, Id = x.Id, IsHtml = x.IsHtml, Message = x.Message, Subject = x.Subject, ToRecipient = x.ToRecipient })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<EmailRequestResponse>>.NotFound (null, "Email requests");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.EmailLogs
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.CreatedBy == id).LongCountAsync (cancellationToken);

                var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetEmailRequestByUserIdAsync), nameof (id), id, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetEmailRequestByUserIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<EmailRequestResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<EmailRequestResponse>> UpdateEmailRequestAsync (EmailRequestDto emailRequest)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateEmailRequestAsync), nameof (emailRequest.Id), emailRequest.Id.GetValueOrDefault ().ToString (), nameof (emailRequest.LastModifiedBy), emailRequest.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (emailRequest == null)
                {
                    var badRequest = RequestResponse<EmailRequestResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailRequestAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateRequest = await _context.EmailRequests
                    .Where (x => x.Id == emailRequest.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (emailRequest.CancellationToken);

                if (updateRequest == null)
                {
                    var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailRequestAsync), nameof (emailRequest.Id), emailRequest.Id.GetValueOrDefault ().ToString (), nameof (emailRequest.LastModifiedBy), emailRequest.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                updateRequest.Subject = emailRequest.Subject;
                updateRequest.IsHtml = emailRequest.IsHtml;
                updateRequest.BccRecipient = emailRequest.BccRecipient;
                updateRequest.CcRecipient = emailRequest.CcRecipient;
                updateRequest.Message = emailRequest.Message;
                updateRequest.ToRecipient = emailRequest.ToRecipient;
                updateRequest.LastModifiedBy = emailRequest.LastModifiedBy;
                updateRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);

                _context.EmailRequests.Update (updateRequest);
                await _context.SaveChangesAsync (emailRequest.CancellationToken);

                var result = _mapper.Map<EmailRequestResponse> (updateRequest);
                var response = RequestResponse<EmailRequestResponse>.Updated (result, 1, "Email request");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateEmailRequestAsync), nameof (emailRequest.Id), emailRequest.Id.GetValueOrDefault ().ToString (), nameof (emailRequest.LastModifiedBy), emailRequest.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;

            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateEmailRequestAsync), nameof (emailRequest.Id), emailRequest.Id.GetValueOrDefault ().ToString (), nameof (emailRequest.LastModifiedBy), emailRequest.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<EmailRequestResponse>.Error (null);
            }
        }
    }
}

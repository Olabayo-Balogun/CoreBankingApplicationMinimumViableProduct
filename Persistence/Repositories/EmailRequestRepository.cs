using Application.Interface.Persistence;
using Application.Model;
using Application.Model.EmailRequests.Command;
using Application.Model.EmailRequests.Queries;

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
				_logger.LogInformation ($"CreateEmailRequest begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequest.CreatedBy}");

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
				_logger.LogInformation ($"CreateEmailRequest at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequest.CreatedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateEmailRequest by UserPublicId: {emailRequest.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailRequestResponse>>> CreateMultipleEmailRequestAsync (List<EmailRequestDto> emailRequests)
		{
			try
			{
				_logger.LogInformation ($"CreateMultipleEmailRequest begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequests.First ().CreatedBy}");
				emailRequests.ForEach (x => x.DateCreated = DateTime.UtcNow.AddHours (1));
				var payload = _mapper.Map<List<EmailRequest>> (emailRequests);

				await _context.EmailRequests.AddRangeAsync (payload);
				await _context.SaveChangesAsync (emailRequests.First ().CancellationToken);

				var response = _mapper.Map<List<EmailRequestResponse>> (payload);
				var result = RequestResponse<List<EmailRequestResponse>>.Created (response, response.Count, "Email requests");
				_logger.LogInformation ($"CreateMultipleEmailRequest at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequests.First ().CreatedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateMultipleEmailRequest by userId: {emailRequests.First ().CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailRequestResponse>> DeleteEmailRequestAsync (DeleteEmailCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteEmailRequest begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				var check = await _context.EmailRequests
					.Where (x => x.Id == request.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (request.CancellationToken);

				if (check == null)
				{
					var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");
					_logger.LogInformation ($"DeleteEmailRequest ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				check.IsDeleted = true;
				check.DeletedBy = request.UserId;
				check.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.EmailRequests.Update (check);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<EmailRequestResponse>.Deleted (null, 1, "Email request");
				_logger.LogInformation ($"DeleteEmailRequest ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteEmailRequest by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailRequestResponse>> DeleteMultipleEmailRequestsAsync (DeleteMultipleEmailCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteMultipleEmailRequests begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				List<EmailRequest> emailRequests = [];
				foreach (long id in request.Ids)
				{
					var check = await _context.EmailRequests
						.Where (x => x.Id == id && x.IsDeleted == false)
						.FirstOrDefaultAsync (request.CancellationToken);

					if (check == null)
					{
						var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email requests");
						_logger.LogInformation ($"DeleteMultipleEmailRequests ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
						return badRequest;
					}
					check.IsDeleted = true;
					check.DeletedBy = request.UserId;
					check.DateDeleted = DateTime.UtcNow.AddHours (1);

					emailRequests.Add (check);
				}

				_context.EmailRequests.UpdateRange (emailRequests);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<EmailRequestResponse>.Deleted (null, emailRequests.Count, "Email requests");
				_logger.LogInformation ($"DeleteMultipleEmailRequests ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteMultipleEmailRequests by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailRequestResponse>> GetAllEmailRequestCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAllEmailRequestCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.EmailRequests
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<EmailRequestResponse>.CountSuccessful (null, count, "Email requests");
				_logger.LogInformation ($"GetAllEmailRequestCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllEmailRequestCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailRequestByHtmlStatus begins at {DateTime.UtcNow.AddHours (1)} for status: {status}");
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
					_logger.LogInformation ($"GetEmailRequestByHtmlStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for status {status}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.IsHtml == status).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email requests");
				_logger.LogInformation ($"GetEmailRequestByHtmlStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for status: {status}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailRequestByHtmlStatus exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for status: {status}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailRequestResponse>> GetEmailRequestByIdAsync (long id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetEmailRequestById begins at {DateTime.UtcNow.AddHours (1)} for Id: {id}");
				var result = await _context.EmailRequests
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Id == id)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailRequestResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, Id = x.Id, IsHtml = x.IsHtml, Message = x.Message, Subject = x.Subject, ToRecipient = x.ToRecipient })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");
					_logger.LogInformation ($"GetEmailRequestById ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for Id: {id}");
					return badRequest;
				}

				var response = RequestResponse<EmailRequestResponse>.SearchSuccessful (result, 1, "Email request");
				_logger.LogInformation ($"GetEmailRequestById ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for Id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailRequestById exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for Id: {id}");
				throw;
			}

		}

		public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByRecipientAsync (string recipientEmailAddress, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailRequestByRecipient begins at {DateTime.UtcNow.AddHours (1)} for recipient with email: {recipientEmailAddress}");
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
					_logger.LogInformation ($"GetEmailRequestByRecipient ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}  for recipient with email: {recipientEmailAddress}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && (x.ToRecipient == recipientEmailAddress.ToLower ().Trim () || x.CcRecipient == recipientEmailAddress.ToLower ().Trim () || x.BccRecipient == recipientEmailAddress.ToLower ().Trim ())).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email requests");
				_logger.LogInformation ($"GetEmailRequestByRecipient ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}  for recipient with email: {recipientEmailAddress}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailRequestByRecipient exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}  for recipient with email: {recipientEmailAddress}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailRequestResponse>>> GetEmailRequestByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailRequestByUserId begins at {DateTime.UtcNow.AddHours (1)} for Id: {id}");
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
					_logger.LogInformation ($"GetEmailRequestByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for Id: {id}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.CreatedBy == id).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailRequestResponse>>.SearchSuccessful (result, count, "Email logs");
				_logger.LogInformation ($"GetEmailRequestByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for Id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailRequestByUserId exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for Id: {id}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailRequestResponse>> UpdateEmailRequestAsync (EmailRequestDto emailRequest)
		{
			try
			{
				_logger.LogInformation ($"UpdateEmailRequest begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequest.LastModifiedBy} for email log with Id: {emailRequest.Id}");

				if (emailRequest == null)
				{
					var badRequest = RequestResponse<EmailRequestResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateEmailRequest ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var updateRequest = await _context.EmailRequests
					.Where (x => x.Id == emailRequest.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (emailRequest.CancellationToken);

				if (updateRequest == null)
				{
					var badRequest = RequestResponse<EmailRequestResponse>.NotFound (null, "Email request");
					_logger.LogInformation ($"UpdateEmailRequest ends at {DateTime.UtcNow.AddHours (1)} by userId: {emailRequest.LastModifiedBy} for email request with Id: {emailRequest.Id} with remark: {badRequest.Remark}");
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
				_logger.LogInformation ($"UpdateEmailRequest at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by userId: {emailRequest.LastModifiedBy} for email request with Id: {emailRequest.Id}");
				return response;

			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateEmailRequest exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

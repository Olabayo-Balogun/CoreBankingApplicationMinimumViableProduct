using Application.Interface.Persistence;
using Application.Model;
using Application.Model.EmailLogs.Command;
using Application.Model.EmailLogs.Queries;

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
				_logger.LogInformation ($"CreateEmailLog begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailLog.CreatedBy}");

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
				_logger.LogInformation ($"CreateEmailLog at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailLog.CreatedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateEmailLog by UserPublicId: {emailLog.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailLogResponse>>> CreateMultipleEmailLogsAsync (List<EmailLogDto> emailLogs)
		{
			try
			{
				_logger.LogInformation ($"CreateMultipleEmailLogs begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailLogs.First ().CreatedBy}");

				foreach (var emailLog in emailLogs)
				{
					emailLog.DateCreated = DateTime.UtcNow.AddHours (1);
					emailLog.Sender = _appSettings.EmailSender;
					emailLog.IsSent = false;
				}

				var payload = _mapper.Map<List<EmailLog>> (emailLogs);

				await _context.EmailLogs.AddRangeAsync (payload);
				await _context.SaveChangesAsync (emailLogs.First ().CancellationToken);

				var response = _mapper.Map<List<EmailLogResponse>> (payload);
				var result = RequestResponse<List<EmailLogResponse>>.Created (response, response.Count, "Email logs");
				_logger.LogInformation ($"CreateMultipleEmailLogs at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {emailLogs.First ().CreatedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateMultipleEmailLogs by UserPublicId: {emailLogs.First ().CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> DeleteEmailLogAsync (DeleteEmailLogCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteEmailLog begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				var check = await _context.EmailLogs
					.Where (x => x.Id == request.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (request.CancellationToken);

				if (check == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");
					_logger.LogInformation ($"DeleteEmailLog ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				check.IsDeleted = true;
				check.DeletedBy = request.UserId;
				check.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Update (check);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<EmailLogResponse>.Deleted (null, 1, "Email log");
				_logger.LogInformation ($"DeleteEmailLog ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteEmailLog by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> DeleteMultipleEmailLogsAsync (DeleteMultipleEmailLogsCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteMultipleEmailLogs begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId}");
				List<EmailLog> emailLogs = [];
				foreach (long id in request.Ids)
				{
					var check = await _context.EmailLogs.Where (x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
					if (check == null)
					{
						var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email logs");
						_logger.LogInformation ($"DeleteMultipleEmailLogs ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
						return badRequest;
					}
					check.IsDeleted = true;
					check.DeletedBy = request.UserId;
					check.DateDeleted = DateTime.UtcNow.AddHours (1);
					emailLogs.Add (check);
				}

				if (emailLogs.Count < 1)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email logs");
					_logger.LogInformation ($"DeleteMultipleEmailLogs ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {badRequest.Remark}");
					return badRequest;
				}
				_context.EmailLogs.UpdateRange (emailLogs);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<EmailLogResponse>.Deleted (null, emailLogs.Count, "Email logs");
				_logger.LogInformation ($"DeleteMultipleEmailLogs ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.UserId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteMultipleEmailLogs by UserPublicId: {request.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> GetAllEmailLogCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetAllEmailLogCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.EmailLogs
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<EmailLogResponse>.CountSuccessful (null, count, "Email logs");
				_logger.LogInformation ($"GetCountOfCreatedContactUs ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllEmailLogCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByHtmlStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailLogByHtmlStatus begins at {DateTime.UtcNow.AddHours (1)} for status: {status}");
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
					_logger.LogInformation ($"GetEmailLogByHtmlStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for status {status}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.IsHtml == status).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");
				_logger.LogInformation ($"GetEmailLogByHtmlStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for status: {status}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailLogByHtmlStatus exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for status: {status}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> GetEmailLogByIdAsync (long id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetEmailLogById begins at {DateTime.UtcNow.AddHours (1)} for ID: {id}");
				var result = await _context.EmailLogs
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Id == id)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");
					_logger.LogInformation ($"GetEmailLogById ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for ID: {id}");
					return badRequest;
				}

				var response = RequestResponse<EmailLogResponse>.SearchSuccessful (result, 1, "Email log");
				_logger.LogInformation ($"GetEmailLogById ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for ID: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailLogById exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for ID: {id}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogBySentStatusAsync (bool status, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailLogBySentStatus begins at {DateTime.UtcNow.AddHours (1)} for status: {status}");
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
					_logger.LogInformation ($"GetEmailLogBySentStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for status {status}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.IsSent == status).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");
				_logger.LogInformation ($"GetEmailLogBySentStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for status: {status}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailLogBySentStatus exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for status: {status}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailLogResponse>>> GetEmailLogByUserIdAsync (string id, CancellationToken cancellationToken, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetEmailLogByUserId begins at {DateTime.UtcNow.AddHours (1)} for userId: {id}");
				var result = await _context.EmailLogs
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.CreatedBy == id)
					.OrderByDescending (y => y.DateCreated)
					.Select (x => new EmailLogResponse { BccRecipient = x.BccRecipient, CcRecipient = x.CcRecipient, DateSent = x.DateSent, Id = x.Id, IsHtml = x.IsHtml, IsSent = x.IsSent, Message = x.Message, Sender = x.Sender, Subject = x.Subject, ToRecipient = x.ToRecipient })
					.Skip ((page - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");
					_logger.LogInformation ($"GetEmailLogByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for userId: {id}");
					return badRequest;
				}

				var count = await _context.EmailLogs
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.CreatedBy == id).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful (result, count, "Email logs");
				_logger.LogInformation ($"GetEmailLogByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for userId: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetEmailLogByUserId exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for userId: {id}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> GetUnsentEmailLogCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetUnsentEmailLogCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.EmailLogs
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.IsSent == false)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<EmailLogResponse>.CountSuccessful (null, count, "Email logs");
				_logger.LogInformation ($"GetUnsentEmailLogCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUnsentEmailLogCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> UpdateEmailLogAsync (EmailLogDto emailLog)
		{
			try
			{
				_logger.LogInformation ($"UpdateEmailLog begins at {DateTime.UtcNow.AddHours (1)} by userId: {emailLog.LastModifiedBy} for email log with publicId: {emailLog.Id}");
				if (emailLog == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateEmailLog ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var updateRequest = await _context.EmailLogs
					.Where (x => x.Id == emailLog.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (emailLog.CancellationToken);

				if (updateRequest == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");
					_logger.LogInformation ($"UpdateEmailLog ends at {DateTime.UtcNow.AddHours (1)} by userId: {emailLog.LastModifiedBy} for email log with publicId: {emailLog.Id} with remark: {badRequest.Remark}");
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

				_context.EmailLogs.Update (updateRequest);
				await _context.SaveChangesAsync (emailLog.CancellationToken);

				var result = _mapper.Map<EmailLogResponse> (updateRequest);
				var response = RequestResponse<EmailLogResponse>.Updated (result, 1, "Email log");
				_logger.LogInformation ($"UpdateEmailLog at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by userId: {emailLog.LastModifiedBy} for email log with publicId: {emailLog.Id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateEmailLog exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<EmailLogResponse>> UpdateEmailLogSentStatusAsync (UpdateEmailLogSentStatusCommand request)
		{
			try
			{
				_logger.LogInformation ($"UpdateEmailLogSentStatus begins at {DateTime.UtcNow.AddHours (1)} by for email log with Id: {request.Id}");
				if (request == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var updateRequest = await _context.EmailLogs
					.Where (x => x.Id == request.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (request.CancellationToken);

				if (updateRequest == null)
				{
					var badRequest = RequestResponse<EmailLogResponse>.NotFound (null, "Email log");
					_logger.LogInformation ($"UpdateEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} for email log with Id: {request.Id} with remark: {badRequest.Remark}");
					return badRequest;
				}

				updateRequest.IsSent = request.IsSent;
				updateRequest.DateSent = request.IsSent == true ? request.DateSent : null;

				updateRequest.LastModifiedBy = request.LastModifiedBy;

				_context.EmailLogs.Update (updateRequest);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = _mapper.Map<EmailLogResponse> (updateRequest);
				var response = RequestResponse<EmailLogResponse>.Updated (result, 1, "Email log");
				_logger.LogInformation ($"UpdateEmailLogSentStatus at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for email log with Id: {request.Id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateEmailLogSentStatus with ID: {request.Id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<EmailLogResponse>>> UpdateMultipleEmailLogSentStatusAsync (List<UpdateEmailLogSentStatusCommand> requests)
		{
			try
			{
				List<EmailLog> emailLogs = [];
				foreach (var request in requests)
				{
					_logger.LogInformation ($"UpdateMultipleEmailLogSentStatus begins at {DateTime.UtcNow.AddHours (1)} by for email log with ID: {request.Id}");
					if (request == null)
					{
						var badRequest = RequestResponse<List<EmailLogResponse>>.NullPayload (null);
						_logger.LogInformation ($"UpdateMultipleEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} for email log with remark: {badRequest.Remark}");
						return badRequest;
					}

					var updateRequest = await _context.EmailLogs
						.Where (x => x.Id == request.Id && x.IsDeleted == false)
						.FirstOrDefaultAsync (request.CancellationToken);

					if (updateRequest == null)
					{
						var badRequest = RequestResponse<List<EmailLogResponse>>.NotFound (null, "Email logs");
						_logger.LogInformation ($"UpdateMultipleEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} for email log with ID: {request.Id} with remark: {badRequest.Remark}");
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

					_logger.LogInformation ($"UpdateMultipleEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_context.EmailLogs.UpdateRange (emailLogs);

				await _context.SaveChangesAsync (requests.First ().CancellationToken);
				var result = _mapper.Map<List<EmailLogResponse>> (emailLogs);
				var response = RequestResponse<List<EmailLogResponse>>.Updated (result, 1, "Email log");


				_logger.LogInformation ($"UpdateMultipleEmailLogSentStatus ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateMultipleEmailLogSentStatus exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

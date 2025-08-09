using Application.Interface.Persistence;
using Application.Model;
using Application.Model.AuditLogs.Command;
using Application.Model.Payments.Command;
using Application.Model.Payments.Queries;
using Application.Models.AuditLogs.Response;
using Application.Models.Payments.Command;
using Application.Models.Payments.Queries;
using Application.Models.Payments.Response;

using AutoMapper;

using Domain.DTO;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<PaymentRepository> _logger;
		private readonly IAuditLogRepository _auditLogRepository;

		public PaymentRepository (ApplicationDbContext context, IMapper mapper, ILogger<PaymentRepository> logger, IAuditLogRepository auditLogRepository)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
			_auditLogRepository = auditLogRepository;
		}
		public async Task<RequestResponse<PaymentResponse>> CreatePaymentAsync (PaymentDto createPaymentViewModel)
		{
			try
			{
				_logger.LogInformation ($"CreatePayment begins at {DateTime.UtcNow.AddHours (1)} by userPublicId: {createPaymentViewModel.CreatedBy} using payment channel: {createPaymentViewModel.Channel}");
				if (createPaymentViewModel == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NullPayload (null);
					_logger.LogInformation ($"CreatePayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var payload = _mapper.Map<Domain.Entities.Payment> (createPaymentViewModel);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);
				payload.PublicId = Guid.NewGuid ().ToString ();
				payload.PaymentReferenceId = Guid.NewGuid ().ToString ();
				payload.IsConfirmed = false;

				await _context.Payments.AddAsync (payload);
				await _context.SaveChangesAsync (createPaymentViewModel.CancellationToken);

				var response = _mapper.Map<PaymentResponse> (payload);
				var result = RequestResponse<PaymentResponse>.Created (response, 1, "Payment");

				_logger.LogInformation ($"CreatePayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by userPublicId: {createPaymentViewModel.CreatedBy} using payment channel: {createPaymentViewModel.Channel}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreatePayment by userPublicId: {createPaymentViewModel.CreatedBy} using payment channel: {createPaymentViewModel.Channel} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<PaymentResponse>> DeletePaymentAsync (DeletePaymentCommand deletePaymentViewModel)
		{
			try
			{
				_logger.LogInformation ($"DeletePayment begins at {DateTime.UtcNow.AddHours (1)} by DeletedBy: {deletePaymentViewModel.DeletedBy}");
				if (deletePaymentViewModel == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NullPayload (null);
					_logger.LogInformation ($"DeletePayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}
				var paymentCheck = await _context.Payments
					.Where (x => x.PublicId == deletePaymentViewModel.PublicId && x.IsDeleted == false)
					.FirstOrDefaultAsync (deletePaymentViewModel.CancellationToken);

				if (paymentCheck == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"DeletePayment ends at {DateTime.UtcNow.AddHours (1)} by DeletedBy: {deletePaymentViewModel.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = deletePaymentViewModel.CancellationToken,
					CreatedBy = paymentCheck.CreatedBy,
					Name = "Payment",
					Payload = JsonConvert.SerializeObject (paymentCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<PaymentResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"DeletePayment ends at {DateTime.UtcNow.AddHours (1)} by DeletedBy: {deletePaymentViewModel.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				paymentCheck.IsDeleted = true;
				paymentCheck.DeletedBy = deletePaymentViewModel.DeletedBy;
				paymentCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Update (paymentCheck);
				await _context.SaveChangesAsync (deletePaymentViewModel.CancellationToken);

				var result = RequestResponse<PaymentResponse>.Deleted (null, 1, "Payment");
				_logger.LogInformation ($"DeletePayment ends at {DateTime.UtcNow.AddHours (1)} by DeletedBy: {deletePaymentViewModel.DeletedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeletePayment by DeletedBy: {deletePaymentViewModel.DeletedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetAllPaymentsAsync (GetAllPaymentQuery getAllPaymentViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetAllPayments begins at {DateTime.UtcNow.AddHours (1)}");
				var result = new List<PaymentResponse> ();
				long count = 0;

				if (getAllPaymentViewModel == null)
				{
					var badRequest = RequestResponse<List<PaymentResponse>>.NullPayload (null);
					_logger.LogInformation ($"GetAllPayments ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} with count: {badRequest.TotalCount}");
					return badRequest;
				}

				result = await _context.Payments
						.AsNoTracking ()
						.Where (x => x.IsDeleted == getAllPaymentViewModel.IsDeleted && x.Channel == getAllPaymentViewModel.Channel)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((getAllPaymentViewModel.PageNumber - 1) * getAllPaymentViewModel.PageSize)
						.Take (getAllPaymentViewModel.PageSize)
						.ToListAsync (getAllPaymentViewModel.CancellationToken);

				count = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.IsDeleted == getAllPaymentViewModel.IsDeleted && x.Channel == getAllPaymentViewModel.Channel).LongCountAsync (getAllPaymentViewModel.CancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetAllPayments ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetAllPayments ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllPayments exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetPaymentByAmountPaidAsync (GetPaymentByAmountPaidQuery getPaymentByAmountPaidViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentByAmountPaid begins at {DateTime.UtcNow.AddHours (1)} for amount: {getPaymentByAmountPaidViewModel.Amount} and userId: {getPaymentByAmountPaidViewModel.UserId}");
				var result = new List<PaymentResponse> ();
				long count = 0;

				if (getPaymentByAmountPaidViewModel == null)
				{
					var badRequest = RequestResponse<List<PaymentResponse>>.NullPayload (null);
					_logger.LogInformation ($"GetPaymentByAmountPaid ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} with count: {badRequest.TotalCount}");
					return badRequest;
				}

				result = await _context.Payments
					   .AsNoTracking ()
					   .Where (x => x.Amount == getPaymentByAmountPaidViewModel.Amount && x.CreatedBy == getPaymentByAmountPaidViewModel.UserId)
					   .OrderByDescending (x => x.DateCreated)
					   .Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
					   .Skip ((getPaymentByAmountPaidViewModel.PageNumber - 1) * getPaymentByAmountPaidViewModel.PageSize)
					   .Take (getPaymentByAmountPaidViewModel.PageSize)
					   .ToListAsync (getPaymentByAmountPaidViewModel.CancellationToken);

				count = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.Amount == getPaymentByAmountPaidViewModel.Amount && x.CreatedBy == getPaymentByAmountPaidViewModel.UserId).LongCountAsync (getPaymentByAmountPaidViewModel.CancellationToken);


				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetPaymentByAmountPaid for amount: {getPaymentByAmountPaidViewModel.Amount} and userId: {getPaymentByAmountPaidViewModel.UserId} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetPaymentByAmountPaid for amount: {getPaymentByAmountPaidViewModel.Amount} and userId: {getPaymentByAmountPaidViewModel.UserId} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentByAmountPaid for amount: {getPaymentByAmountPaidViewModel.Amount} and userId: {getPaymentByAmountPaidViewModel.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetPaymentByChannelAsync (GetPaymentByChannelQuery getPaymentByChannelViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentByChannel begins at {DateTime.UtcNow.AddHours (1)} for channel: {getPaymentByChannelViewModel.Channel}");
				var result = new List<PaymentResponse> ();

				if (getPaymentByChannelViewModel == null)
				{
					var badRequest = RequestResponse<List<PaymentResponse>>.NullPayload (null);
					_logger.LogInformation ($"GetPaymentByChannel ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} with count: {badRequest.TotalCount}");
					return badRequest;
				}

				result = await _context.Payments
						.AsNoTracking ()
						.Where (x => x.Channel == getPaymentByChannelViewModel.Channel)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((getPaymentByChannelViewModel.PageNumber - 1) * getPaymentByChannelViewModel.PageSize)
						.Take (getPaymentByChannelViewModel.PageSize)
						.ToListAsync (getPaymentByChannelViewModel.CancellationToken);


				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetPaymentByChannel for channel: {getPaymentByChannelViewModel.Channel}  ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.Channel == getPaymentByChannelViewModel.Channel).LongCountAsync (getPaymentByChannelViewModel.CancellationToken);

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetPaymentByChannel for channel: {getPaymentByChannelViewModel.Channel} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentByChannel for channel: {getPaymentByChannelViewModel.Channel} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetPaymentByConfirmationStatusAsync (string userId, bool isConfirmed, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentByConfirmationStatus begins at {DateTime.UtcNow.AddHours (1)} for userId: {userId} and isConfirmed: {isConfirmed}");

				var result = await _context.Payments
						.AsNoTracking ()
						.Where (x => x.CreatedBy == userId && x.IsConfirmed == isConfirmed)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((pageNumber - 1) * pageSize)
						.Take (pageSize)
						.ToListAsync (cancellationToken);


				if (result == null)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetPaymentByConfirmationStatus for userId: {userId} and isConfirmed: {isConfirmed} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.CreatedBy == userId && x.IsConfirmed == isConfirmed).LongCountAsync (cancellationToken);

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetPaymentByConfirmationStatus for userId: {userId} and isConfirmed: {isConfirmed} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentByConfirmationStatus for userId: {userId} and isConfirmed: {isConfirmed} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetPaymentsByCustomerIdAsync (GetPaymentByCustomerIdQuery getPaymentByCustomerIdViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentsByCustomerId begins at {DateTime.UtcNow.AddHours (1)} for customer Id: {getPaymentByCustomerIdViewModel.UserId} and date paid: {getPaymentByCustomerIdViewModel.DatePaid}");
				var result = new List<PaymentResponse> ();
				long count = 0;

				if (getPaymentByCustomerIdViewModel == null)
				{
					var badRequest = RequestResponse<List<PaymentResponse>>.NullPayload (null);
					_logger.LogInformation ($"GetPaymentsByCustomerId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} with count: {badRequest.TotalCount}");
					return badRequest;
				}

				result = getPaymentByCustomerIdViewModel.DatePaid != null ? await _context.Payments
						.AsNoTracking ()
						.Where (x => x.IsDeleted == getPaymentByCustomerIdViewModel.IsDeleted && x.DateCreated == getPaymentByCustomerIdViewModel.DatePaid && x.CreatedBy == getPaymentByCustomerIdViewModel.UserId)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((getPaymentByCustomerIdViewModel.PageNumber - 1) * getPaymentByCustomerIdViewModel.PageSize)
						.Take (getPaymentByCustomerIdViewModel.PageSize)
						.ToListAsync (getPaymentByCustomerIdViewModel.CancellationToken) : await _context.Payments
						.AsNoTracking ()
						.Where (x => x.IsDeleted == getPaymentByCustomerIdViewModel.IsDeleted && x.CreatedBy == getPaymentByCustomerIdViewModel.UserId)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((getPaymentByCustomerIdViewModel.PageNumber - 1) * getPaymentByCustomerIdViewModel.PageSize)
						.Take (getPaymentByCustomerIdViewModel.PageSize)
						.ToListAsync (getPaymentByCustomerIdViewModel.CancellationToken);

				count = getPaymentByCustomerIdViewModel.DatePaid != null ? await _context.Payments
				.AsNoTracking ()
				.Where (x => x.IsDeleted == getPaymentByCustomerIdViewModel.IsDeleted && x.DateCreated == getPaymentByCustomerIdViewModel.DatePaid && x.CreatedBy == getPaymentByCustomerIdViewModel.UserId).LongCountAsync (getPaymentByCustomerIdViewModel.CancellationToken) : await _context.Payments
				.AsNoTracking ()
				.Where (x => x.IsDeleted == getPaymentByCustomerIdViewModel.IsDeleted && x.CreatedBy == getPaymentByCustomerIdViewModel.UserId).LongCountAsync (getPaymentByCustomerIdViewModel.CancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetPaymentsByCustomerId for customer Id: {getPaymentByCustomerIdViewModel.UserId} and date paid: {getPaymentByCustomerIdViewModel.DatePaid} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetPaymentsByCustomerId for customer Id: {getPaymentByCustomerIdViewModel.UserId} and date paid: {getPaymentByCustomerIdViewModel.DatePaid} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentsByCustomerId for customer Id: {getPaymentByCustomerIdViewModel.UserId} and date paid: {getPaymentByCustomerIdViewModel.DatePaid} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<PaymentResponse>>> GetPaymentsByDatePaidAsync (GetPaymentByDatePaidQuery getPaymentByDatePaidViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentsByDatePaid begins at {DateTime.UtcNow.AddHours (1)} for date paid: {getPaymentByDatePaidViewModel.DatePaid} and payment type: {getPaymentByDatePaidViewModel.ProductName}");
				var result = new List<PaymentResponse> ();

				if (getPaymentByDatePaidViewModel == null)
				{
					var badRequest = RequestResponse<List<PaymentResponse>>.NullPayload (null);
					_logger.LogInformation ($"GetPaymentsByDatePaid ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} with count: {badRequest.TotalCount}");
					return badRequest;
				}

				result = await _context.Payments
						.AsNoTracking ()
						.Where (x => x.DateCreated == getPaymentByDatePaidViewModel.DatePaid)
						.OrderByDescending (x => x.DateCreated)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId })
						.Skip ((getPaymentByDatePaidViewModel.PageNumber - 1) * getPaymentByDatePaidViewModel.PageSize)
						.Take (getPaymentByDatePaidViewModel.PageSize)
						.ToListAsync (getPaymentByDatePaidViewModel.CancellationToken);


				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<PaymentResponse>>.NotFound (null, "Payments");
					_logger.LogInformation ($"GetPaymentsByDatePaid for date paid: {getPaymentByDatePaidViewModel.DatePaid} and payment type: {getPaymentByDatePaidViewModel.ProductName} ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {badResponse.TotalCount}");
					return badResponse;
				}

				var count = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.DateCreated == getPaymentByDatePaidViewModel.DatePaid).LongCountAsync (getPaymentByDatePaidViewModel.CancellationToken);

				var response = RequestResponse<List<PaymentResponse>>.SearchSuccessful (result, count, "Payments");
				_logger.LogInformation ($"GetPaymentsByDatePaid for date paid: {getPaymentByDatePaidViewModel.DatePaid} and payment type: {getPaymentByDatePaidViewModel.ProductName} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentsByDatePaid for date paid: {getPaymentByDatePaidViewModel.DatePaid} and payment type: {getPaymentByDatePaidViewModel.ProductName} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<PaymentResponse>> GetPaymentByIdAsync (GetPaymentByIdQuery getPaymentByIdRequestViewModel)
		{
			try
			{
				_logger.LogInformation ($"GetPaymentDetailById begins at {DateTime.UtcNow.AddHours (1)} for PublicId: {getPaymentByIdRequestViewModel.PaymentPublicId}, and userId: {getPaymentByIdRequestViewModel.UserId}");
				PaymentResponse? result = new ();
				result = getPaymentByIdRequestViewModel.UserId != null && getPaymentByIdRequestViewModel.IsDeleted
					? await _context.Payments
						.AsNoTracking ()
						.Where (x => x.IsDeleted == getPaymentByIdRequestViewModel.IsDeleted && x.PublicId == getPaymentByIdRequestViewModel.PaymentPublicId && x.DeletedBy == getPaymentByIdRequestViewModel.UserId)
						.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.DeletedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, Date = x.DateDeleted })
						.FirstOrDefaultAsync (getPaymentByIdRequestViewModel.CancellationToken)
					: getPaymentByIdRequestViewModel.UserId != null && !getPaymentByIdRequestViewModel.IsDeleted
						? await _context.Payments
											.AsNoTracking ()
											.Where (x => x.IsDeleted == getPaymentByIdRequestViewModel.IsDeleted && x.PublicId == getPaymentByIdRequestViewModel.PaymentPublicId && x.CreatedBy == getPaymentByIdRequestViewModel.UserId)
											.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, Date = x.DateCreated })
											.FirstOrDefaultAsync (getPaymentByIdRequestViewModel.CancellationToken)
						: getPaymentByIdRequestViewModel.UserId != null && !getPaymentByIdRequestViewModel.IsDeleted && getPaymentByIdRequestViewModel.PaymentReferenceId != null
											? await _context.Payments
																.AsNoTracking ()
																.Where (x => x.IsDeleted == getPaymentByIdRequestViewModel.IsDeleted && x.PaymentReferenceId == getPaymentByIdRequestViewModel.PaymentReferenceId && x.CreatedBy == getPaymentByIdRequestViewModel.UserId)
																.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, Date = x.DateCreated })
																.FirstOrDefaultAsync (getPaymentByIdRequestViewModel.CancellationToken)
											: getPaymentByIdRequestViewModel.IsDeleted && getPaymentByIdRequestViewModel.PaymentReferenceId != null
																? await _context.Payments
																					.AsNoTracking ()
																					.Where (x => x.IsDeleted == getPaymentByIdRequestViewModel.IsDeleted && x.PaymentReferenceId == getPaymentByIdRequestViewModel.PaymentReferenceId)
																					.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.DeletedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, Date = x.DateDeleted })
																					.FirstOrDefaultAsync (getPaymentByIdRequestViewModel.CancellationToken)
																: await _context.Payments
																					.AsNoTracking ()
																					.Where (x => x.IsDeleted == getPaymentByIdRequestViewModel.IsDeleted && x.PublicId == getPaymentByIdRequestViewModel.PaymentPublicId)
																					.Select (x => new PaymentResponse { Amount = x.Amount, UserId = x.CreatedBy, Channel = x.Channel, Currency = x.Currency, IsConfirmed = x.IsConfirmed, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, Date = x.DateCreated })
																					.FirstOrDefaultAsync (getPaymentByIdRequestViewModel.CancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"GetPaymentDetailById ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for PublicId: {getPaymentByIdRequestViewModel.PaymentPublicId} and userId: {getPaymentByIdRequestViewModel.UserId}");
					return badRequest;
				}
				var response = RequestResponse<PaymentResponse>.SearchSuccessful (result, 1, "Payment");
				_logger.LogInformation ($"GetPaymentDetailById ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for PublicId: {getPaymentByIdRequestViewModel.PaymentPublicId} and userId: {getPaymentByIdRequestViewModel.UserId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetPaymentDetailById for PublicId: {getPaymentByIdRequestViewModel.PaymentPublicId} and userId: {getPaymentByIdRequestViewModel.UserId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for PublicId: {getPaymentByIdRequestViewModel.PaymentPublicId}");
				throw;
			}
		}

		public async Task<RequestResponse<PaymentResponse>> VerifyPaymentAsync (VerifyPaymentCommand verifyPaymentViewModel)
		{
			try
			{
				_logger.LogInformation ($"VerifyPayment begins at {DateTime.UtcNow.AddHours (1)} for payment reference: {verifyPaymentViewModel.PaymentReferenceNumber} and userID is {verifyPaymentViewModel.PublicUserId}");
				var result = await _context.Payments
					.AsNoTracking ()
					.Where (x => x.PaymentReferenceId == verifyPaymentViewModel.PaymentReferenceNumber && x.CreatedBy == verifyPaymentViewModel.PublicUserId)
					.Select (x => new PaymentResponse { Amount = x.Amount, Channel = x.Channel, Date = x.DateCreated, PaymentReferenceId = x.PaymentReferenceId, PaymentService = x.PaymentService, PublicId = x.PublicId, UserId = x.CreatedBy, IsConfirmed = x.IsConfirmed, Currency = x.Currency })
					.FirstOrDefaultAsync (verifyPaymentViewModel.CancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"VerifyPayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for payment reference: {verifyPaymentViewModel.PaymentReferenceNumber} and userID is {verifyPaymentViewModel.PublicUserId}");

					return badRequest;
				}

				var response = RequestResponse<PaymentResponse>.SearchSuccessful (result, 1, "Payment");
				_logger.LogInformation ($"VerifyPayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for payment reference: {verifyPaymentViewModel.PaymentReferenceNumber} and userID is {verifyPaymentViewModel.PublicUserId}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"VerifyPayment exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for payment reference: {verifyPaymentViewModel.PaymentReferenceNumber} and userID is {verifyPaymentViewModel.PublicUserId}");
				throw;
			}
		}

		public async Task<RequestResponse<PaymentResponse>> ConfirmPaymentAsync (CancellationToken cancellationToken, decimal amount, string paymentReferenceNumber)
		{
			try
			{
				_logger.LogInformation ($"ConfirmPayment begins at {DateTime.UtcNow.AddHours (1)} for payment reference: {paymentReferenceNumber} and amount: {amount}");
				var payload = await _context.Payments
					.Where (x => x.PaymentReferenceId == paymentReferenceNumber && x.IsDeleted == false)
					.FirstOrDefaultAsync (cancellationToken);

				if (payload == null)
				{
					var badRequest = RequestResponse<PaymentResponse>.NotFound (null, "Payment");
					_logger.LogInformation ($"ConfirmPayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for payment reference: {paymentReferenceNumber} and amount: {amount}");

					return badRequest;
				}
				var result = new PaymentResponse ();

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = cancellationToken,
					CreatedBy = payload.CreatedBy,
					Name = "Payment",
					Payload = JsonConvert.SerializeObject (payload)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<PaymentResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"ConfirmPayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for payment reference: {paymentReferenceNumber} and amount: {amount}");

					return badRequest;
				}

				payload.IsConfirmed = true;
				payload.Amount = amount;
				payload.LastModifiedBy = "SYSTEM";

				_context.Payments.Update (payload);
				await _context.SaveChangesAsync (cancellationToken);


				result = _mapper.Map<PaymentResponse> (payload);
				var response = RequestResponse<PaymentResponse>.Updated (result, 1, "Payment");

				_logger.LogInformation ($"ConfirmPayment ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for payment reference: {paymentReferenceNumber} and amount: {amount}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"ConfirmPayment exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message} for payment reference: {paymentReferenceNumber} and amount: {amount}");
				throw;
			}
		}
	}
}

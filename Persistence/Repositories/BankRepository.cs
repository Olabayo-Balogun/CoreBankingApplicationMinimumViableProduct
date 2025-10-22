using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Banks.Command;
using Application.Models.Banks.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
	public class BankRepository : IBankRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IAuditLogRepository _auditLogRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<BankRepository> _logger;
		public BankRepository (ApplicationDbContext context, IMapper mapper, ILogger<BankRepository> logger, IAuditLogRepository auditLogRepository)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
			_auditLogRepository = auditLogRepository;
		}

		public async Task<RequestResponse<BankResponse>> CreateBankAsync (BankDto bank)
		{
			try
			{
				_logger.LogInformation ($"CreateBank begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {bank.CreatedBy}");

				if (bank == null)
				{
					var badRequest = RequestResponse<BankResponse>.NullPayload (null);
					_logger.LogInformation ($"CreateBank ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var bankCheck = await _context.Banks.AsNoTracking ().Where (x => x.Name == bank.Name.Trim () && x.IsDeleted == false).LongCountAsync ();

				if (bankCheck > 0)
				{
					var badRequest = RequestResponse<BankResponse>.AlreadyExists (null, bankCheck, "Bank");
					_logger.LogInformation ($"CreateBank ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by UserPublicId: {bank.CreatedBy}");
					return badRequest;
				}

				var payload = _mapper.Map<Bank> (bank);

				payload.IsDeleted = false;
				payload.DateDeleted = null;
				payload.LastModifiedBy = null;
				payload.LastModifiedDate = null;
				payload.DeletedBy = null;
				payload.DateCreated = DateTime.UtcNow.AddHours (1);

				await _context.Banks.AddAsync (payload, bank.CancellationToken);
				await _context.SaveChangesAsync (bank.CancellationToken);

				var response = _mapper.Map<BankResponse> (payload);
				var result = RequestResponse<BankResponse>.Created (response, 1, "Bank");

				_logger.LogInformation ($"CreateBank ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by UserPublicId: {bank.CreatedBy}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateBank by UserPublicId: {bank.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BankResponse>> DeleteBankAsync (DeleteBankCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteBank begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} for Bank with ID: {request.Id}");

				var bankCheck = await _context.Banks.Where (x => x.Id == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (bankCheck == null)
				{
					var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");

					_logger.LogInformation ($"DeleteBank ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = request.CancellationToken,
					CreatedBy = bankCheck.CreatedBy,
					Name = "Bank",
					Payload = JsonConvert.SerializeObject (bankCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<BankResponse>.AuditLogFailed (null);

					_logger.LogInformation ($"DeleteBank ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

					return badRequest;
				}

				bankCheck.IsDeleted = true;
				bankCheck.DeletedBy = request.DeletedBy;
				bankCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Banks.Update (bankCheck);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<BankResponse>.Deleted (null, 1, "Bank");

				_logger.LogInformation ($"DeleteBank ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {result.Remark}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteBank by UserPublicId: {request.DeletedBy} for Bank with ID: {request.Id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BankResponse>> GetBankByPublicIdAsync (long id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBankByPublicId begins at {DateTime.UtcNow.AddHours (1)} for bank with publicId: {id}");

				var result = await _context.Banks
					.AsNoTracking ()
					.Where (bank => bank.Id == id)
					.Select (x => new BankResponse { Name = x.Name, CbnCode = x.CbnCode, Id = x.Id, NibssCode = x.NibssCode })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");

					_logger.LogInformation ($"GetBankByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for id: {id}");

					return badRequest;
				}

				var response = RequestResponse<BankResponse>.SearchSuccessful (result, 1, "Bank");
				_logger.LogInformation ($"GetBankByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBankByPublicId for bank with publicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<BankResponse>>> GetBanksByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetBankByUserId begins at {DateTime.UtcNow.AddHours (1)} for bank with userPublicId: {id}");

				var result = await _context.Banks
					.AsNoTracking ()
					.Where (bank => bank.CreatedBy == id)
					.OrderByDescending (x => x.DateCreated)
					.Select (x => new BankResponse { Name = x.Name, CbnCode = x.CbnCode, Id = x.Id, NibssCode = x.NibssCode })
					.Skip ((pageNumber - 1) * pageSize)
					.Take (pageSize)
					.ToListAsync (cancellationToken);

				if (result.Count < 1)
				{
					var badResponse = RequestResponse<List<BankResponse>>.NotFound (null, "Bank");
					_logger.LogInformation ($"GetBankByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {result.Count}");

					return badResponse;
				}

				var count = await _context.Banks
					.AsNoTracking ()
					.Where (bank => bank.CreatedBy == id && bank.IsDeleted == false)
					.LongCountAsync ();

				var response = RequestResponse<List<BankResponse>>.SearchSuccessful (result, count, "Banks");

				_logger.LogInformation ($"GetBankByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBankByUserId for bank with userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BankResponse>> GetBankCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBankCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Banks
					.AsNoTracking ()
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<BankResponse>.CountSuccessful (null, count, "Bank");
				_logger.LogInformation ($"GetBankCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBankCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BankResponse>> GetBankCountByUserIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBankCountByUserId for userPublicId: {id} begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Banks
					.AsNoTracking ()
					.Where (x => x.CreatedBy == id)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<BankResponse>.CountSuccessful (null, count, "Bank");
				_logger.LogInformation ($"GetBankCountByUserId for userPublicId: {id} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBankCountByUserId for userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BankResponse>> UpdateBankAsync (BankDto bank)
		{
			try
			{
				_logger.LogInformation ($"UpdateBank begins at {DateTime.UtcNow.AddHours (1)} for bank with publicId: {bank.Id} by UserPublicId: {bank.LastModifiedBy}");

				if (bank == null)
				{
					var badRequest = RequestResponse<BankResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateBank ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var updateBankRequest = await _context.Banks
					.Where (x => x.Id == bank.Id && x.IsDeleted == false)
					.FirstOrDefaultAsync (bank.CancellationToken);

				if (updateBankRequest == null)
				{
					var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");
					_logger.LogInformation ($"UpdateBank ends at {DateTime.UtcNow.AddHours (1)}  with remark: {badRequest.Remark} by UserPublicId: {bank.LastModifiedBy} for bank with Id: {bank.Id}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = bank.CancellationToken,
					CreatedBy = updateBankRequest.CreatedBy,
					Name = "Bank",
					Payload = JsonConvert.SerializeObject (updateBankRequest)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<BankResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateBank ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by UserPublicId: {bank.LastModifiedBy} for bank with Id: {bank.Id}");
					return badRequest;
				}

				updateBankRequest.NibssCode = bank.NibssCode;
				updateBankRequest.CbnCode = bank.CbnCode;
				updateBankRequest.Name = bank.Name;
				updateBankRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateBankRequest.LastModifiedBy = bank.LastModifiedBy;

				_context.Banks.Update (updateBankRequest);
				await _context.SaveChangesAsync (bank.CancellationToken);

				var result = _mapper.Map<BankResponse> (updateBankRequest);
				var response = RequestResponse<BankResponse>.Updated (result, 1, "Bank");
				_logger.LogInformation ($"UpdateBank at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by UserPublicId: {bank.LastModifiedBy} for bank with Id: {bank.Id}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateBank for bank with publicId: {bank.Id} error occurred at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {bank.LastModifiedBy} with message: {ex.Message}");
				throw;
			}
		}
	}
}

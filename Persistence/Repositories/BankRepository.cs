using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Banks.Command;
using Application.Models.Banks.Response;
using Application.Utility;

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
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.CreatedBy), bank.CreatedBy);
                _logger.LogInformation (openingLog);

                if (bank == null)
                {
                    var badRequest = RequestResponse<BankResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateBankAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var bankCheck = await _context.Banks.AsNoTracking ().Where (x => x.Name == bank.Name.Trim () && x.IsDeleted == false).LongCountAsync ();

                if (bankCheck > 0)
                {
                    var badRequest = RequestResponse<BankResponse>.AlreadyExists (null, bankCheck, "Bank");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.CreatedBy), bank.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.CreatedBy), bank.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.CreatedBy), bank.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<BankResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BankResponse>> DeleteBankAsync (DeleteBankCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteBankAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var bankCheck = await _context.Banks.Where (x => x.Id == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (bankCheck == null)
                {
                    var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBankAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBankAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                bankCheck.IsDeleted = true;
                bankCheck.DeletedBy = request.DeletedBy;
                bankCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                _context.Banks.Update (bankCheck);
                await _context.SaveChangesAsync ();

                var result = RequestResponse<BankResponse>.Deleted (null, 1, "Bank");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBankAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteBankAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BankResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BankResponse>> GetBankByPublicIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBankByPublicIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.Banks
                    .AsNoTracking ()
                    .Where (bank => bank.Id == id)
                    .Select (x => new BankResponse { Name = x.Name, CbnCode = x.CbnCode, Id = x.Id, NibssCode = x.NibssCode })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBankByPublicIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var response = RequestResponse<BankResponse>.SearchSuccessful (result, 1, "Bank");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetBankByPublicIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBankByPublicIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BankResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<BankResponse>>> GetBanksByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBanksByUserIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

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
                    var badRequest = RequestResponse<List<BankResponse>>.NotFound (null, "Bank");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBanksByUserIdAsync), nameof (id), id.ToString (), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Banks
                    .AsNoTracking ()
                    .Where (bank => bank.CreatedBy == id && bank.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<BankResponse>>.SearchSuccessful (result, count, "Banks");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetBanksByUserIdAsync), nameof (id), id.ToString (), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBanksByUserIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<BankResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<BankResponse>> GetBankCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBankCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Banks
                    .AsNoTracking ()
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<BankResponse>.CountSuccessful (null, count, "Bank");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBankCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBankCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BankResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BankResponse>> GetBankCountByUserIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBankCountByUserIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                long count = await _context.Banks
                    .AsNoTracking ()
                    .Where (x => x.CreatedBy == id)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<BankResponse>.CountSuccessful (null, count, "Bank");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBankCountByUserIdAsync), nameof (id), id.ToString (), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBankCountByUserIdAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BankResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BankResponse>> UpdateBankAsync (BankDto bank)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.LastModifiedBy), bank.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (bank == null)
                {
                    var badRequest = RequestResponse<BankResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBankAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateBankRequest = await _context.Banks
                    .Where (x => x.Id == bank.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (bank.CancellationToken);

                if (updateBankRequest == null)
                {
                    var badRequest = RequestResponse<BankResponse>.NotFound (null, "Bank");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.LastModifiedBy), bank.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
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

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.LastModifiedBy), bank.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
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

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.LastModifiedBy), bank.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateBankAsync), nameof (bank.Name), bank.Name, nameof (bank.LastModifiedBy), bank.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<BankResponse>.Error (null);
            }
        }
    }
}

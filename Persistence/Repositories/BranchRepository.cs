using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Branches.Command;
using Application.Models.Branches.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BranchRepository> _logger;
        public BranchRepository (ApplicationDbContext context, IMapper mapper, ILogger<BranchRepository> logger, IAuditLogRepository auditLogRepository)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<RequestResponse<BranchResponse>> CreateBranchAsync (BranchDto branch)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateBranchAsync), nameof (branch.Name), branch.Name, nameof (branch.CreatedBy), branch.CreatedBy);
                _logger.LogInformation (openingLog);

                if (branch == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateBranchAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var branchCheck = await _context.Branches.AsNoTracking ().Where (x => x.Name == branch.Name.Trim () && x.IsDeleted == false).LongCountAsync ();

                if (branchCheck > 0)
                {
                    var badRequest = RequestResponse<BranchResponse>.AlreadyExists (null, branchCheck, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateBranchAsync), nameof (branch.Name), branch.Name, nameof (branch.CreatedBy), branch.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var payload = _mapper.Map<Branch> (branch);

                payload.IsDeleted = false;
                payload.DateDeleted = null;
                payload.LastModifiedBy = null;
                payload.LastModifiedDate = null;
                payload.DeletedBy = null;
                payload.DateCreated = DateTime.UtcNow.AddHours (1);

                await _context.Branches.AddAsync (payload, branch.CancellationToken);
                await _context.SaveChangesAsync (branch.CancellationToken);

                var response = _mapper.Map<BranchResponse> (payload);
                var result = RequestResponse<BranchResponse>.Created (response, 1, "Branch");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateBranchAsync), nameof (branch.Name), branch.Name, nameof (branch.CreatedBy), branch.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateBranchAsync), nameof (branch.Name), branch.Name, nameof (branch.CreatedBy), branch.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> DeleteBranchAsync (DeleteBranchCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteBranchAsync), nameof (request.PublicId), request.PublicId, nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var branchCheck = await _context.Branches.Where (x => x.PublicId == request.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (branchCheck == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBranchAsync), nameof (request.PublicId), request.PublicId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = branchCheck.CreatedBy,
                    Name = "Branch",
                    Payload = JsonConvert.SerializeObject (branchCheck)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<BranchResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBranchAsync), nameof (request.PublicId), request.PublicId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                branchCheck.IsDeleted = true;
                branchCheck.DeletedBy = request.DeletedBy;
                branchCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                await _context.SaveChangesAsync ();

                var result = RequestResponse<BranchResponse>.Deleted (null, 1, "Branch");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteBranchAsync), nameof (request.PublicId), request.PublicId, nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteBranchAsync), nameof (request.PublicId), request.PublicId, nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> CloseBranchAsync (CloseBranchCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CloseBranchAsync), nameof (request.Id), request.Id, nameof (request.LastModifiedBy), request.LastModifiedBy);
                _logger.LogInformation (openingLog);

                var branchCheck = await _context.Branches.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (branchCheck == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CloseBranchAsync), nameof (request.Id), request.Id, nameof (request.LastModifiedBy), request.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = branchCheck.CreatedBy,
                    Name = "Branch",
                    Payload = JsonConvert.SerializeObject (branchCheck)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<BranchResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CloseBranchAsync), nameof (request.Id), request.Id, nameof (request.LastModifiedBy), request.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                branchCheck.IsClosed = true;
                branchCheck.LastModifiedBy = request.LastModifiedBy;
                branchCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                await _context.SaveChangesAsync ();

                var result = RequestResponse<BranchResponse>.Deleted (null, 1, "Branch");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CloseBranchAsync), nameof (request.Id), request.Id, nameof (request.LastModifiedBy), request.LastModifiedBy, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CloseBranchAsync), nameof (request.Id), request.Id, nameof (request.LastModifiedBy), request.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> GetBranchByPublicIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBranchByPublicIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Branches
                    .AsNoTracking ()
                    .Where (branch => branch.PublicId == id)
                    .Select (x => new BranchResponse { Lga = x.Lga, Country = x.Country, Code = x.Code, Address = x.Address, State = x.State, Name = x.Name, IsClosed = x.IsClosed, PublicId = x.PublicId })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchByPublicIdAsync), nameof (id), id, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<BranchResponse>.SearchSuccessful (result, 1, "Branch");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchByPublicIdAsync), nameof (id), id, response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBranchByPublicIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<BranchResponse>>> GetBranchesByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBranchesByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Branches
                    .AsNoTracking ()
                    .Where (branch => branch.CreatedBy == id)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new BranchResponse { Lga = x.Lga, Country = x.Country, Code = x.Code, Address = x.Address, State = x.State, Name = x.Name, IsClosed = x.IsClosed, PublicId = x.PublicId })
                    .Skip ((pageNumber - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<BranchResponse>>.NotFound (null, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchesByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Branches
                    .AsNoTracking ()
                    .Where (branch => branch.CreatedBy == id && branch.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<BranchResponse>>.SearchSuccessful (result, count, "Branches");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchesByUserIdAsync), nameof (id), id, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBranchesByUserIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<BranchResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> GetBranchCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBranchCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Branches
                    .AsNoTracking ()
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<BranchResponse>.CountSuccessful (null, count, "Branch");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBranchCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> GetBranchCountByUserIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetBranchCountByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                long count = await _context.Branches
                    .AsNoTracking ()
                    .Where (x => x.CreatedBy == id)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<BranchResponse>.CountSuccessful (null, count, "Branch");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetBranchCountByUserIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetBranchCountByUserIdAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<BranchResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<BranchResponse>> UpdateBranchAsync (BranchDto branch)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateBranchAsync), nameof (branch.PublicId), branch.PublicId, nameof (branch.LastModifiedBy), branch.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (branch == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBranchAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateBranchRequest = await _context.Branches
                    .Where (x => x.PublicId == branch.PublicId && x.IsDeleted == false)
                    .FirstOrDefaultAsync (branch.CancellationToken);

                if (updateBranchRequest == null)
                {
                    var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBranchAsync), nameof (branch.PublicId), branch.PublicId, nameof (branch.LastModifiedBy), branch.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = branch.CancellationToken,
                    CreatedBy = updateBranchRequest.CreatedBy,
                    Name = "Branch",
                    Payload = JsonConvert.SerializeObject (updateBranchRequest)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<BranchResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBranchAsync), nameof (branch.PublicId), branch.PublicId, nameof (branch.LastModifiedBy), branch.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                updateBranchRequest.State = branch.State;
                updateBranchRequest.Address = branch.Address;
                updateBranchRequest.Code = branch.Code;
                updateBranchRequest.Country = branch.Country;
                updateBranchRequest.Lga = branch.Lga;
                updateBranchRequest.Name = branch.Name;
                updateBranchRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateBranchRequest.LastModifiedBy = branch.LastModifiedBy;

                await _context.SaveChangesAsync (branch.CancellationToken);

                var result = _mapper.Map<BranchResponse> (updateBranchRequest);
                var response = RequestResponse<BranchResponse>.Updated (result, 1, "Branch");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateBranchAsync), nameof (branch.PublicId), branch.PublicId, nameof (branch.LastModifiedBy), branch.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateBranchAsync), nameof (branch.PublicId), branch.PublicId, nameof (branch.LastModifiedBy), branch.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<BranchResponse>.Error (null);
            }
        }
    }
}

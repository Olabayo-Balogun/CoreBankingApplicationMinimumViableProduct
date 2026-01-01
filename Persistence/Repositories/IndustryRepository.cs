using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Industry.Command;
using Application.Models.Industry.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
    public class IndustryRepository : IIndustryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<IndustryRepository> _logger;
        public IndustryRepository (ApplicationDbContext context, IMapper mapper, ILogger<IndustryRepository> logger, IAuditLogRepository auditLogRepository)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<RequestResponse<IndustryResponse>> CreateIndustryAsync (IndustryDto industry)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.CreatedBy), industry.CreatedBy);
                _logger.LogInformation (openingLog);

                if (industry == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var industryCheck = await _context.Industries.AsNoTracking ().Where (x => x.Name == industry.Name.Trim () && x.IsDeleted == false).LongCountAsync ();

                if (industryCheck > 0)
                {
                    var badRequest = RequestResponse<IndustryResponse>.AlreadyExists (null, industryCheck, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.CreatedBy), industry.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var payload = _mapper.Map<Industry> (industry);

                payload.IsDeleted = false;
                payload.DateDeleted = null;
                payload.LastModifiedBy = null;
                payload.LastModifiedDate = null;
                payload.DeletedBy = null;
                payload.DateCreated = DateTime.UtcNow.AddHours (1);

                await _context.Industries.AddAsync (payload, industry.CancellationToken);
                await _context.SaveChangesAsync (industry.CancellationToken);

                var response = _mapper.Map<IndustryResponse> (payload);
                var result = RequestResponse<IndustryResponse>.Created (response, 1, "Industry");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.CreatedBy), industry.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.CreatedBy), industry.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> DeleteIndustryAsync (DeleteIndustryCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteIndustryAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var industryCheck = await _context.Industries.Where (x => x.Id == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (industryCheck == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NotFound (null, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = industryCheck.CreatedBy,
                    Name = "Industry",
                    Payload = JsonConvert.SerializeObject (industryCheck)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<IndustryResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                industryCheck.IsDeleted = true;
                industryCheck.DeletedBy = request.DeletedBy;
                industryCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                await _context.SaveChangesAsync ();

                var result = RequestResponse<IndustryResponse>.Deleted (null, 1, "Industry");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteIndustryAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> GetIndustryByIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryByIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.Id == id)
                    .Select (x => new IndustryResponse { Name = x.Name, Description = x.Description, Id = x.Id })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NotFound (null, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryByIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var response = RequestResponse<IndustryResponse>.SearchSuccessful (result, 1, "Industry");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryByIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryByIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<IndustryResponse>>> GetAllIndustriesAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllIndustriesAsync));
                _logger.LogInformation (openingLog);

                var result = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new IndustryResponse { Name = x.Name, Description = x.Description, Id = x.Id })
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<IndustryResponse>>.NotFound (null, "Industries");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllIndustriesAsync), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<IndustryResponse>>.SearchSuccessful (result, count, "Industries");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllIndustriesAsync), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllIndustriesAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<IndustryResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<IndustryResponse>>> GetIndustriesByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustriesByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.CreatedBy == id && industry.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new IndustryResponse { Name = x.Name, Description = x.Description, Id = x.Id })
                    .Skip ((pageNumber - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<IndustryResponse>>.NotFound (null, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustriesByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.CreatedBy == id && industry.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<IndustryResponse>>.SearchSuccessful (result, count, "Industries");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustriesByUserIdAsync), nameof (id), id, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustriesByUserIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<IndustryResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> GetIndustryCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Industries
                    .AsNoTracking ()
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<IndustryResponse>.CountSuccessful (null, count, "Industry");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> GetIndustryCountByUserIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryCountByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                long count = await _context.Industries
                    .AsNoTracking ()
                    .Where (x => x.CreatedBy == id)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<IndustryResponse>.CountSuccessful (null, count, "Industry");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryCountByUserIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryCountByUserIdAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> UpdateIndustryAsync (IndustryDto industry)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.LastModifiedBy), industry.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (industry == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateIndustryRequest = await _context.Industries
                    .Where (x => x.Id == industry.Id && x.IsDeleted == false)
                    .FirstOrDefaultAsync (industry.CancellationToken);

                if (updateIndustryRequest == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NotFound (null, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.LastModifiedBy), industry.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = industry.CancellationToken,
                    CreatedBy = updateIndustryRequest.CreatedBy,
                    Name = "Industry",
                    Payload = JsonConvert.SerializeObject (updateIndustryRequest)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<IndustryResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.LastModifiedBy), industry.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                updateIndustryRequest.Description = industry.Description;
                updateIndustryRequest.Name = industry.Name;
                updateIndustryRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateIndustryRequest.LastModifiedBy = industry.LastModifiedBy;

                await _context.SaveChangesAsync (industry.CancellationToken);

                var result = _mapper.Map<IndustryResponse> (updateIndustryRequest);
                var response = RequestResponse<IndustryResponse>.Updated (result, 1, "Industry");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.LastModifiedBy), industry.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateIndustryAsync), nameof (industry.Name), industry.Name, nameof (industry.LastModifiedBy), industry.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<IndustryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryResponse>> GetIndustryByNameAsync (string name, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryByNameAsync), nameof (name), name);
                _logger.LogInformation (openingLog);

                var result = await _context.Industries
                    .AsNoTracking ()
                    .Where (industry => industry.Name == name)
                    .Select (x => new IndustryResponse { Name = x.Name, Description = x.Description, Id = x.Id })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<IndustryResponse>.NotFound (null, "Industry");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryByNameAsync), nameof (name), name, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var response = RequestResponse<IndustryResponse>.SearchSuccessful (result, 1, "Industry");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryByNameAsync), nameof (name), name, response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryByNameAsync), nameof (name), name, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryResponse>.Error (null);
            }
        }
    }
}

using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.IndustryField.Command;
using Application.Models.IndustryField.Response;
using Application.Utility;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
    public class IndustryFieldRepository : IIndustryFieldRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<IndustryFieldRepository> _logger;
        public IndustryFieldRepository (ApplicationDbContext context, IMapper mapper, ILogger<IndustryFieldRepository> logger, IAuditLogRepository auditLogRepository)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<RequestResponse<IndustryFieldResponse>> CreateIndustryFieldAsync (IndustryFieldDto industryField)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy);
                _logger.LogInformation (openingLog);

                if (industryField == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (industryField.Order < 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field cannot have an order less than one");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryField.IndustryId < 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field cannot have an industryID less than one");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var industries = await _context.Industries.AsNoTracking ().Where (x => x.IsDeleted == false).Select (x => x.Id).ToListAsync (industryField.CancellationToken);
                if (industries == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Unable to retrieve industries");
                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (!industries.Contains (industryField.IndustryId))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry does not exist");
                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var industryFields = await _context.IndustryFields.AsNoTracking ().Where (x => x.IsDeleted == false && x.IndustryId == industryField.IndustryId).ToListAsync (industryField.CancellationToken);

                if (industryFields == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryFields.Any (x => x.Name.ToLower () == industryField.Name.Trim ().ToLower ()))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.AlreadyExists (null, industryFields.Count, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryFields.Any (x => x.Order == industryField.Order))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field already exists with the same order");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (!industryFields.Any (x => x.Order == industryField.Order - 1) && industryField.Order > 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, $"An industry field does not exist for the order {industryField.Order - 1}, please resolve this");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                industryField.Name = industryField.Name.Trim ();
                var payload = _mapper.Map<IndustryField> (industryField);

                payload.IsDeleted = false;
                payload.DateDeleted = null;
                payload.LastModifiedBy = null;
                payload.LastModifiedDate = null;
                payload.DeletedBy = null;
                payload.DateCreated = DateTime.UtcNow.AddHours (1);

                await _context.IndustryFields.AddAsync (payload, industryField.CancellationToken);
                await _context.SaveChangesAsync (industryField.CancellationToken);

                var response = _mapper.Map<IndustryFieldResponse> (payload);
                var result = RequestResponse<IndustryFieldResponse>.Created (response, 1, "Industry field");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> DeleteIndustryFieldAsync (DeleteIndustryFieldCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteIndustryFieldAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var industryCheck = await _context.IndustryFields.Where (x => x.Id == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
                if (industryCheck == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryFieldAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = industryCheck.CreatedBy,
                    Name = "IndustryField",
                    Payload = JsonConvert.SerializeObject (industryCheck)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryFieldAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                industryCheck.IsDeleted = true;
                industryCheck.DeletedBy = request.DeletedBy;
                industryCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                await _context.SaveChangesAsync ();

                var result = RequestResponse<IndustryFieldResponse>.Deleted (null, 1, "Industry field");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteIndustryFieldAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteIndustryFieldAsync), nameof (request.Id), request.Id.ToString (), nameof (request.DeletedBy), request.DeletedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldByIdAsync (long id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldByIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.Id == id)
                    .Select (x => new IndustryFieldResponse { Name = x.Name, DataType = x.DataType, Id = x.Id, IndustryId = x.IndustryId, IsRequired = x.IsRequired, Order = x.Order, ToolTip = x.ToolTip })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldByIdAsync), nameof (id), id.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var response = RequestResponse<IndustryFieldResponse>.SearchSuccessful (result, 1, "Industry field");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldByIdAsync), nameof (id), id.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldByIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<IndustryFieldResponse>>> GetIndustryFieldsByIndustryIdAsync (long id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldsByIndustryIdAsync), nameof (id), id.ToString ());
                _logger.LogInformation (openingLog);

                var result = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.IndustryId == id && industry.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new IndustryFieldResponse { Name = x.Name, DataType = x.DataType, Id = x.Id, IndustryId = x.IndustryId, IsRequired = x.IsRequired, Order = x.Order, ToolTip = x.ToolTip })
                    .Skip ((pageNumber - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<IndustryFieldResponse>>.NotFound (null, "Industry fields");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldsByIndustryIdAsync), nameof (id), id.ToString (), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.IndustryId == id && industry.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<IndustryFieldResponse>>.SearchSuccessful (result, count, "Industry fields");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldsByIndustryIdAsync), nameof (id), id.ToString (), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldsByIndustryIdAsync), nameof (id), id.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<IndustryFieldResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<IndustryFieldResponse>>> GetAllIndustryFieldsAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllIndustryFieldsAsync));
                _logger.LogInformation (openingLog);

                var result = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new IndustryFieldResponse { Name = x.Name, DataType = x.DataType, Id = x.Id, IndustryId = x.IndustryId, IsRequired = x.IsRequired, Order = x.Order, ToolTip = x.ToolTip })
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<IndustryFieldResponse>>.NotFound (null, "Industry fields");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllIndustryFieldsAsync), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<IndustryFieldResponse>>.SearchSuccessful (result, count, "Industry fields");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllIndustryFieldsAsync), nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllIndustryFieldsAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<IndustryFieldResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<IndustryFieldResponse>>> GetIndustryFieldsByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldsByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.CreatedBy == id)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new IndustryFieldResponse { Name = x.Name, DataType = x.DataType, Id = x.Id, IndustryId = x.IndustryId, IsRequired = x.IsRequired, Order = x.Order, ToolTip = x.ToolTip })
                    .Skip ((pageNumber - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellationToken);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<IndustryFieldResponse>>.NotFound (null, "Industry fields");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldsByUserIdAsync), nameof (id), id, nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.CreatedBy == id && industry.IsDeleted == false)
                    .LongCountAsync ();

                var response = RequestResponse<List<IndustryFieldResponse>>.SearchSuccessful (result, count, "Industry fields");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldsByUserIdAsync), nameof (id), id, nameof (response.TotalCount), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldsByUserIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<IndustryFieldResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldCountAsync (CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldCountAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.IndustryFields
                    .AsNoTracking ()
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<IndustryFieldResponse>.CountSuccessful (null, count, "Industry field");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldCountAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldCountAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldCountByUserIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldCountByUserIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                long count = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (x => x.CreatedBy == id && x.IsDeleted == false)
                    .LongCountAsync (cancellationToken);

                var response = RequestResponse<IndustryFieldResponse>.CountSuccessful (null, count, "Industry field");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldCountByUserIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldCountByUserIdAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> UpdateIndustryFieldAsync (IndustryFieldDto industryField)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.LastModifiedBy), industryField.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (industryField == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (industryField.Order < 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field cannot have an order less than one");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryField.IndustryId < 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field cannot have an industryID less than one");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var industries = await _context.Industries.AsNoTracking ().Where (x => x.IsDeleted == false).Select (x => x.Id).ToListAsync (industryField.CancellationToken);
                if (industries == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Unable to retrieve industries");
                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (!industries.Contains (industryField.IndustryId))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry does not exist");
                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var industryFields = await _context.IndustryFields.AsNoTracking ().Where (x => x.IsDeleted == false && x.IndustryId == industryField.IndustryId).ToListAsync (industryField.CancellationToken);

                if (industryFields == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.LastModifiedBy), industryField.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryFields.Any (x => x.Name.ToLower () == industryField.Name.Trim ().ToLower () && x.Id != industryField.Id))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.AlreadyExists (null, industryFields.Count, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (!industryFields.Any (x => x.Id == industryField.Id))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (industryFields.Any (x => x.Order == industryField.Order && x.Name.ToLower () != industryField.Name.ToLower ().Trim ()))
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, "An industry field already exists with the same order");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                if (!industryFields.Any (x => x.Id != industryField.Id && x.Order == industryField.Order - 1) && industryField.Order > 1)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.Failed (null, 400, $"An industry field does not exist for the order {industryField.Order - 1}, please resolve this");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.CreatedBy), industryField.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var updateIndustryFieldRequest = await _context.IndustryFields.Where (x => x.Id == industryField.Id).FirstOrDefaultAsync (industryField.CancellationToken);

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = industryField.CancellationToken,
                    CreatedBy = updateIndustryFieldRequest.CreatedBy,
                    Name = "IndustryField",
                    Payload = JsonConvert.SerializeObject (updateIndustryFieldRequest)
                };

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.LastModifiedBy), industryField.LastModifiedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                updateIndustryFieldRequest.DataType = industryField.DataType;
                updateIndustryFieldRequest.IsRequired = industryField.IsRequired;
                updateIndustryFieldRequest.Order = industryField.Order;
                updateIndustryFieldRequest.ToolTip = industryField.ToolTip;
                updateIndustryFieldRequest.IndustryId = industryField.IndustryId;
                updateIndustryFieldRequest.Name = industryField.Name;
                updateIndustryFieldRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateIndustryFieldRequest.LastModifiedBy = industryField.LastModifiedBy;

                await _context.SaveChangesAsync (industryField.CancellationToken);

                var result = _mapper.Map<IndustryFieldResponse> (updateIndustryFieldRequest);
                var response = RequestResponse<IndustryFieldResponse>.Updated (result, 1, "Industry field");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.LastModifiedBy), industryField.LastModifiedBy, response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateIndustryFieldAsync), nameof (industryField.Name), industryField.Name, nameof (industryField.LastModifiedBy), industryField.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldByNameAsync (string name, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetIndustryFieldByNameAsync), nameof (name), name);
                _logger.LogInformation (openingLog);

                var result = await _context.IndustryFields
                    .AsNoTracking ()
                    .Where (industry => industry.Name == name)
                    .Select (x => new IndustryFieldResponse { Name = x.Name, DataType = x.DataType, Id = x.Id, IndustryId = x.IndustryId, IsRequired = x.IsRequired, Order = x.Order, ToolTip = x.ToolTip })
                    .FirstOrDefaultAsync (cancellationToken);

                if (result == null)
                {
                    var badRequest = RequestResponse<IndustryFieldResponse>.NotFound (null, "Industry field");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldByNameAsync), nameof (name), name, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var response = RequestResponse<IndustryFieldResponse>.SearchSuccessful (result, 1, "Industry field");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetIndustryFieldByNameAsync), nameof (name), name, response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetIndustryFieldByNameAsync), nameof (name), name, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<IndustryFieldResponse>.Error (null);
            }
        }
    }
}

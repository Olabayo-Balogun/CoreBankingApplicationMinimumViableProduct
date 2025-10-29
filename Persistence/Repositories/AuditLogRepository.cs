using Application.Interface.Persistence;
using Application.Models;
using Application.Models.Accounts.Response;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Banks.Response;
using Application.Models.Branches.Response;
using Application.Models.Transactions.Response;
using Application.Models.Uploads.Response;
using Application.Models.Users.Response;
using Application.Utility;

using AutoMapper;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ILogger<AuditLogRepository> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AuditLogRepository (ILogger<AuditLogRepository> logger, ApplicationDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public async Task<RequestResponse<AuditLogResponse>> CreateAuditLogAsync (CreateAuditLogCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy);
                _logger.LogInformation (openingLog);

                if (request == null)
                {
                    var badRequest = RequestResponse<AuditLogResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!request.Name.Equals ("Account", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Bank", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Branch", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Transaction", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Upload", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("User", StringComparison.OrdinalIgnoreCase))
                {
                    var badRequest = RequestResponse<AuditLogsQueryResponse>.Failed (null, 400, "Please enter valid details");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                }

                var payload = new AuditLog
                {
                    Name = request.Name,
                    Payload = request.Payload,
                    CreatedBy = request.CreatedBy,
                    IsDeleted = false,
                    DateDeleted = null,
                    LastModifiedBy = null,
                    LastModifiedDate = null,
                    DeletedBy = null,
                    DateCreated = DateTime.UtcNow.AddHours (1)
                };

                payload.CreatedBy = request.CreatedBy;
                payload.Name = request.Name.Trim ();
                payload.Payload = request.Payload;
                payload.PublicId = Guid.NewGuid ().ToString ();

                await _context.AuditLogs.AddAsync (payload, request.CancellationToken);
                await _context.SaveChangesAsync (request.CancellationToken);

                var response = new AuditLogResponse ();
                switch (payload.Name)
                {
                    case "Account":
                        response.AccountLog = JsonConvert.DeserializeObject<AccountResponse> (payload.Payload);
                        break;
                    case "Bank":
                        response.BankLog = JsonConvert.DeserializeObject<BankResponse> (payload.Payload);
                        break;
                    case "Branch":
                        response.BranchLog = JsonConvert.DeserializeObject<BranchResponse> (payload.Payload);
                        break;
                    case "Transaction":
                        response.TransactionLog = JsonConvert.DeserializeObject<TransactionResponse> (payload.Payload);
                        break;
                    case "Upload":
                        response.UploadLog = JsonConvert.DeserializeObject<UploadResponse> (payload.Payload);
                        break;
                    case "User":
                        response.UserLog = JsonConvert.DeserializeObject<UserResponse> (payload.Payload);
                        break;
                    default:
                        break;
                }
                var result = RequestResponse<AuditLogResponse>.Created (response, 1, "Audit log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy, result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<AuditLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<AuditLogsQueryResponse>> CreateMultipleAuditLogAsync (List<CreateAuditLogCommand> requests)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleAuditLogAsync));
                _logger.LogInformation (initiationLog);
                if (requests == null)
                {
                    var badRequest = RequestResponse<AuditLogsQueryResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleAuditLogAsync), badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var payloads = new List<AuditLog> ();
                foreach (var request in requests)
                {
                    string openingLog = Utility.GenerateMethodInitiationLog (nameof (CreateMultipleAuditLogAsync), nameof (request.CreatedBy), request.Name, nameof (request.CreatedBy), request.CreatedBy);
                    _logger.LogInformation (openingLog);

                    if (!request.Name.Equals ("Account", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Bank", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Branch", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Transaction", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("Upload", StringComparison.OrdinalIgnoreCase) && !request.Name.Equals ("User", StringComparison.OrdinalIgnoreCase))
                    {
                        var badRequest = RequestResponse<AuditLogsQueryResponse>.Failed (null, 400, "Please enter valid details");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleAuditLogAsync), nameof (request.Name), request.Name, nameof (request.CreatedBy), request.CreatedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var payload = new AuditLog
                    {
                        Name = request.Name,
                        Payload = request.Payload,
                        CreatedBy = request.CreatedBy,
                        IsDeleted = false,
                        DateDeleted = null,
                        LastModifiedBy = null,
                        LastModifiedDate = null,
                        DeletedBy = null,
                        DateCreated = DateTime.UtcNow.AddHours (1),
                        PublicId = Guid.NewGuid ().ToString ()
                    };
                    payload.DateCreated = DateTime.UtcNow.AddHours (1);
                    payloads.Add (payload);
                }

                await _context.AuditLogs.AddRangeAsync (payloads, requests.First ().CancellationToken);
                await _context.SaveChangesAsync (requests.First ().CancellationToken);

                var response = new AuditLogsQueryResponse
                {
                    AccountLogs = [],
                    BankLogs = [],
                    BranchLogs = [],
                    TransactionLogs = [],
                    UploadLogs = [],
                    UserLogs = []
                };

                foreach (var payload in payloads)
                {
                    switch (payload.Name)
                    {
                        case "Account":
                            response.AccountLogs.Add (JsonConvert.DeserializeObject<AccountResponse> (payload.Payload));
                            break;
                        case "Bank":
                            response.BankLogs.Add (JsonConvert.DeserializeObject<BankResponse> (payload.Payload));
                            break;
                        case "Branch":
                            response.BranchLogs.Add (JsonConvert.DeserializeObject<BranchResponse> (payload.Payload));
                            break;
                        case "Transaction":
                            response.TransactionLogs.Add (JsonConvert.DeserializeObject<TransactionResponse> (payload.Payload));
                            break;
                        case "Upload":
                            response.UploadLogs.Add (JsonConvert.DeserializeObject<UploadResponse> (payload.Payload));
                            break;
                        case "User":
                            response.UserLogs.Add (JsonConvert.DeserializeObject<UserResponse> (payload.Payload));
                            break;
                        default:
                            return RequestResponse<AuditLogsQueryResponse>.NullPayload (null);
                    }
                }

                var result = RequestResponse<AuditLogsQueryResponse>.Created (response, payloads.LongCount (), "Audit logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (CreateMultipleAuditLogAsync), result.Remark);
                _logger.LogInformation (conclusionLog);
                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (CreateAuditLogAsync), ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<AuditLogsQueryResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<AuditLogResponse>> GetAuditLogByIdAsync (string id, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAuditLogByIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var response = await _context.AuditLogs
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.PublicId == id)
                    .FirstOrDefaultAsync (cancellationToken);

                if (response == null)
                {
                    var badRequest = RequestResponse<AuditLogResponse>.NotFound (null, "Audit log");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAuditLogByIdAsync), nameof (id), id, badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                var payload = new AuditLogResponse ();
                switch (response.Name)
                {
                    case "Account":
                        payload.AccountLog = JsonConvert.DeserializeObject<AccountResponse> (response.Payload);
                        break;
                    case "Bank":
                        payload.BankLog = JsonConvert.DeserializeObject<BankResponse> (response.Payload);
                        break;
                    case "Branch":
                        payload.BranchLog = JsonConvert.DeserializeObject<BranchResponse> (response.Payload);
                        break;
                    case "Transaction":
                        payload.TransactionLog = JsonConvert.DeserializeObject<TransactionResponse> (response.Payload);
                        break;
                    case "Upload":
                        payload.UploadLog = JsonConvert.DeserializeObject<UploadResponse> (response.Payload);
                        break;
                    case "User":
                        payload.UserLog = JsonConvert.DeserializeObject<UserResponse> (response.Payload);
                        break;
                    default:
                        break;
                }

                var result = RequestResponse<AuditLogResponse>.SearchSuccessful (payload, 1, "Audit log");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAuditLogByIdAsync), nameof (id), id, result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAuditLogByIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<AuditLogResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<AuditLogsQueryResponse>> GetAuditLogsAsync (string? userId, string? logName, CancellationToken cancellationToken, int pageNumber, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAuditLogsAsync), nameof (userId), userId ?? string.Empty, nameof (logName), logName ?? string.Empty);
                _logger.LogInformation (openingLog);

                AuditLogsQueryResponse result = new ()
                {
                    AccountLogs = [],
                    BankLogs = [],
                    BranchLogs = [],
                    TransactionLogs = [],
                    UploadLogs = [],
                    UserLogs = []
                };

                List<AuditLog>? responses = null;

                long count = 0;
                if (userId != null && logName == null)
                {
                    responses = await _context.AuditLogs
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.CreatedBy == userId)
                        .OrderByDescending (x => x.DateCreated)
                        .Skip ((pageNumber - 1) * pageSize)
                        .Take (pageSize)
                        .ToListAsync (cancellationToken);

                    count = await _context.AuditLogs
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.CreatedBy == userId)
                        .LongCountAsync (cancellationToken);
                }
                else if (userId == null && logName != null)
                {
                    responses = await _context.AuditLogs.AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.Name == logName.Trim ())
                        .OrderByDescending (x => x.DateCreated)
                        .Skip ((pageNumber - 1) * pageSize)
                        .Take (pageSize)
                        .ToListAsync (cancellationToken);

                    count = await _context.AuditLogs
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.Name == logName.Trim ())
                        .LongCountAsync (cancellationToken);
                }
                else if (userId != null && logName != null)
                {
                    responses = await _context.AuditLogs.AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.Name == logName.Trim () && x.CreatedBy == userId)
                        .OrderByDescending (x => x.DateCreated)
                        .Skip ((pageNumber - 1) * pageSize)
                        .Take (pageSize)
                        .ToListAsync (cancellationToken);

                    count = await _context.AuditLogs
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.Name == logName.Trim () && x.CreatedBy == userId)
                        .LongCountAsync (cancellationToken);
                }
                else
                {
                    responses = await _context.AuditLogs.AsNoTracking ()
                        .Where (x => x.IsDeleted == false)
                        .OrderByDescending (x => x.DateCreated)
                        .Skip ((pageNumber - 1) * pageSize)
                        .Take (pageSize)
                        .ToListAsync (cancellationToken);
                    count = await _context.AuditLogs
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false)
                        .LongCountAsync (cancellationToken);
                }


                if (responses.Count < 1)
                {
                    var badResult = RequestResponse<AuditLogsQueryResponse>.NotFound (null, "Audit logs");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAuditLogsAsync), nameof (userId), userId ?? string.Empty, nameof (logName), logName ?? string.Empty, nameof (badResult.TotalCount), badResult.TotalCount.ToString (), badResult.Remark);
                    _logger.LogInformation (closingLog);

                    return badResult;
                }

                foreach (var response in responses)
                {
                    switch (response.Name)
                    {
                        case "Account":
                            result.AccountLogs.Add (JsonConvert.DeserializeObject<AccountResponse> (response.Payload));
                            break;
                        case "Bank":
                            result.BankLogs.Add (JsonConvert.DeserializeObject<BankResponse> (response.Payload));
                            break;
                        case "Branch":
                            result.BranchLogs.Add (JsonConvert.DeserializeObject<BranchResponse> (response.Payload));
                            break;
                        case "Transaction":
                            result.TransactionLogs.Add (JsonConvert.DeserializeObject<TransactionResponse> (response.Payload));
                            break;
                        case "Upload":
                            result.UploadLogs.Add (JsonConvert.DeserializeObject<UploadResponse> (response.Payload));
                            break;
                        case "User":
                            result.UserLogs.Add (JsonConvert.DeserializeObject<UserResponse> (response.Payload));
                            break;
                        default:
                            break;
                    }
                }

                var auditLogResponse = RequestResponse<AuditLogsQueryResponse>.SearchSuccessful (result, count, "Audit logs");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAuditLogsAsync), nameof (userId), userId ?? string.Empty, nameof (logName), logName ?? string.Empty, nameof (auditLogResponse.TotalCount), auditLogResponse.TotalCount.ToString (), auditLogResponse.Remark);

                _logger.LogInformation (conclusionLog);
                return auditLogResponse;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAuditLogsAsync), nameof (userId), userId ?? string.Empty, nameof (logName), logName ?? string.Empty, ex.Message);
                _logger.LogError (errorLog);
                return RequestResponse<AuditLogsQueryResponse>.Error (null);
            }
        }
    }
}

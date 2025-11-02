using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.EmailRequests.Command;
using Application.Models.Users.Command;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Utility = Application.Utility.Utility;

namespace Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserRepository> _logger;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailRequestService _emailRequestService;
        private readonly IAuditLogRepository _auditLogRepository;

        public UserRepository (ApplicationDbContext context, IMapper mapper, IOptions<AppSettings> appsettings, ILogger<UserRepository> logger, IEmailTemplateService emailTemplateService, IEmailRequestService emailRequestService, IAuditLogRepository auditLogRepository)
        {
            _mapper = mapper;
            _context = context;
            _appSettings = appsettings.Value;
            _logger = logger;
            _emailRequestService = emailRequestService;
            _emailTemplateService = emailTemplateService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<RequestResponse<UserResponse>> DeleteMultipleUserAsync (DeleteUsersCommand request)
        {
            try
            {
                string initiationLog = Utility.GenerateMethodInitiationLog (nameof (DeleteMultipleUserAsync), nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (initiationLog);

                List<CreateAuditLogCommand> auditLogs = [];
                List<Domain.Entities.User> users = [];

                foreach (string id in request.UserIds)
                {
                    var userCheck = await _context.Users.Where (x => x.PublicId == id.Trim () && x.IsDeleted == false).FirstOrDefaultAsync ();
                    if (userCheck == null)
                    {
                        var badRequest = RequestResponse<UserResponse>.NotFound (null, "Users");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUserAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    var permissionCheck = await _context.Users.AsNoTracking ().Where (x => x.PublicId == request.DeletedBy.Trim () && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync ();
                    if (permissionCheck == null)
                    {
                        var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUserAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    if (!permissionCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && request.DeletedBy.Trim () != id.Trim ())
                    {
                        var badRequest = RequestResponse<UserResponse>.Unauthorized (null, $"Unauthorized to delete user with ID: {id}");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUserAsync), nameof (id), id, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                    {
                        CancellationToken = request.CancellationToken,
                        CreatedBy = userCheck.PublicId,
                        Name = "User",
                        Payload = JsonConvert.SerializeObject (userCheck)
                    };

                    auditLogs.Add (createAuditLogRequestViewModel);

                    userCheck.IsDeleted = true;
                    userCheck.DeletedBy = request.DeletedBy;
                    userCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                    users.Add (userCheck);
                }

                RequestResponse<AuditLogsQueryResponse> createAuditLog = await _auditLogRepository.CreateMultipleAuditLogAsync (auditLogs);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUserAsync), nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (request.CancellationToken);

                var result = RequestResponse<UserResponse>.Deleted (null, users.Count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteMultipleUserAsync), result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteMultipleUserAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> DeleteUserAsync (DeleteUserCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy);
                _logger.LogInformation (openingLog);

                var userCheck = await _context.Users.Where (x => x.PublicId == request.DeletedBy && x.IsDeleted == false).FirstOrDefaultAsync ();
                if (userCheck == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var permissionCheck = await _context.Users.AsNoTracking ().Where (x => x.PublicId == request.DeletedBy.Trim () && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync ();
                if (permissionCheck == null)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!permissionCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, $"Unauthorized to delete user");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = request.CancellationToken,
                    CreatedBy = userCheck.PublicId,
                    Name = "User",
                    Payload = JsonConvert.SerializeObject (userCheck)
                };

                userCheck.IsDeleted = true;
                userCheck.DeletedBy = request.DeletedBy;
                userCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy, badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                _context.SaveChanges ();

                var result = RequestResponse<UserResponse>.Deleted (null, 1, "User");

                string conlcusionLog = Utility.GenerateMethodConclusionLog (nameof (DeleteUserAsync), nameof (request.UserId), request.UserId, nameof (request.DeletedBy), request.DeletedBy, result.Remark);
                _logger.LogInformation (conlcusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (DeleteUserAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfCreatedUserAsync (CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfCreatedUserAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .LongCountAsync (cancellation);

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfCreatedUserAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfCreatedUserAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfCreatedUserByDateAsync (DateTime date, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfCreatedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                long count = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
                    .LongCountAsync (cancellation);

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfCreatedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfCreatedUserByDateAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfActiveUsersByDateAsync (DateTime date, string period, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfActiveUsersByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (period), period);
                _logger.LogInformation (openingLog);

                long count = 0;
                switch (period.ToLower ().Trim ())
                {
                    case "daily":
                        count = await _context.Users
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.LastLoggedInDate != null && x.LastLoggedInDate.Value.Date == DateTime.UtcNow.AddHours (1).Date)
                        .LongCountAsync (cancellation);
                        break;
                    case "weekly":
                        DateTime currentDate = date;
                        DateTime start = currentDate.Date.AddDays (-(int)currentDate.DayOfWeek);
                        DateTime end = start.AddDays (7);
                        count = await _context.Users
                            .AsNoTracking ()
                            .Where (x => x.IsDeleted == false && x.LastLoggedInDate != null && x.LastLoggedInDate.Value.Date >= start && x.LastLoggedInDate.Value.Date < end)
                            .LongCountAsync (cancellation);
                        break;
                    case "monthly":
                        count = await _context.Users
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.LastLoggedInDate != null && x.LastLoggedInDate.Value.Month == date.Month)
                        .LongCountAsync (cancellation);
                        break;
                    case "yearly":
                        count = await _context.Users
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.LastLoggedInDate != null && x.LastLoggedInDate.Value.Year == date.Year)
                        .LongCountAsync (cancellation);
                        break;
                    default:
                        count = await _context.Users
                        .AsNoTracking ()
                        .Where (x => x.IsDeleted == false && x.LastLoggedInDate != null && x.LastLoggedInDate.Value.Date == date.Date)
                        .LongCountAsync (cancellation);
                        break;
                }

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfActiveUsersByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (period), period, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfActiveUsersByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (period), period, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfUserByRoleAsync (string role, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfUserByRoleAsync), nameof (role), role);
                _logger.LogInformation (openingLog);

                long count = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.UserRole == role)
                    .LongCountAsync (cancellation);

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfUserByRoleAsync), nameof (role), role, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfUserByRoleAsync), nameof (role), role, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfDeletedUserAsync (CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfDeletedUserAsync));
                _logger.LogInformation (openingLog);

                long count = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true)
                    .LongCountAsync (cancellation);

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfDeletedUserAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (closingLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfDeletedUserAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetCountOfDeletedUsersByDateAsync (DateTime date, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetCountOfDeletedUsersByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                long count = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date)
                    .LongCountAsync (cancellation);

                var response = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetCountOfDeletedUsersByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetCountOfDeletedUsersByDateAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetUserByIdAsync (string id, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUserByIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.PublicId == id)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .FirstOrDefaultAsync (cancellation);

                if (result == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUserByIdAsync), nameof (id), id, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUserByIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUserByIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetUserByEmailAddressAsync (string emailAddress, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUserByEmailAddressAsync), nameof (emailAddress), emailAddress);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.Email == emailAddress)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .FirstOrDefaultAsync (cancellation);

                if (result == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUserByEmailAddressAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUserByEmailAddressAsync), nameof (emailAddress), emailAddress, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUserByEmailAddressAsync), nameof (emailAddress), emailAddress, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetUserLocationByIdAsync (string id, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUserLocationByIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.PublicId == id)
                    .Select (x => new UserResponse { PublicId = x.PublicId, CountryOfResidence = x.CountryOfResidence, CountryOfOrigin = x.CountryOfOrigin, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence })
                    .FirstOrDefaultAsync (cancellation);

                if (result == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUserLocationByIdAsync), nameof (id), id, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUserLocationByIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUserLocationByIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> GetUserFullNameByIdAsync (string id, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetUserFullNameByIdAsync), nameof (id), id);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.PublicId == id)
                    .Select (x => new UserResponse { FirstName = x.FirstName, LastName = x.LastName, BusinessName = x.BusinessName })
                    .FirstOrDefaultAsync (cancellation);

                if (result == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetUserFullNameByIdAsync), nameof (id), id, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetUserFullNameByIdAsync), nameof (id), id, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetUserFullNameByIdAsync), nameof (id), id, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<LoginResponse>> LoginAsync (LoginCommand login)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (LoginAsync), nameof (login.Email), login.Email);
                _logger.LogInformation (openingLog);

                var email = login.Email.ToLower ().Trim ();
                var user = await _context.Users
                    .Where (x => x.Email == email && x.IsDeleted == false)
                    .FirstOrDefaultAsync (login.CancellationToken);

                if (user == null)
                {
                    var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "User does not exist");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (LoginAsync), nameof (login.Email), login.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }
                else if (user.EmailConfirmed == false)
                {
                    var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "Please verify your user email");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (LoginAsync), nameof (login.Email), login.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var isPasswordMatch = VerifyPassword (login.Password, user.Password);

                if (!isPasswordMatch)
                {
                    var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "Email address or password incorrect");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (LoginAsync), nameof (login.Email), login.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                string name = "";
                if (user.FirstName != null)
                {
                    name = user.FirstName.Trim ();
                }
                else if (user.BusinessName != null)
                {
                    name = user.BusinessName.Trim ();
                }

                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, name),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, user.UserRole),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.PrimarySid, user.PublicId),
                    new(ClaimTypes.Country, user.CountryOfOrigin)
                };

                var token = GetToken (authClaims);

                user.LastLoggedInDate = DateTime.UtcNow.AddHours (1);
                user.LastModifiedBy = user.PublicId;

                await _context.SaveChangesAsync (login.CancellationToken);

                var response = _mapper.Map<LoginResponse> (user);
                response.ValidTo = token.ValidTo;
                response.Token = new JwtSecurityTokenHandler ().WriteToken (token).ToString ();

                var result = RequestResponse<LoginResponse>.Success (response, 1, "Login successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (LoginAsync), nameof (login.Email), login.Email, nameof (result.TotalCount), result.TotalCount.ToString (), result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (LoginAsync), nameof (login.Email), login.Email, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<LoginResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<LogoutResponse>> LogoutAsync (string userId, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (LogoutAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, "null"),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Role, "null")
                };

                var logoutToken = GetLogoutToken (authClaims);
                var response = new LogoutResponse
                {
                    Token = new JwtSecurityTokenHandler ().WriteToken (logoutToken).ToString ()
                };

                var result = RequestResponse<LogoutResponse>.Success (response, 1, "Logout successful");

                string closingLog = Utility.GenerateMethodConclusionLog (nameof (LogoutAsync), nameof (userId), userId, nameof (result.TotalCount), result.TotalCount.ToString (), result.Remark);
                _logger.LogInformation (closingLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (LogoutAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<LogoutResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<UserResponse>> RegisterAsync (UserDto user)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (RegisterAsync), nameof (user.Email), user.Email);
                _logger.LogInformation (openingLog);

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (RegisterAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                user.Email = user.Email.ToLower ().Trim ();
                user.FirstName = user.FirstName != null ? Utility.ToSentenceCase (user.FirstName.Trim ()) : null;
                user.LastName = user.LastName != null ? Utility.ToSentenceCase (user.LastName.Trim ()) : null;
                user.MiddleName = user.MiddleName != null ? Utility.ToSentenceCase (user.MiddleName.Trim ()) : null;
                user.BusinessName = user.BusinessName != null ? user.BusinessName.ToUpper ().Trim () : user.BusinessName;
                user.PhoneNumber = user.PhoneNumber?.Trim ();
                user.ProfileImage = user.ProfileImage?.Trim ();

                var userRequest = await _context.Users.AsNoTracking ().Where (x => x.Email == user.Email && x.IsDeleted == false && x.EmailConfirmed == true).LongCountAsync ();

                if (userRequest > 0)
                {
                    var badRequest = RequestResponse<UserResponse>.AlreadyExists (null, userRequest, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (RegisterAsync), nameof (user.Email), user.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                string emailDomain = user.Email.Substring (user.Email.Length - 12);
                var payload = _mapper.Map<User> (user);
                Guid token = Guid.NewGuid ();
                string verificationLink = $"{_appSettings.BaseUrl}VerifyEmail?Email={user.Email}&Token={token}";

                if (user.Email.Equals ("admin@cbamvp.runasp.net", StringComparison.OrdinalIgnoreCase))
                {
                    payload.IsDeleted = false;
                    payload.DateDeleted = null;
                    payload.LastModifiedBy = null;
                    payload.LastModifiedDate = null;
                    payload.DeletedBy = null;
                    payload.IsIndividual = true;
                    payload.CreatedBy = null;
                    payload.EmailConfirmed = !_appSettings.IsProduction;
                    payload.DateCreated = DateTime.UtcNow.AddHours (1);
                    payload.UserRole = UserRoles.Admin;
                    payload.Password = HashPassword (user.Password);
                    payload.PasswordHash = payload.Password.GetHashCode ().ToString ();
                    payload.PublicId = Guid.NewGuid ().ToString ();
                    payload.EmailVerificationToken = token.ToString ();
                    payload.PublicId = Guid.NewGuid ().ToString ();
                }
                else if (user.Email.EndsWith ("@cbamvp.runasp.net", StringComparison.OrdinalIgnoreCase))
                {
                    payload.IsDeleted = false;
                    payload.DateDeleted = null;
                    payload.LastModifiedBy = null;
                    payload.LastModifiedDate = null;
                    payload.DeletedBy = null;
                    payload.IsIndividual = true;
                    payload.CreatedBy = null;
                    payload.EmailConfirmed = !_appSettings.IsProduction;
                    payload.DateCreated = DateTime.UtcNow.AddHours (1);
                    payload.UserRole = UserRoles.Staff;
                    payload.Password = HashPassword (user.Password);
                    payload.PasswordHash = payload.Password.GetHashCode ().ToString ();
                    payload.EmailVerificationToken = token.ToString ();
                    payload.PublicId = Guid.NewGuid ().ToString ();
                }
                else
                {
                    payload.IsDeleted = false;
                    payload.DateDeleted = null;
                    payload.LastModifiedBy = null;
                    payload.LastModifiedDate = null;
                    payload.DeletedBy = null;
                    payload.CreatedBy = null;
                    payload.IsIndividual = payload.BusinessName == null;
                    payload.EmailConfirmed = !_appSettings.IsProduction;
                    payload.Password = HashPassword (user.Password);
                    payload.PasswordHash = payload.Password.GetHashCode ().ToString ();
                    payload.DateCreated = DateTime.UtcNow.AddHours (1);
                    payload.UserRole = UserRoles.User;
                    payload.EmailVerificationToken = token.ToString ();
                    payload.PublicId = Guid.NewGuid ().ToString ();
                }

                await _context.Users.AddAsync (payload, user.CancellationToken);
                await _context.SaveChangesAsync (user.CancellationToken);

                string emailInitiationLog = Utility.GenerateMethodInitiationLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email);
                _logger.LogInformation (emailInitiationLog);

                var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", user.CancellationToken);

                string emailConclusionLog = Utility.GenerateMethodConclusionLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email, nameof (template.TotalCount), template.TotalCount.ToString (), template.Remark);
                _logger.LogInformation (emailConclusionLog);

                if (template.IsSuccessful == true && template.Data != null)
                {
                    template.Data.Template = template.Data.Template.Replace ("{userName}", $"{user.FirstName} {user.LastName}");
                    template.Data.Template = template.Data.Template.Replace ("{verificationLink}", verificationLink);

                    var request = new CreateEmailCommand { ToRecipient = payload.Email, Message = template.Data.Template, IsHtml = true, Subject = "CBA Email Verification", CreatedBy = payload.PublicId, CancellationToken = user.CancellationToken, CcRecipient = null, BccRecipient = null };
                    var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);
                }

                var response = _mapper.Map<UserResponse> (payload);
                var result = RequestResponse<UserResponse>.Created (response, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (RegisterAsync), nameof (user.Email), user.Email, nameof (result.TotalCount), result.TotalCount.ToString (), result.Remark);
                _logger.LogInformation (conclusionLog);

                return result;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (RegisterAsync), nameof (user.Email), user.Email, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<UserResponse>> UpdateUserAsync (UserDto user)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                user.Email = user.Email.ToLower ().Trim ();
                user.FirstName = user.FirstName != null ? Utility.ToSentenceCase (user.FirstName.Trim ()) : null;
                user.LastName = user.LastName != null ? Utility.ToSentenceCase (user.LastName.Trim ()) : null;

                var updateUserRequest = await _context.Users.Where (x => x.PublicId == user.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (user.CancellationToken);

                if (updateUserRequest == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var permissionCheck = await _context.Users.Where (x => x.PublicId == user.LastModifiedBy && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync (user.CancellationToken);
                if (permissionCheck == null)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!permissionCheck.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase) && user.LastModifiedBy != user.PublicId)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Unauthorized to update user");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!updateUserRequest.Email.Equals (user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var checkNewEmail = await _context.Users.Where (x => x.Email == user.Email && x.IsDeleted == false && x.EmailConfirmed == true).LongCountAsync (user.CancellationToken);

                    if (checkNewEmail > 0)
                    {
                        var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "User email already exists, you cannot update your email address to the email address of an existing user");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }

                    Guid token = Guid.NewGuid ();
                    string verificationLink = $"{_appSettings.BaseUrl}VerifyEmail?Email={user.Email}&Token={token}";

                    user.EmailConfirmed = false;
                    user.EmailVerificationToken = token.ToString ();

                    string emailInitiationLog = Utility.GenerateMethodInitiationLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email);
                    _logger.LogInformation (emailInitiationLog);

                    var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", user.CancellationToken);

                    string emailConclusionLog = Utility.GenerateMethodConclusionLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email, nameof (template.TotalCount), template.TotalCount.ToString (), template.Remark);
                    _logger.LogInformation (emailConclusionLog);

                    if (template.IsSuccessful == true && template.Data != null)
                    {
                        template.Data.Template = template.Data.Template.Replace ("{userName}", $"{user.FirstName} {user.LastName}");
                        template.Data.Template = template.Data.Template.Replace ("{verificationLink}", verificationLink);

                        var request = new CreateEmailCommand { ToRecipient = user.Email, Message = template.Data.Template, IsHtml = true, Subject = "CBA Email Verification", CreatedBy = updateUserRequest.PublicId, CancellationToken = user.CancellationToken, CcRecipient = null, BccRecipient = null };
                        var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);
                    }
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = user.CancellationToken,
                    CreatedBy = updateUserRequest.PublicId,
                    Name = "User",
                    Payload = JsonConvert.SerializeObject (updateUserRequest)
                };

                updateUserRequest.Email = user.Email;
                updateUserRequest.FirstName = user.FirstName;
                updateUserRequest.ProfileImage = user.ProfileImage;
                updateUserRequest.LastName = user.LastName;
                updateUserRequest.BusinessName = user.BusinessName != null ? user.BusinessName.ToUpper ().Trim () : user.BusinessName;
                updateUserRequest.PhoneNumber = user.PhoneNumber;
                updateUserRequest.CountryOfResidence = user.CountryOfResidence;
                updateUserRequest.LastModifiedBy = user.LastModifiedBy;
                updateUserRequest.Email = user.Email;
                updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateUserRequest.CountryOfOrigin = user.CountryOfOrigin;

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (user.CancellationToken);

                var result = _mapper.Map<UserResponse> (updateUserRequest);
                var response = RequestResponse<UserResponse>.Updated (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserAsync), nameof (user.LastModifiedBy), user.LastModifiedBy, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateUserAsync), nameof (user.Email), user.Email, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<UserResponse>> UpdateUserRoleAsync (UpdateUserRoleCommand updateUserResponsibility)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateUserRoleAsync), nameof (updateUserResponsibility.UserId), updateUserResponsibility.UserId, nameof (updateUserResponsibility.LastModifiedBy), updateUserResponsibility.LastModifiedBy);
                _logger.LogInformation (openingLog);

                if (updateUserResponsibility == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NullPayload (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserRoleAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var updateUserRequest = await _context.Users.Where (x => x.PublicId == updateUserResponsibility.UserId && x.IsDeleted == false).FirstOrDefaultAsync (updateUserResponsibility.CancellationToken);

                if (updateUserRequest == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserRoleAsync), nameof (updateUserResponsibility.UserId), updateUserResponsibility.UserId, nameof (updateUserResponsibility.LastModifiedBy), updateUserResponsibility.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = updateUserResponsibility.CancellationToken,
                    CreatedBy = updateUserRequest.PublicId,
                    Name = "User",
                    Payload = JsonConvert.SerializeObject (updateUserRequest)
                };

                updateUserRequest.LastModifiedBy = updateUserResponsibility.LastModifiedBy;
                updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateUserRequest.UserRole = updateUserResponsibility.UserRole;

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserRoleAsync), nameof (updateUserResponsibility.UserId), updateUserResponsibility.UserId, nameof (updateUserResponsibility.LastModifiedBy), updateUserResponsibility.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (updateUserResponsibility.CancellationToken);

                var result = _mapper.Map<UserResponse> (updateUserRequest);
                var response = RequestResponse<UserResponse>.Updated (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserRoleAsync), nameof (updateUserResponsibility.UserId), updateUserResponsibility.UserId, nameof (updateUserResponsibility.LastModifiedBy), updateUserResponsibility.LastModifiedBy, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateUserRoleAsync), nameof (updateUserResponsibility.UserId), updateUserResponsibility.UserId, nameof (updateUserResponsibility.LastModifiedBy), updateUserResponsibility.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<UserResponse>> UpdateUserProfileImageAsync (string profileImage, string userId, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdateUserProfileImageAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

                var updateUserRequest = await _context.Users.Where (x => x.PublicId == userId && x.IsDeleted == false).FirstOrDefaultAsync (cancellation);
                if (updateUserRequest == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserProfileImageAsync), nameof (userId), userId, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                CreateAuditLogCommand createAuditLogRequestViewModel = new ()
                {
                    CancellationToken = cancellation,
                    CreatedBy = updateUserRequest.PublicId,
                    Name = "User",
                    Payload = JsonConvert.SerializeObject (updateUserRequest)
                };

                updateUserRequest.LastModifiedBy = userId;
                updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateUserRequest.ProfileImage = profileImage;

                RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

                if (createAuditLog.IsSuccessful == false)
                {
                    var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserProfileImageAsync), nameof (userId), userId, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                await _context.SaveChangesAsync (cancellation);

                var result = _mapper.Map<UserResponse> (updateUserRequest);
                var response = RequestResponse<UserResponse>.Updated (result, 1, "User");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdateUserProfileImageAsync), nameof (userId), userId, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdateUserProfileImageAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }

        }

        public async Task<RequestResponse<UserResponse>> VerifyUserEmailAsync (EmailVerificationCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email);
                _logger.LogInformation (openingLog);

                var email = request.Email.ToLower ().Trim ();
                var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync ();

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.EmailConfirmed == true)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Email is already verified");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.EmailVerificationToken != request.Token)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email verification failed due to incorrect token");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                user.EmailConfirmed = true;
                user.EmailVerificationToken = null;

                await _context.SaveChangesAsync ();

                var result = _mapper.Map<UserResponse> (user);
                var response = RequestResponse<UserResponse>.Success (result, 1, "Email is verification successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);
                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (VerifyUserEmailAsync), nameof (request.Email), request.Email, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> ForgotPasswordAsync (string userEmail, CancellationToken cancellation)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail);
                _logger.LogInformation (openingLog);

                var email = userEmail.ToLower ().Trim ();
                var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync (cancellation);

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.EmailConfirmed == false)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                Guid token = Guid.NewGuid ();
                string resetLink = $"{_appSettings.BaseUrl}ChangePassword?email={user.Email}&token={token}";

                user.PasswordResetToken = token;

                await _context.SaveChangesAsync (cancellation);

                string emailInitiationLog = Utility.GenerateMethodInitiationLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email);
                _logger.LogInformation (emailInitiationLog);

                var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("PasswordReset", cancellation);

                string emailConclusionLog = Utility.GenerateMethodConclusionLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (user.Email), user.Email, nameof (template.TotalCount), template.TotalCount.ToString (), template.Remark);
                _logger.LogInformation (emailConclusionLog);

                if (template.IsSuccessful == true && template.Data != null)
                {
                    template.Data.Template = template.Data.Template.Replace ("{userName}", $"{user.FirstName} {user.LastName}");
                    template.Data.Template = template.Data.Template.Replace ("{resetLink}", resetLink);

                    var request = new CreateEmailCommand { ToRecipient = user.Email, Message = template.Data.Template, IsHtml = true, Subject = "CBA Password Reset", CreatedBy = user.PublicId, CancellationToken = cancellation };
                    var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);

                    if (emailRequest.IsSuccessful != true)
                    {
                        var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Password reset failed");

                        string closingLog = Utility.GenerateMethodConclusionLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                        _logger.LogInformation (closingLog);

                        return badRequest;
                    }
                }

                var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password reset successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (ForgotPasswordAsync), nameof (userEmail), userEmail, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> ChangePasswordAsync (ChangePasswordCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email);
                _logger.LogInformation (openingLog);

                var email = request.Email.ToLower ().Trim ();
                var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.EmailConfirmed == false)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!request.NewPassword.Equals (request.ConfirmPassword))
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Password does not match");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.PasswordResetToken != request.Token)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Please input valid token");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                user.Password = HashPassword (request.NewPassword);
                user.PasswordHash = user.Password.GetHashCode ().ToString ();

                await _context.SaveChangesAsync (request.CancellationToken);

                var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password update successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (ChangePasswordAsync), nameof (request.Email), request.Email, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> UpdatePasswordAsync (UpdatePasswordCommand request)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy);
                _logger.LogInformation (openingLog);

                var user = await _context.Users.Where (x => x.PublicId == request.LastModifiedBy && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

                if (user == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (user.EmailConfirmed == false)
                {
                    var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (!request.NewPassword.Equals (request.ConfirmPassword))
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Password does not match");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var isPasswordMatch = VerifyPassword (request.NewPassword.Trim (), user.Password.Trim ());

                if (isPasswordMatch == true)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Your old and new password must not match");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);
                    return badRequest;
                }

                user.Password = HashPassword (request.NewPassword);
                user.PasswordHash = user.Password.GetHashCode ().ToString ();
                user.LastModifiedBy = request.LastModifiedBy;

                await _context.SaveChangesAsync (request.CancellationToken);

                var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password update successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (UpdatePasswordAsync), nameof (request.LastModifiedBy), request.LastModifiedBy, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<UserResponse>> ResendEmailVerificationTokenAsync (string emailAddress, CancellationToken cancellationToken)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress);
                _logger.LogInformation (openingLog);

                var updateUserRequest = await _context.Users.Where (x => x.Email == emailAddress && x.IsDeleted == false).FirstOrDefaultAsync (cancellationToken);

                if (updateUserRequest == null)
                {
                    var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (updateUserRequest.EmailConfirmed != false)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Email is already verified");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                string emailInitiationLog = Utility.GenerateMethodInitiationLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (emailAddress), emailAddress);
                _logger.LogInformation (emailInitiationLog);

                var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", cancellationToken);

                string emailConclusionLog = Utility.GenerateMethodConclusionLog (nameof (_emailTemplateService.GetEmailTemplateByTemplateNameAsync), nameof (emailAddress), emailAddress, nameof (template.TotalCount), template.TotalCount.ToString (), template.Remark);
                _logger.LogInformation (emailConclusionLog);

                if (!template.IsSuccessful)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                if (template.Data == null)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var token = Guid.NewGuid ();
                string verificationLink = $"{_appSettings.BaseUrl}VerifyEmail?Email={emailAddress}&Token={token}";

                template.Data.Template = updateUserRequest.BusinessName != null
                    ? template.Data.Template.Replace ("{userName}", $"{updateUserRequest.FirstName}")
                    : template.Data.Template.Replace ("{userName}", $"{updateUserRequest.BusinessName}");

                template.Data.Template = template.Data.Template.Replace ("{verificationLink}", verificationLink);

                var request = new CreateEmailCommand { ToRecipient = emailAddress, Message = template.Data.Template, IsHtml = true, Subject = "CBA Email Verification", CreatedBy = updateUserRequest.PublicId, CancellationToken = cancellationToken, CcRecipient = null, BccRecipient = null };

                updateUserRequest.LastModifiedBy = updateUserRequest.PublicId;
                updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
                updateUserRequest.EmailVerificationToken = token.ToString ();

                await _context.SaveChangesAsync (cancellationToken);

                var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);

                if (!emailRequest.IsSuccessful)
                {
                    var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Token resend successful");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (ResendEmailVerificationTokenAsync), nameof (emailAddress), emailAddress, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<UserResponse>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetDeletedUsersByUserIdAsync (string userId, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetDeletedUsersByUserIdAsync), nameof (userId), userId);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true && x.DeletedBy == userId)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetDeletedUsersByUserIdAsync), nameof (userId), userId, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true && x.DeletedBy == userId).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetDeletedUsersByUserIdAsync), nameof (userId), userId, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetDeletedUsersByUserIdAsync), nameof (userId), userId, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetLatestCreatedUsersAsync (CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetLatestCreatedUsersAsync));
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetLatestCreatedUsersAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetLatestCreatedUsersAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetLatestCreatedUsersAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllDeletedUserByDateAsync (DateTime date, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllDeletedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync ();

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllDeletedUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllDeletedUsersAsync (CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllDeletedUsersAsync));
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUsersAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllDeletedUsersAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllDeletedUsersAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllUserByDateAsync (DateTime date, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"));
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), nameof (result.Count), result.Count.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUserByDateAsync), nameof (date), date.ToString ("dd/MM/yyyy"), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllUserByCountryAsync (string name, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUserByCountryAsync), nameof (name), name);
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.CountryOfOrigin == name || x.CountryOfResidence == name)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByCountryAsync), nameof (name), name, nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.CountryOfOrigin == name || x.CountryOfResidence == name).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByCountryAsync), nameof (name), name, nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);


                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUserByCountryAsync), nameof (name), name, ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllUserByRoleAsync (string role, bool isDeleted, CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUserByRoleAsync), nameof (role), role, nameof (isDeleted), isDeleted.ToString ());
                _logger.LogInformation (openingLog);

                var result = isDeleted == true ? await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == true && x.UserRole == role)
                    .OrderByDescending (x => x.DateDeleted)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation) : await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false && x.UserRole == role)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByRoleAsync), nameof (role), role, nameof (isDeleted), isDeleted.ToString (), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = isDeleted == true ? await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == true && x.UserRole == role).LongCountAsync (cancellation) : await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false && x.UserRole == role).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUserByRoleAsync), nameof (role), role, nameof (isDeleted), isDeleted.ToString (), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUserByRoleAsync), nameof (role), role, nameof (isDeleted), isDeleted.ToString (), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public async Task<RequestResponse<List<UserResponse>>> GetAllUsersAsync (CancellationToken cancellation, int page, int pageSize)
        {
            try
            {
                string openingLog = Utility.GenerateMethodInitiationLog (nameof (GetAllUsersAsync));
                _logger.LogInformation (openingLog);

                var result = await _context.Users
                    .AsNoTracking ()
                    .Where (x => x.IsDeleted == false)
                    .OrderByDescending (x => x.DateCreated)
                    .Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
                    .Skip ((page - 1) * pageSize)
                    .Take (pageSize)
                    .ToListAsync (cancellation);

                if (result.Count < 1)
                {
                    var badRequest = RequestResponse<List<UserResponse>>.NotFound (null, "Users");

                    string closingLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUsersAsync), nameof (badRequest.TotalCount), badRequest.TotalCount.ToString (), badRequest.Remark);
                    _logger.LogInformation (closingLog);

                    return badRequest;
                }

                var count = await _context.Users
                .AsNoTracking ()
                .Where (x => x.IsDeleted == false).LongCountAsync (cancellation);

                var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");

                string conclusionLog = Utility.GenerateMethodConclusionLog (nameof (GetAllUsersAsync), nameof (response.TotalCount), response.TotalCount.ToString (), response.Remark);
                _logger.LogInformation (conclusionLog);

                return response;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetAllUsersAsync), ex.Message);
                _logger.LogError (errorLog);

                return RequestResponse<List<UserResponse>>.Error (null);
            }
        }

        public JwtSecurityToken GetToken (List<Claim> authClaims)
        {
            try
            {
                var authSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_appSettings.Secret));
                var token = new JwtSecurityToken (
                    issuer: _appSettings.ValidIssuer,
                    audience: _appSettings.ValidAudience,
                    expires: DateTime.UtcNow.AddMinutes (10),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials (authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return token;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetToken), ex.Message);
                _logger.LogError (errorLog);

                throw;
            }

        }

        public JwtSecurityToken GetLogoutToken (List<Claim> authClaims)
        {
            try
            {
                var authSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_appSettings.Secret));

                var token = new JwtSecurityToken (
                    issuer: _appSettings.ValidIssuer,
                    audience: _appSettings.ValidAudience,
                    expires: DateTime.UtcNow.AddHours (-1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials (authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return token;
            }
            catch (Exception ex)
            {
                string errorLog = Utility.GenerateMethodExceptionLog (nameof (GetLogoutToken), ex.Message);
                _logger.LogError (errorLog);

                throw;
            }

        }

        public string HashPassword (string password)
        {
            return BCrypt.Net.BCrypt.HashPassword (password);
        }

        public bool VerifyPassword (string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify (password, hashedPassword);
        }
    }
}

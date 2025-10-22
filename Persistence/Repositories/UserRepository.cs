using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
				_logger.LogInformation ($"DeleteMultipleUser begins at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy}");
				List<CreateAuditLogCommand> auditLogs = [];
				List<Domain.Entities.User> users = [];

				foreach (string id in request.UserIds)
				{
					var userCheck = await _context.Users.Where (x => x.PublicId == id.Trim () && x.IsDeleted == false).FirstOrDefaultAsync ();
					if (userCheck == null)
					{
						var badRequest = RequestResponse<UserResponse>.NotFound (null, "Users");
						_logger.LogInformation ($"DeleteMultipleUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					var permissionCheck = await _context.Users.AsNoTracking ().Where (x => x.PublicId == request.DeletedBy.Trim () && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync ();
					if (permissionCheck == null)
					{
						var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");
						_logger.LogInformation ($"DeleteUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
						return badRequest;
					}

					if (!permissionCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && request.DeletedBy.Trim () != id.Trim ())
					{
						var badRequest = RequestResponse<UserResponse>.Unauthorized (null, $"Unauthorized to delete user with ID: {id}");
						_logger.LogInformation ($"DeleteUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
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
					_logger.LogInformation ($"DeleteMultipleUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_context.UpdateRange (users);
				await _context.SaveChangesAsync (request.CancellationToken);

				var result = RequestResponse<UserResponse>.Deleted (null, users.Count, "Users");
				_logger.LogInformation ($"DeleteMultipleUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteMultipleUser by UserPublicId: {request.DeletedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> DeleteUserAsync (DeleteUserCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteUser begins at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy}");
				var userCheck = await _context.Users.Where (x => x.PublicId == request.DeletedBy && x.IsDeleted == false).FirstOrDefaultAsync ();
				if (userCheck == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"DeleteUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var permissionCheck = await _context.Users.AsNoTracking ().Where (x => x.PublicId == request.DeletedBy.Trim () && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync ();
				if (permissionCheck == null)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");
					_logger.LogInformation ($"DeleteUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!permissionCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase))
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, $"Unauthorized to delete user");
					_logger.LogInformation ($"DeleteUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = request.CancellationToken,
					CreatedBy = userCheck.PublicId,
					Name = "User",
					Payload = JsonConvert.SerializeObject (userCheck)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"DeleteUser successfully ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				userCheck.IsDeleted = true;
				userCheck.DeletedBy = request.DeletedBy;
				userCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Update (userCheck);
				_context.SaveChanges ();

				var result = RequestResponse<UserResponse>.Deleted (null, 1, "User");
				_logger.LogInformation ($"DeleteUser successfully ends at {DateTime.UtcNow.AddHours (1)} by userId: {request.DeletedBy} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteUser by UserPublicId: {request.DeletedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllDeletedUserByDateAsync (DateTime date, CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllDeletedUserByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllDeletedUserByDate ends at {DateTime.UtcNow.AddHours (1)} for Date: {date} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date.Date).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllDeletedUserByDate ends at {DateTime.UtcNow.AddHours (1)} for Date: {date} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllDeletedUserByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllDeletedUsersAsync (CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllDeletedUsers begins at {DateTime.UtcNow.AddHours (1)}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllDeletedUsers ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllDeletedUsers ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllDeletedUsers exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllUserByDateAsync (DateTime date, CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUserByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllUserByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllUserByDate for Date: {date} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUserByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllUserByCountryAsync (string name, CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUserByCountry begins at {DateTime.UtcNow.AddHours (1)} for Country: {name}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllUserByState for Country: {name} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.CountryOfOrigin == name || x.CountryOfResidence == name).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllUserByState for Country: {name} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUserByState for Country: {name} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllUserByRoleAsync (string role, bool isDeleted, CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUserByRole begins at {DateTime.UtcNow.AddHours (1)} for role: {role}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllUserByRole for role: {role} ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = isDeleted == true ? await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true && x.UserRole == role).LongCountAsync (cancellation) : await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false && x.UserRole == role).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllUserByRole for role: {role} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUserByRole for role: {role} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetAllUsersAsync (CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetAllUser begins at {DateTime.UtcNow.AddHours (1)}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetAllUser ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetAllUser ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUser exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfCreatedUserAsync (CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfCreatedUser begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false)
					.LongCountAsync (cancellation);

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfCreatedUser ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetAllUser exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfCreatedUserByDateAsync (DateTime date, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfCreatedUserByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
				long count = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.DateCreated.Date == date.Date)
					.LongCountAsync (cancellation);

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfCreatedUserByDate ends at {DateTime.UtcNow.AddHours (1)} for Date: {date} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfCreatedUserByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfActiveUsersByDateAsync (DateTime date, string period, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfCreatedUserByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
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

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfCreatedUserByDate ends at {DateTime.UtcNow.AddHours (1)} for Date: {date} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfCreatedUserByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfUserByRoleAsync (string role, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfUserByRole begins at {DateTime.UtcNow.AddHours (1)} for role: {role}");
				long count = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.UserRole == role)
					.LongCountAsync (cancellation);

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfUserByRole ends at {DateTime.UtcNow.AddHours (1)} for role: {role} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfUserByRole for role: {role} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfDeletedUserAsync (CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfDeletedUser begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true)
					.LongCountAsync (cancellation);

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfDeletedUser ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfDeletedUser exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetCountOfDeletedUsersByDateAsync (DateTime date, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetCountOfDeletedUsersByDate begins at {DateTime.UtcNow.AddHours (1)} for Date: {date}");
				long count = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == true && x.DateDeleted != null && x.DateDeleted.Value.Date == date)
					.LongCountAsync (cancellation);

				var result = RequestResponse<UserResponse>.CountSuccessful (null, count, "Users");
				_logger.LogInformation ($"GetCountOfDeletedUsersByDate ends at {DateTime.UtcNow.AddHours (1)} for Date: {date} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetCountOfDeletedUsersByDate for Date: {date} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetDeletedUsersByUserIdAsync (string userId, CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetDeletedUsersByUserId begins at {DateTime.UtcNow.AddHours (1)} for deleterId: {userId}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetDeletedUsersByUserId ends at {DateTime.UtcNow.AddHours (1)} for deleterId: {userId} and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == true && x.DeletedBy == userId).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetDeletedUsersByUserId ends at {DateTime.UtcNow.AddHours (1)} for deleterId: {userId} and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetDeletedUsersByUserId for deleterId: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<UserResponse>>> GetLatestCreatedUsersAsync (CancellationToken cancellation, int page, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetLatestCreatedUsers begins at {DateTime.UtcNow.AddHours (1)}");
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
					var badResponse = RequestResponse<List<UserResponse>>.NotFound (null, "Users");
					_logger.LogInformation ($"GetLatestCreatedUsers ends at {DateTime.UtcNow.AddHours (1)} with {badResponse.TotalCount} users retrieved and remark: {badResponse.Remark}");
					return badResponse;
				}

				var count = await _context.Users
				.AsNoTracking ()
				.Where (x => x.IsDeleted == false).LongCountAsync (cancellation);

				var response = RequestResponse<List<UserResponse>>.SearchSuccessful (result, count, "Users");
				_logger.LogInformation ($"GetLatestCreatedUsers ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users retrieved and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetLatestCreatedUsers exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public JwtSecurityToken GetToken (List<Claim> authClaims)
		{
			try
			{
				_logger.LogInformation ($"GetToken begins at {DateTime.UtcNow.AddHours (1)}");

				var authSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_appSettings.Secret));
				var token = new JwtSecurityToken (
					issuer: _appSettings.ValidIssuer,
					audience: _appSettings.ValidAudience,
					expires: DateTime.UtcNow.AddMinutes (10),
					claims: authClaims,
					signingCredentials: new SigningCredentials (authSigningKey, SecurityAlgorithms.HmacSha256)
					);
				_logger.LogInformation ($"GetToken begins at {DateTime.UtcNow.AddHours (1)}");
				return token;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetToken exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public JwtSecurityToken GetLogoutToken (List<Claim> authClaims)
		{
			try
			{
				_logger.LogInformation ($"GetLogoutToken begins at {DateTime.UtcNow.AddHours (1)}");
				var authSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_appSettings.Secret));

				var token = new JwtSecurityToken (
					issuer: _appSettings.ValidIssuer,
					audience: _appSettings.ValidAudience,
					expires: DateTime.UtcNow.AddHours (-1),
					claims: authClaims,
					signingCredentials: new SigningCredentials (authSigningKey, SecurityAlgorithms.HmacSha256)
					);
				_logger.LogInformation ($"GetLogoutToken begins at {DateTime.UtcNow.AddHours (1)}");
				return token;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetLogoutToken exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<UserResponse>> GetUserByIdAsync (string id, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetUserById begins at {DateTime.UtcNow.AddHours (1)} for userId: {id}");
				var result = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == id)
					.Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
					.FirstOrDefaultAsync (cancellation);

				if (result == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"GetUserById for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {badRequest.TotalCount} users found and remark: {badRequest.Remark}");
					return badRequest;
				}

				var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");
				_logger.LogInformation ($"GetUserById for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users found and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUserById for userId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetUserByEmailAddressAsync (string emailAddress, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetUserByEmailAddress begins at {DateTime.UtcNow.AddHours (1)} for email: {emailAddress}");
				var result = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.Email == emailAddress)
					.Select (x => new UserResponse { PublicId = x.PublicId, BusinessName = x.BusinessName, BusinessType = x.BusinessType, Bvn = x.Bvn, CountryOfOrigin = x.CountryOfOrigin, CountryOfResidence = x.CountryOfResidence, Nin = x.Nin, DateOfBirth = x.DateOfBirth, Email = x.Email, FirstName = x.FirstName, GroupName = x.GroupName, IdentificationId = x.IdentificationId, IdentificationType = x.IdentificationType, IsIndividual = x.IsIndividual, IsStaff = x.IsStaff, LastName = x.LastName, MiddleName = x.MiddleName, PhoneNumber = x.PhoneNumber, ProfileImage = x.ProfileImage, ProofOfIdentification = x.ProofOfIdentification, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence, UserRole = x.UserRole })
					.FirstOrDefaultAsync (cancellation);

				if (result == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"GetUserByEmailAddress for email: {emailAddress} ends at {DateTime.UtcNow.AddHours (1)} with {badRequest.TotalCount} users found and remark: {badRequest.Remark}");
					return badRequest;
				}

				var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");
				_logger.LogInformation ($"GetUserByEmailAddress for email: {emailAddress} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users found and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUserByEmailAddress for email: {emailAddress} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetUserLocationByIdAsync (string id, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetUserLocationByIdAsync begins at {DateTime.UtcNow.AddHours (1)} for userId: {id}");
				var result = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == id)
					.Select (x => new UserResponse { PublicId = x.PublicId, CountryOfResidence = x.CountryOfResidence, CountryOfOrigin = x.CountryOfOrigin, StateOfOrigin = x.StateOfOrigin, StateOfResidence = x.StateOfResidence })
					.FirstOrDefaultAsync (cancellation);

				if (result == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"GetUserLocationByIdAsync for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {badRequest.TotalCount} users found and remark: {badRequest.Remark}");
					return badRequest;
				}

				var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");
				_logger.LogInformation ($"GetUserLocationByIdAsync for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users found and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUserLocationByIdAsync for userId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> GetUserFullNameByIdAsync (string id, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"GetUserFullNameById begins at {DateTime.UtcNow.AddHours (1)} for userId: {id}");

				var result = await _context.Users
					.AsNoTracking ()
					.Where (x => x.IsDeleted == false && x.PublicId == id)
					.Select (x => new UserResponse { FirstName = x.FirstName, LastName = x.LastName, BusinessName = x.BusinessName })
					.FirstOrDefaultAsync (cancellation);

				if (result == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"GetUserFullNameById for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {badRequest.TotalCount} users found and remark: {badRequest.Remark}");
					return badRequest;
				}

				var response = RequestResponse<UserResponse>.SearchSuccessful (result, 1, "User");
				_logger.LogInformation ($"GetUserFullNameById for userId: {id} ends at {DateTime.UtcNow.AddHours (1)} with {response.TotalCount} users found and remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetUserFullNameById for userId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}


		public async Task<RequestResponse<LoginResponse>> LoginAsync (LoginCommand login)
		{
			try
			{
				_logger.LogInformation ($"Login begins at {DateTime.UtcNow.AddHours (1)} for user with email: {login.Email}");

				var email = login.Email.ToLower ().Trim ();
				var user = await _context.Users
					.Where (x => x.Email == email && x.IsDeleted == false)
					.FirstOrDefaultAsync (login.CancellationToken);

				if (user == null)
				{
					var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "User does not exist");
					_logger.LogInformation ($"Login ends at {DateTime.UtcNow.AddHours (1)} for user with email: {login.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}
				else if (user.EmailConfirmed == false)
				{
					var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "Please verify your user email");

					_logger.LogInformation ($"Login ends at {DateTime.UtcNow.AddHours (1)} for user with email: {login.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var isPasswordMatch = VerifyPassword (login.Password, user.Password);

				if (!isPasswordMatch)
				{
					var badRequest = RequestResponse<LoginResponse>.Unauthorized (null, "Email address or password incorrect");

					_logger.LogInformation ($"Login ends at {DateTime.UtcNow.AddHours (1)} for user with email: {login.Email} with remark: {badRequest.Remark}");
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

				_context.Users.Update (user);
				await _context.SaveChangesAsync (login.CancellationToken);

				var response = _mapper.Map<LoginResponse> (user);
				response.ValidTo = token.ValidTo;
				response.Token = new JwtSecurityTokenHandler ().WriteToken (token).ToString ();

				var result = RequestResponse<LoginResponse>.Success (response, 1, "Login successful");

				_logger.LogInformation ($"Login ends at {DateTime.UtcNow.AddHours (1)} for user with email: {login.Email} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"Login for user with email: {login.Email} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<LogoutResponse>> LogoutAsync (string userId, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"Logout begins at {DateTime.UtcNow.AddHours (1)} for user with userId: {userId}");

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
				_logger.LogInformation ($"Logout ends at {DateTime.UtcNow.AddHours (1)} for user with userId: {userId} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"Logout for user with userId: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<UserResponse>> RegisterAsync (UserDto user)
		{
			try
			{
				_logger.LogInformation ($"Registration begins at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");
				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NullPayload (null);
					_logger.LogInformation ($"Registration ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

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
					_logger.LogInformation ($"Registration ends at {DateTime.UtcNow.AddHours (1)} by user with email: {user.Email} with remark: {badRequest.Remark}");
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

				_logger.LogInformation ($"Fetching email verification template begins at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");
				var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", user.CancellationToken);
				_logger.LogInformation ($"Fetching email verification template ends at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");

				if (template.IsSuccessful == true && template.Data != null)
				{
					template.Data.Template = template.Data.Template.Replace ("{userName}", $"{user.FirstName} {user.LastName}");
					template.Data.Template = template.Data.Template.Replace ("{verificationLink}", verificationLink);

					var request = new CreateEmailCommand { ToRecipient = payload.Email, Message = template.Data.Template, IsHtml = true, Subject = "CBA Email Verification", CreatedBy = payload.PublicId, CancellationToken = user.CancellationToken, CcRecipient = null, BccRecipient = null };
					var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);
				}

				var response = _mapper.Map<UserResponse> (payload);
				var result = RequestResponse<UserResponse>.Created (response, 1, "User");
				_logger.LogInformation ($"Registration ends at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email} with remark: {result.Remark}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"Registration for user with email: {user.Email} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<UserResponse>> UpdateUserAsync (UserDto user)
		{
			try
			{
				_logger.LogInformation ($"UpdateUser begins at {DateTime.UtcNow.AddHours (1)} by userId: {user.LastModifiedBy}");
				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");
					return badRequest;
				}

				user.Email = user.Email.ToLower ().Trim ();
				user.FirstName = user.FirstName != null ? Utility.ToSentenceCase (user.FirstName.Trim ()) : null;
				user.LastName = user.LastName != null ? Utility.ToSentenceCase (user.LastName.Trim ()) : null;

				var updateUserRequest = await _context.Users.Where (x => x.PublicId == user.PublicId && x.IsDeleted == false).FirstOrDefaultAsync (user.CancellationToken);

				if (updateUserRequest == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.PublicId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var permissionCheck = await _context.Users.Where (x => x.PublicId == user.LastModifiedBy && x.IsDeleted == false).Select (x => x.UserRole).FirstOrDefaultAsync (user.CancellationToken);
				if (permissionCheck == null)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Cannot verify user identity");
					_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.PublicId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!permissionCheck.Equals ("Admin", StringComparison.OrdinalIgnoreCase) && user.LastModifiedBy != user.PublicId)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Unauthorized to update user");
					_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.PublicId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!updateUserRequest.Email.Equals (user.Email, StringComparison.OrdinalIgnoreCase))
				{
					var checkNewEmail = await _context.Users.Where (x => x.Email == user.Email && x.IsDeleted == false && x.EmailConfirmed == true).LongCountAsync (user.CancellationToken);

					if (checkNewEmail > 0)
					{
						var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "User email already exists, you cannot update your email address to the email address of an existing user");
						_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.PublicId} with remark: {badRequest.Remark}");
						return badRequest;
					}

					_logger.LogInformation ($"Fetching email verification template begins at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");

					Guid token = Guid.NewGuid ();
					string verificationLink = $"{_appSettings.BaseUrl}VerifyEmail?Email={user.Email}&Token={token}";

					user.EmailConfirmed = false;
					user.EmailVerificationToken = token.ToString ();

					var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", user.CancellationToken);
					_logger.LogInformation ($"Fetching email verification template ends at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");
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

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateUser ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

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

				_context.Users.Update (updateUserRequest);
				await _context.SaveChangesAsync (user.CancellationToken);

				var result = _mapper.Map<UserResponse> (updateUserRequest);
				var response = RequestResponse<UserResponse>.Updated (result, 1, "User");
				_logger.LogInformation ($"UpdateUser for user with userId: {user.LastModifiedBy} ends at {DateTime.UtcNow.AddHours (1)} by userId: {user.LastModifiedBy} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateUser for user with userId: {user.LastModifiedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<UserResponse>> UpdateUserRoleAsync (UpdateUserRoleCommand updateUserResponsibility)
		{
			try
			{
				_logger.LogInformation ($"UpdateUserRole begins at {DateTime.UtcNow.AddHours (1)} by userId: {updateUserResponsibility.LastModifiedBy}");
				if (updateUserResponsibility == null)
				{
					var badRequest = RequestResponse<UserResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateUserRole ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var updateUserRequest = await _context.Users.Where (x => x.PublicId == updateUserResponsibility.UserId && x.IsDeleted == false).FirstOrDefaultAsync (updateUserResponsibility.CancellationToken);

				if (updateUserRequest == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"UpdateUserRole ends at {DateTime.UtcNow.AddHours (1)} by userId: {updateUserResponsibility.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = updateUserResponsibility.CancellationToken,
					CreatedBy = updateUserRequest.PublicId,
					Name = "User",
					Payload = JsonConvert.SerializeObject (updateUserRequest)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateUserRole ends at {DateTime.UtcNow.AddHours (1)} by userId: {updateUserResponsibility.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				updateUserRequest.LastModifiedBy = updateUserResponsibility.LastModifiedBy;
				updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateUserRequest.UserRole = updateUserResponsibility.UserRole;

				_context.Users.Update (updateUserRequest);
				await _context.SaveChangesAsync (updateUserResponsibility.CancellationToken);

				var result = _mapper.Map<UserResponse> (updateUserRequest);
				var response = RequestResponse<UserResponse>.Updated (result, 1, "User");
				_logger.LogInformation ($"UpdateUserRole ends at {DateTime.UtcNow.AddHours (1)} by userId: {updateUserResponsibility.LastModifiedBy} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateUserRole for user by userId: {updateUserResponsibility.LastModifiedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public async Task<RequestResponse<UserResponse>> UpdateUserProfileImageAsync (string profileImage, string userId, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"UpdateUserProfileImage begins at {DateTime.UtcNow.AddHours (1)} by userId: {userId}");
				var updateUserRequest = await _context.Users.Where (x => x.PublicId == userId && x.IsDeleted == false).FirstOrDefaultAsync (cancellation);
				if (updateUserRequest == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"UpdateUserProfileImage ends at {DateTime.UtcNow.AddHours (1)} by userId: {userId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				CreateAuditLogCommand createAuditLogRequestViewModel = new ()
				{
					CancellationToken = cancellation,
					CreatedBy = updateUserRequest.PublicId,
					Name = "Country",
					Payload = JsonConvert.SerializeObject (updateUserRequest)
				};

				RequestResponse<AuditLogResponse> createAuditLog = await _auditLogRepository.CreateAuditLogAsync (createAuditLogRequestViewModel);

				if (createAuditLog.IsSuccessful == false)
				{
					var badRequest = RequestResponse<UserResponse>.AuditLogFailed (null);
					_logger.LogInformation ($"UpdateUserProfileImage ends at {DateTime.UtcNow.AddHours (1)} by userId: {userId} with remark: {badRequest.Remark}");
					return badRequest;
				}

				updateUserRequest.LastModifiedBy = userId;
				updateUserRequest.LastModifiedDate = DateTime.UtcNow.AddHours (1);
				updateUserRequest.ProfileImage = profileImage;

				_context.Users.Update (updateUserRequest);
				await _context.SaveChangesAsync (cancellation);

				var result = _mapper.Map<UserResponse> (updateUserRequest);
				var response = RequestResponse<UserResponse>.Updated (result, 1, "User");
				_logger.LogInformation ($"UpdateUserProfileImage ends at {DateTime.UtcNow.AddHours (1)} by userId: {userId} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateUserProfileImage for user with userId: {userId} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}

		}

		public string HashPassword (string password)
		{
			return BCrypt.Net.BCrypt.HashPassword (password);
		}

		// Verify a password against its hashed version
		public bool VerifyPassword (string password, string hashedPassword)
		{
			return BCrypt.Net.BCrypt.Verify (password, hashedPassword);
		}

		public async Task<RequestResponse<UserResponse>> VerifyUserEmailAsync (EmailVerificationCommand request)
		{
			try
			{
				_logger.LogInformation ($"VerifyUserEmail begins at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email}");

				var email = request.Email.ToLower ().Trim ();
				var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync ();

				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"VerifyUserEmail ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.EmailConfirmed == true)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Email is already verified");
					_logger.LogInformation ($"VerifyUserEmail ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.EmailVerificationToken != request.Token)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email verification failed due to incorrect token");
					_logger.LogInformation ($"VerifyUserEmail ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				// Mark the email as verified
				user.EmailConfirmed = true;
				user.EmailVerificationToken = null;

				_context.Users.Update (user);
				await _context.SaveChangesAsync ();

				var result = _mapper.Map<UserResponse> (user);
				var response = RequestResponse<UserResponse>.Success (result, 1, "Email is verification successful");

				_logger.LogInformation ($"VerifyUserEmail ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"VerifyUserEmail for user with user email: {request.Email} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> ForgotPasswordAsync (string userEmail, CancellationToken cancellation)
		{
			try
			{
				_logger.LogInformation ($"ForgotUserPassword begins at {DateTime.UtcNow.AddHours (1)} by user email: {userEmail}");
				var email = userEmail.ToLower ().Trim ();
				var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync (cancellation);

				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"ForgotUserPassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {userEmail} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.EmailConfirmed == false)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");
					_logger.LogInformation ($"ForgotUserPassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {userEmail} with remark: {badRequest.Remark}");
					return badRequest;
				}

				Guid token = Guid.NewGuid ();
				string resetLink = $"{_appSettings.BaseUrl}ChangePassword?email={user.Email}&token={token}";

				user.PasswordResetToken = token;

				_context.Users.Update (user);
				await _context.SaveChangesAsync (cancellation);

				_logger.LogInformation ($"Fetching password reset template begins at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");

				var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("PasswordReset", cancellation);

				_logger.LogInformation ($"Fetching password reset template ends at {DateTime.UtcNow.AddHours (1)} for user with email: {user.Email}");
				if (template.IsSuccessful == true && template.Data != null)
				{
					template.Data.Template = template.Data.Template.Replace ("{userName}", $"{user.FirstName} {user.LastName}");
					template.Data.Template = template.Data.Template.Replace ("{resetLink}", resetLink);

					var request = new CreateEmailCommand { ToRecipient = user.Email, Message = template.Data.Template, IsHtml = true, Subject = "CBA Password Reset", CreatedBy = user.PublicId, CancellationToken = cancellation };
					var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);

					if (emailRequest.IsSuccessful != true)
					{
						var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Password reset failed");
						_logger.LogInformation ($"ForgotUserPassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {userEmail} with remark: {badRequest.Remark}");
						return badRequest;
					}
				}

				var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password reset successful");
				_logger.LogInformation ($"ForgotUserPassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {userEmail} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"ForgotUserPassword for user with user email: {userEmail} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> ChangePasswordAsync (ChangePasswordCommand request)
		{
			try
			{
				_logger.LogInformation ($"ChangePassword begins at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email}");
				var email = request.Email.ToLower ().Trim ();
				var user = await _context.Users.Where (x => x.Email == email && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"ChangePassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.EmailConfirmed == false)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");
					_logger.LogInformation ($"ChangePassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!request.NewPassword.Equals (request.ConfirmPassword))
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Password does not match");
					_logger.LogInformation ($"ChangePassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.PasswordResetToken != request.Token)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Please input valid token");
					_logger.LogInformation ($"ChangePassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email}");
					return badRequest;
				}
				user.Password = HashPassword (request.NewPassword);
				user.PasswordHash = user.Password.GetHashCode ().ToString ();

				_context.Users.Update (user);
				await _context.SaveChangesAsync (request.CancellationToken);

				var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password update successful");
				_logger.LogInformation ($"ChangePassword ends at {DateTime.UtcNow.AddHours (1)} by user email: {request.Email} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"ChangePassword for user with user email: {request.Email} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> UpdatePasswordAsync (UpdatePasswordCommand request)
		{
			try
			{
				_logger.LogInformation ($"UpdatePassword begins at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy}");
				var user = await _context.Users.Where (x => x.PublicId == request.LastModifiedBy && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);

				if (user == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"UpdatePassword ends at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (user.EmailConfirmed == false)
				{
					var badRequest = RequestResponse<UserResponse>.Unauthorized (null, "Email is unverified");
					_logger.LogInformation ($"UpdatePassword ends at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (!request.NewPassword.Equals (request.ConfirmPassword))
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Password does not match");
					_logger.LogInformation ($"UpdatePassword ends at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var isPasswordMatch = VerifyPassword (request.NewPassword.Trim (), user.Password.Trim ());

				if (isPasswordMatch == true)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Your old and new password must not match");
					_logger.LogInformation ($"UpdatePassword ends at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy} with remark: {badRequest.Remark}");
					return badRequest;
				}

				user.Password = HashPassword (request.NewPassword);
				user.PasswordHash = user.Password.GetHashCode ().ToString ();
				user.LastModifiedBy = request.LastModifiedBy;

				_context.Users.Update (user);
				await _context.SaveChangesAsync (request.CancellationToken);


				var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Password update successful");
				_logger.LogInformation ($"UpdatePassword ends at {DateTime.UtcNow.AddHours (1)} by user ID: {request.LastModifiedBy} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdatePassword for user with user ID: {request.LastModifiedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<UserResponse>> ResendEmailVerificationTokenAsync (string emailAddress, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"ResendEmailVerificationToken begins at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress}");

				var updateUserRequest = await _context.Users.Where (x => x.Email == emailAddress && x.IsDeleted == false).FirstOrDefaultAsync (cancellationToken);

				if (updateUserRequest == null)
				{
					var badRequest = RequestResponse<UserResponse>.NotFound (null, "User");
					_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (updateUserRequest.EmailConfirmed != false)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 400, "Email is already verified");
					_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {badRequest.Remark}");
					return badRequest;
				}

				_logger.LogInformation ($"Fetching email verification template begins at {DateTime.UtcNow.AddHours (1)} for user with email: {updateUserRequest.Email}");

				var template = await _emailTemplateService.GetEmailTemplateByTemplateNameAsync ("Registration", cancellationToken);
				_logger.LogInformation ($"Fetching email verification template ends at {DateTime.UtcNow.AddHours (1)} for user with email: {updateUserRequest.Email}");
				if (!template.IsSuccessful)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");
					_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {badRequest.Remark}");
					return badRequest;
				}

				if (template.Data == null)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");
					_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {badRequest.Remark}");
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

				_context.Users.Update (updateUserRequest);
				await _context.SaveChangesAsync (cancellationToken);

				var emailRequest = await _emailRequestService.CreateEmailRequestAsync (request);

				if (!emailRequest.IsSuccessful)
				{
					var badRequest = RequestResponse<UserResponse>.Failed (null, 500, "Token resend unsuccessful");
					_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {badRequest.Remark}");
					return badRequest;
				}

				var response = RequestResponse<UserResponse>.Success (new UserResponse (), 1, "Token resend successful");

				_logger.LogInformation ($"ResendEmailVerificationToken ends at {DateTime.UtcNow.AddHours (1)} by Email: {emailAddress} with remark: {response.Remark}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"ResendEmailVerificationToken for user with Email: {emailAddress} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}
	}
}

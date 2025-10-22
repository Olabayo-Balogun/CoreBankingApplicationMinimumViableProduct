using Application.Interface.Persistence;
using Application.Models;
using Application.Models.AuditLogs.Command;
using Application.Models.AuditLogs.Response;
using Application.Models.Branches.Command;
using Application.Models.Branches.Response;

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
				_logger.LogInformation ($"CreateBranch begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {branch.CreatedBy}");

				if (branch == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NullPayload (null);
					_logger.LogInformation ($"CreateBranch ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var branchCheck = await _context.Branches.AsNoTracking ().Where (x => x.Name == branch.Name.Trim () && x.IsDeleted == false).LongCountAsync ();

				if (branchCheck > 0)
				{
					var badRequest = RequestResponse<BranchResponse>.AlreadyExists (null, branchCheck, "Branch");
					_logger.LogInformation ($"CreateBranch ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by UserPublicId: {branch.CreatedBy}");
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

				_logger.LogInformation ($"CreateBranch ends at {DateTime.UtcNow.AddHours (1)} with remark: {result.Remark} by UserPublicId: {branch.CreatedBy}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CreateBranch by UserPublicId: {branch.CreatedBy} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> DeleteBranchAsync (DeleteBranchCommand request)
		{
			try
			{
				_logger.LogInformation ($"DeleteBranch begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} for Branch with ID: {request.Id}");

				var branchCheck = await _context.Branches.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (branchCheck == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

					_logger.LogInformation ($"DeleteBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

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

					_logger.LogInformation ($"DeleteBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {badRequest.Remark}");

					return badRequest;
				}

				branchCheck.IsDeleted = true;
				branchCheck.DeletedBy = request.DeletedBy;
				branchCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Branches.Update (branchCheck);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<BranchResponse>.Deleted (null, 1, "Branch");

				_logger.LogInformation ($"DeleteBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.DeletedBy} with remark: {result.Remark}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"DeleteBranch by UserPublicId: {request.DeletedBy} for Branch with ID: {request.Id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> CloseBranchAsync (CloseBranchCommand request)
		{
			try
			{
				_logger.LogInformation ($"CloseBranch begins at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.LastModifiedBy} for Branch with ID: {request.Id}");

				var branchCheck = await _context.Branches.Where (x => x.PublicId == request.Id && x.IsDeleted == false).FirstOrDefaultAsync (request.CancellationToken);
				if (branchCheck == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

					_logger.LogInformation ($"CloseBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.LastModifiedBy} with remark: {badRequest.Remark}");

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

					_logger.LogInformation ($"CloseBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.LastModifiedBy} with remark: {badRequest.Remark}");

					return badRequest;
				}

				branchCheck.IsClosed = true;
				branchCheck.LastModifiedBy = request.LastModifiedBy;
				branchCheck.DateDeleted = DateTime.UtcNow.AddHours (1);

				_context.Branches.Update (branchCheck);
				await _context.SaveChangesAsync ();

				var result = RequestResponse<BranchResponse>.Deleted (null, 1, "Branch");

				_logger.LogInformation ($"CloseBranch ends at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {request.LastModifiedBy} with remark: {result.Remark}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"CloseBranch by UserPublicId: {request.LastModifiedBy} for Branch with ID: {request.Id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> GetBranchByPublicIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBranchByPublicId begins at {DateTime.UtcNow.AddHours (1)} for branch with publicId: {id}");

				var result = await _context.Branches
					.AsNoTracking ()
					.Where (branch => branch.PublicId == id)
					.Select (x => new BranchResponse { Lga = x.Lga, Country = x.Country, Code = x.Code, Address = x.Address, State = x.State, Name = x.Name, IsClosed = x.IsClosed, PublicId = x.PublicId })
					.FirstOrDefaultAsync (cancellationToken);

				if (result == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");

					_logger.LogInformation ($"GetBranchByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} for id: {id}");

					return badRequest;
				}

				var response = RequestResponse<BranchResponse>.SearchSuccessful (result, 1, "Branch");
				_logger.LogInformation ($"GetBranchByPublicId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} for id: {id}");
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBranchByPublicId for branch with publicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<List<BranchResponse>>> GetBranchesByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			try
			{
				_logger.LogInformation ($"GetBranchByUserId begins at {DateTime.UtcNow.AddHours (1)} for branch with userPublicId: {id}");

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
					var badResponse = RequestResponse<List<BranchResponse>>.NotFound (null, "Branch");
					_logger.LogInformation ($"GetBranchByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {badResponse.Remark} with count: {result.Count}");

					return badResponse;
				}

				var count = await _context.Branches
					.AsNoTracking ()
					.Where (branch => branch.CreatedBy == id && branch.IsDeleted == false)
					.LongCountAsync ();

				var response = RequestResponse<List<BranchResponse>>.SearchSuccessful (result, count, "Branches");

				_logger.LogInformation ($"GetBranchByUserId ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} with count: {response.TotalCount}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBranchByUserId for branch with userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> GetBranchCountAsync (CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBranchCount begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Branches
					.AsNoTracking ()
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<BranchResponse>.CountSuccessful (null, count, "Branch");
				_logger.LogInformation ($"GetBranchCount ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBranchCount exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> GetBranchCountByUserIdAsync (string id, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation ($"GetBranchCountByUserId for userPublicId: {id} begins at {DateTime.UtcNow.AddHours (1)}");
				long count = await _context.Branches
					.AsNoTracking ()
					.Where (x => x.CreatedBy == id)
					.LongCountAsync (cancellationToken);

				var response = RequestResponse<BranchResponse>.CountSuccessful (null, count, "Branch");
				_logger.LogInformation ($"GetBranchCountByUserId for userPublicId: {id} ends at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"GetBranchCountByUserId for userPublicId: {id} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {ex.Message}");
				throw;
			}
		}

		public async Task<RequestResponse<BranchResponse>> UpdateBranchAsync (BranchDto branch)
		{
			try
			{
				_logger.LogInformation ($"UpdateBranch begins at {DateTime.UtcNow.AddHours (1)} for branch with publicId: {branch.PublicId} by UserPublicId: {branch.LastModifiedBy}");

				if (branch == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NullPayload (null);
					_logger.LogInformation ($"UpdateBranch ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark}");

					return badRequest;
				}

				var updateBranchRequest = await _context.Branches
					.Where (x => x.PublicId == branch.PublicId && x.IsDeleted == false)
					.FirstOrDefaultAsync (branch.CancellationToken);

				if (updateBranchRequest == null)
				{
					var badRequest = RequestResponse<BranchResponse>.NotFound (null, "Branch");
					_logger.LogInformation ($"UpdateBranch ends at {DateTime.UtcNow.AddHours (1)}  with remark: {badRequest.Remark} by UserPublicId: {branch.LastModifiedBy} for branch with Id: {branch.Id}");
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
					_logger.LogInformation ($"UpdateBranch ends at {DateTime.UtcNow.AddHours (1)} with remark: {badRequest.Remark} by UserPublicId: {branch.LastModifiedBy} for branch with Id: {branch.Id}");
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

				_context.Branches.Update (updateBranchRequest);
				await _context.SaveChangesAsync (branch.CancellationToken);

				var result = _mapper.Map<BranchResponse> (updateBranchRequest);
				var response = RequestResponse<BranchResponse>.Updated (result, 1, "Branch");
				_logger.LogInformation ($"UpdateBranch at {DateTime.UtcNow.AddHours (1)} with remark: {response.Remark} by UserPublicId: {branch.LastModifiedBy} for branch with Id: {branch.Id}");

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError ($"UpdateBranch for branch with publicId: {branch.Id} error occurred at {DateTime.UtcNow.AddHours (1)} by UserPublicId: {branch.LastModifiedBy} with message: {ex.Message}");
				throw;
			}
		}
	}
}

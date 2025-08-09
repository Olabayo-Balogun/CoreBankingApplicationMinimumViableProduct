using Application.Model;
using Application.Models.Branches.Command;
using Application.Models.Branches.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IBranchRepository
	{
		Task<RequestResponse<BranchResponse>> CreateBranchAsync (BranchDto branch);
		Task<RequestResponse<BranchResponse>> DeleteBranchAsync (DeleteBranchCommand request);
		Task<RequestResponse<BranchResponse>> CloseBranchAsync (CloseBranchCommand request);
		Task<RequestResponse<BranchResponse>> GetBranchByPublicIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<List<BranchResponse>>> GetBranchesByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
		Task<RequestResponse<BranchResponse>> GetBranchCountAsync (CancellationToken cancellationToken);
		Task<RequestResponse<BranchResponse>> GetBranchCountByUserIdAsync (string id, CancellationToken cancellationToken);
		Task<RequestResponse<BranchResponse>> UpdateBranchAsync (BranchDto branch);

	}
}

using Application.Models;
using Application.Models.IndustryField.Command;
using Application.Models.IndustryField.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
    public interface IIndustryFieldRepository
    {
        Task<RequestResponse<IndustryFieldResponse>> CreateIndustryFieldAsync (IndustryFieldDto industry);
        Task<RequestResponse<IndustryFieldResponse>> DeleteIndustryFieldAsync (DeleteIndustryFieldCommand request);
        Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldByIdAsync (long id, CancellationToken cancellationToken);
        Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldByNameAsync (string name, CancellationToken cancellationToken);
        Task<RequestResponse<List<IndustryFieldResponse>>> GetIndustryFieldsByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<List<IndustryFieldResponse>>> GetIndustryFieldsByIndustryIdAsync (long id, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldCountAsync (CancellationToken cancellationToken);
        Task<RequestResponse<IndustryFieldResponse>> GetIndustryFieldCountByUserIdAsync (string id, CancellationToken cancellationToken);
        Task<RequestResponse<IndustryFieldResponse>> UpdateIndustryFieldAsync (IndustryFieldDto account);
        Task<RequestResponse<List<IndustryFieldResponse>>> GetAllIndustryFieldsAsync (CancellationToken cancellationToken);
    }
}

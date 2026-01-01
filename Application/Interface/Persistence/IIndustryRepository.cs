using Application.Models;
using Application.Models.Industry.Command;
using Application.Models.Industry.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
    public interface IIndustryRepository
    {
        Task<RequestResponse<IndustryResponse>> CreateIndustryAsync (IndustryDto industry);
        Task<RequestResponse<IndustryResponse>> DeleteIndustryAsync (DeleteIndustryCommand request);
        Task<RequestResponse<IndustryResponse>> GetIndustryByIdAsync (long id, CancellationToken cancellationToken);
        Task<RequestResponse<IndustryResponse>> GetIndustryByNameAsync (string name, CancellationToken cancellationToken);
        Task<RequestResponse<List<IndustryResponse>>> GetIndustriesByUserIdAsync (string id, CancellationToken cancellationToken, int pageNumber, int pageSize);
        Task<RequestResponse<IndustryResponse>> GetIndustryCountAsync (CancellationToken cancellationToken);
        Task<RequestResponse<IndustryResponse>> GetIndustryCountByUserIdAsync (string id, CancellationToken cancellationToken);
        Task<RequestResponse<IndustryResponse>> UpdateIndustryAsync (IndustryDto account);
        Task<RequestResponse<List<IndustryResponse>>> GetAllIndustriesAsync (CancellationToken cancellationToken);
    }
}

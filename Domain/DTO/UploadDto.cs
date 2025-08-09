using Microsoft.AspNetCore.Http;

namespace Domain.DTO
{
	public class UploadDto : AuditableEntityDto
	{
		public string FilePath { get; set; }
		public IFormFile UploadFile { get; set; }
		public decimal FileSize { get; set; }
		public string FileFormat { get; set; }
		public string? PublicId { get; set; }
		public string RootFilePath { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Uploads.Response
{
    public class UploadResponse
    {
        public string PublicId { get; set; }
        [StringLength (200, ErrorMessage = "Input must not exceed 200 characters")]
        public string FilePath { get; set; }
        public decimal FileSize { get; set; }
        [StringLength (20, ErrorMessage = "Input must not exceed 20 characters")]
        public string FileFormat { get; set; }
    }
}

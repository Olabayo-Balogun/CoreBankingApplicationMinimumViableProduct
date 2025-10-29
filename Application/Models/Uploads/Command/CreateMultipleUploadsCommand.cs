using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Uploads.Command
{
    public class CreateMultipleUploadsCommand
    {
        /// <summary>
        /// The ProductPublicId of the product you're uploading the Content to, it is not required 
        /// </summary>
        public string? ProductPublicId { get; set; }
        /// <summary>
        /// The upload files
        /// </summary>
        [Required (ErrorMessage = "Please upload file")]
        public List<IFormFile> UploadFiles { get; set; }
        /// <summary>
        /// The userId of the person uploading the file
        /// </summary>
        [Required (ErrorMessage = "Please input CreatedBy")]
        public string CreatedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

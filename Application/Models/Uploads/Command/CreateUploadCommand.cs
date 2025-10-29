using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Uploads.Command
{
    public class CreateUploadCommand
    {
        /// <summary>
        /// The upload file
        /// </summary>
        [Required (ErrorMessage = "Please upload file")]
        public IFormFile? UploadFile { get; set; }
        /// <summary>
        /// The userId of the person uploading the file
        /// </summary>
        [Required (ErrorMessage = "Please input CreatedBy")]
        public string? CreatedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

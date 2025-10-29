using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Uploads.Command
{
    public class UpdateMultipleUploadCommand
    {
        [Required (ErrorMessage = "Upload valid files")]
        /// <summary>
        /// The upload files
        /// </summary>
        public List<IFormFile> UploadFiles { get; set; }
        /// <summary>
        /// The userId of the person uploading the file
        /// </summary>
        [Required (ErrorMessage = "Input UserPublicId")]
        public string LastModifiedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

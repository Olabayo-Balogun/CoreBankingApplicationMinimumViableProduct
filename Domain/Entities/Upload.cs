using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
	public class Upload : AuditableEntity
	{
		/// <summary>
		/// The publicId is a unique GUID that's used to point directly to the upload on the DB
		/// </summary>
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }

		/// <summary>
		/// The file path of the content that can be used to display the content on the frontend
		/// </summary>
		[Required (ErrorMessage = "File path is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string FilePath { get; set; }
		/// <summary>
		/// The actual file path that can be used to delete the upload
		/// </summary>
		[Required (ErrorMessage = "Root file path is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string RootFilePath { get; set; }
		/// <summary>
		/// The file size of the uploaded content
		/// </summary>
		[Precision (18, 2)]
		[Required (ErrorMessage = "File size is required")]
		public decimal FileSize { get; set; }
		/// <summary>
		/// The file format of the upload
		/// </summary>
		[Required (ErrorMessage = "File format is required")]
		[StringLength (50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string FileFormat { get; set; }
	}
}

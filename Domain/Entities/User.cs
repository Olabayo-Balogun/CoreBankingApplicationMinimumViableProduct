using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
	public class User : IdentityUser<long>
	{
		[Key]
		public long Id { get; set; }

		/// <summary>
		/// A GUID converted to string which should be used serve as the public ID of the user.
		/// </summary>
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }

		/// <summary>
		/// The first name of the recipient if the recipient is an individual, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? FirstName { get; set; }

		/// <summary>
		/// The middle name of the recipient if the recipient is an individual, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? MiddleName { get; set; }

		/// <summary>
		/// The last name of the recipient if the recipient is an individual, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? LastName { get; set; }

		/// <summary>
		/// This is used to identify if a user is simply a group or team within an organization
		/// </summary>
		[StringLength (200, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? GroupName { get; set; }

		[EmailAddress (ErrorMessage = "Email address is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Email { get; set; }

		/// <summary>
		/// This should help us tell the age of the individual or business
		/// </summary>
		public DateTime? DateOfBirth { get; set; }

		/// <summary>
		/// This tells us the type of entity, eg, cooperative, SME, PLC, etc, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? BusinessType { get; set; }

		/// <summary>
		/// This should helps with information of their unique identification detail
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? IdentificationId { get; set; }

		/// <summary>
		/// This should helps with information of their identification type eg CAC number, NIN, Driver's license, etc.
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? IdentificationType { get; set; }

		/// <summary>
		/// Upload file path of proof of their identification
		/// </summary>
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? ProofOfIdentification { get; set; }

		/// <summary>
		/// This should helps with information regarding whether the user is an individual or not
		/// </summary>
		[Required (ErrorMessage = "IsIndividual is required")]
		public bool IsIndividual { get; set; }

		/// <summary>
		/// This should helps with information regarding whether the user is a staff of a registered business on our platform
		/// </summary>
		public bool IsStaff { get; set; } = false;

		/// <summary>
		/// This should helps with information regarding whether the user's company account is suspended
		/// </summary>
		public bool IsAccountSuspended { get; set; } = false;

		/// <summary>
		/// This should helps with information of their BVN for reconciliation purposes if they want to see any information of their lending history on the platform, it's not compulsory though
		/// </summary>
		[StringLength (20, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Bvn { get; set; }

		/// <summary>
		/// This should helps with information of their Nin for reconciliation purposes if they want to see any information of their lending history on the platform, it's not compulsory though
		/// </summary>
		[StringLength (20, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Nin { get; set; }

		/// <summary>
		/// This should helps with information regarding whether the user is a verified user or not, as to if we have verified their means of identification
		/// </summary>
		[Required (ErrorMessage = "IsVerified is required")]
		public bool IsVerified { get; set; }

		/// <summary>
		/// This should helps with information on the role of the user eg, Admin, Staff, or just a user
		/// </summary>
		[Required (ErrorMessage = "UserRole is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string UserRole { get; set; }

		/// <summary>
		/// This should help us know the date they verified their email, it's not compulsory though
		/// </summary>
		public DateTime? EmailVerifiedDate { get; set; }

		/// <summary>
		/// This should help us know the last time they logged in, we should update it as at the time they log in, because we can't be so sure they'll click logout, when leaving the website, it's not compulsory though
		/// </summary>
		public DateTime? LastLoggedInDate { get; set; }

		/// <summary>
		/// This should file path of the profile image upload, it's not compulsory though
		/// </summary>
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? ProfileImage { get; set; }
		/// <summary>
		/// This should helps with information on country of residence of the user just for regulatory purposes
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? CountryOfResidence { get; set; }

		/// <summary>
		/// This should helps with information on the nationality of the user just for regulatory purposes
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? CountryOfOrigin { get; set; }
		/// <summary>
		/// This should helps with information on the origin state within the nationality of the user just for regulatory purposes
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? StateOfOrigin { get; set; }

		/// <summary>
		/// This should helps with information on the state of residence of the user just for regulatory purposes
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? StateOfResidence { get; set; }

		/// <summary>
		/// This should contain the business of the user, it's not compulsory though
		/// </summary>
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? BusinessName { get; set; }

		/// <summary>
		/// Phone number details of the user, it's not compulsory though, it's not compulsory though
		/// </summary>
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? PhoneNumber { get; set; }

		[Required (ErrorMessage = "Password cannot be empty")]
		[DataType (DataType.Password)]
		[StringLength (100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		[Display (Name = "Password")]
		[RegularExpression ("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
		[JsonIgnore]
		public string Password { get; set; }

		/// <summary>
		/// This should contain information on if the user has been deleted or not
		/// </summary>
		[Required (ErrorMessage = "IsDeleted is required")]
		public bool IsDeleted { get; set; }

		/// <summary>
		/// This should contain information on the date the user was created
		/// </summary>
		public DateTime DateCreated { get; set; }

		/// <summary>
		/// This should contain information on the last date the user's information was modified, it's not compulsory though
		/// </summary>
		public DateTime? LastModifiedDate { get; set; }

		/// <summary>
		/// This should helps with information on the public GUID of the user who last modified this user's information, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? LastModifiedBy { get; set; }

		/// <summary>
		/// This should helps with information on the date the user was deleted, if the user is ever deleted, it's not compulsory though
		/// </summary>
		public DateTime? DateDeleted { get; set; }

		/// <summary>
		/// This should helps with information on the public GUID of the user who deleted this user's information, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? DeletedBy { get; set; }

		/// <summary>
		/// This should helps with information on the public GUID of the staff who created the user, it's not compulsory though
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? CreatedBy { get; set; }

		/// <summary>
		/// This is the token that the user will use to verify their sign up email
		/// </summary>
		public string? EmailVerificationToken { get; set; }

		public Guid? PasswordResetToken { get; set; }
	}
}


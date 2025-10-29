using Application.Models.Users.Response;

using MediatR;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Users.Command
{
    public class RegistrationCommand : IRequest<RequestResponse<UserResponse>>
    {
        /// <summary>
        /// The first name of the recipient if the recipient is an individual, it's not compulsory though
        /// </summary>
        [RegularExpression ("^(?=.*[a-z])[A-Za-z]{3,}$", ErrorMessage = "Must contain only lowercase and/or uppercase letters")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }
        /// <summary>
        /// The middle name of the recipient if the recipient is an individual, it's not compulsory though
        /// </summary>
        [RegularExpression ("^(?=.*[a-z])[A-Za-z]{3,}$", ErrorMessage = "Must contain only lowercase and/or uppercase letters")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? MiddleName { get; set; }
        /// <summary>
        /// The last name of the recipient if the recipient is an individual, it's not compulsory though
        /// </summary>
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        [RegularExpression ("^(?=.*[a-z])[A-Za-z]{3,}$", ErrorMessage = "Must contain only lowercase and/or uppercase letters")]
        public string? LastName { get; set; }
        /// <summary>
        /// Email address of the recipient, this is really important as we will be sending them reminder emails if they default
        /// </summary>
        [EmailAddress (ErrorMessage = "Email address is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Email { get; set; }
        /// <summary>
        /// This should help us tell the age of the individual or business
        /// </summary>
        [Required (ErrorMessage = "DateOfBirth is required")]
        public DateTime DateOfBirth { get; set; }
        /// <summary>
        /// This tells us the type of entity, eg, cooperative, SME, PLC, etc, it's not compulsory though
        /// </summary>
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? BusinessType { get; set; }
        /// <summary>
        /// This should helps with information of their unique identification detail
        /// </summary>
        [Required (ErrorMessage = "IdentificationId is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string IdentificationId { get; set; }
        /// <summary>
        /// This should helps with information of their identification type eg CAC number, NIN, Driver's license, etc.
        /// </summary>
        [Required (ErrorMessage = "IdentificationType is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string IdentificationType { get; set; }
        /// <summary>
        /// This should helps with information regarding whether the user is an individual or not
        /// </summary>
        [Required (ErrorMessage = "IsIndividual is required")]
        public bool IsIndividual { get; set; }
        /// <summary>
        /// This should helps with information regarding whether the user is a staff of a registered business on our platform
        /// </summary>
        [Required (ErrorMessage = "IsStaff is required")]
        public bool? IsStaff { get; set; }
        /// <summary>
        /// This should helps with information of their BVN for reconciliation purposes if they want to see any information of their lending history on the platform, it's not compulsory though
        /// </summary>
        [StringLength (20, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Bvn { get; set; }
        /// <summary>
        /// This should file path of the profile image upload, it's not compulsory though
        /// </summary>
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? ProfileImage { get; set; }
        /// <summary>
        /// This should helps with information on Country of residence of the user just for regulatory purposes
        /// </summary>
        [Required (ErrorMessage = "CountryOfResidence is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string CountryOfResidence { get; set; }
        /// <summary>
        /// This should helps with information on the nationality of the user just for regulatory purposes
        /// </summary>
        [Required (ErrorMessage = "CountryOfOrigin is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string CountryOfOrigin { get; set; }
        /// <summary>
        /// This should helps with information on the origin state within the nationality of the user just for regulatory purposes
        /// </summary>
        [Required (ErrorMessage = "StateOfOrigin is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string StateOfOrigin { get; set; }
        /// <summary>
        /// This should helps with information on the state of residence of the user just for regulatory purposes
        /// </summary>
        [Required (ErrorMessage = "StateOfResidence is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string StateOfResidence { get; set; }
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
        public string Password { get; set; }
        /// <summary>
        /// The password inputted again so we can confirm that the person didn't input the first one by mistake
        /// </summary>
        [Compare ("Password")]
        public string ConfirmPassword { get; set; }
        /// <summary>
        /// This should helps with information on the public GUID of the user who referred this user to us, it helps us in doing a referral program that rewards those who help us onboard new users, it's not compulsory though
        /// </summary>
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// This is used to identify if a user is simply a group or team within an organization
        /// </summary>
        [StringLength (200, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? GroupName { get; set; }

        /// <summary>
        /// Upload file path of proof of their identification
        /// </summary>
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? ProofOfIdentification { get; set; }
        /// <summary>
        /// This should helps with information of their Nin for reconciliation purposes if they want to see any information of their lending history on the platform, it's not compulsory though
        /// </summary>
        [StringLength (20, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Nin { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

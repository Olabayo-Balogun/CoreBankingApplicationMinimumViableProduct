namespace Application.Models
{
	public class ValidateTokenResponse : ValidationResponse
	{
		public string? UserId { get; set; }
		public string? BusinessName { get; set; }
		public string? UserRole { get; set; }
		public string? Email { get; set; }
		public string? Country { get; set; }
	}
}

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Application.Models;

using Microsoft.AspNetCore.Http;

namespace Application.Utility
{
	public static class Utility
	{
		public static AppSettings _appSettings { get; set; }

		public static void Initialize (AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		public static ValidationResponse IsExceedingPostPayloadLimit (int payloadCount)
		{
			ValidationResponse response = new ();
			if (payloadCount > _appSettings.PostPayloadLimit)
			{
				response.IsValid = false;
				response.Remark = $"Payload size exceeded {_appSettings.PostPayloadLimit} objects";
			}
			else if (payloadCount < 1)
			{
				response.IsValid = false;
				response.Remark = $"Upload valid objects";
			}
			else
			{
				response.IsValid = true;
				response.Remark = "Ok to process";
			}

			return response;
		}

		public static string GenerateRandomNumbers (int size)
		{
			var random = new Random ();
			var sb = new StringBuilder ();

			for (int i = 0; i < size; i++)
			{
				sb.Append (random.Next (1, 10)); // Generates a random number between 1 and 9
			}

			return sb.ToString ();
		}

		public static string EncryptString (string plainText)
		{
			try
			{
				var plainTextBytes = Encoding.UTF8.GetBytes (plainText);
				string encryptedString = Convert.ToBase64String (plainTextBytes);

				return encryptedString;
			}
			catch (Exception)
			{
				throw;
			}

		}

		public static string DecryptString (string encryptedString)
		{
			try
			{
				var base64EncodedBytes2 = Convert.FromBase64String (encryptedString);
				var decryptedString = Encoding.UTF8.GetString (base64EncodedBytes2);


				return decryptedString;
			}
			catch (Exception)
			{
				throw;
			}

		}

		public static string ToSentenceCase (string input)
		{
			if (string.IsNullOrWhiteSpace (input))
			{
				return string.Empty;
			}

			input = input.Trim ();

			// If input contains a hyphen, capitalize the first letter of each hyphen-separated part
			if (input.Contains ('-'))
			{
				var parts = input.Split ('-');
				for (int i = 0; i < parts.Length; i++)
				{
					if (!string.IsNullOrWhiteSpace (parts[i]))
					{
						parts[i] = char.ToUpper (parts[i][0]) + parts[i][1..].ToLower ();
					}
				}
				return string.Join ("-", parts);
			}
			else
			{
				// Standard sentence case: capitalize first letter, rest lower
				input = input.ToLower ();
				return char.ToUpper (input[0]) + input[1..];
			}
		}

		public static string GetMimeTypeFromBase64 (string base64String)
		{
			byte[] bytes = Convert.FromBase64String (base64String);

			if (bytes.Length >= 4)
			{
				if (bytes[0] == 0xFF && bytes[1] == 0xD8)
				{
					return "image/jpeg";
				}
				if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
				{
					return "image/png";
				}
				if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
				{
					return "application/pdf";
				}
				// Add more checks as necessary
			}

			return "application/octet-stream"; // Default to binary data

		}

		public static bool ComparePlainStringAndEncryptedString (string plainStringToBeCompared, string encryptedValueToBeComparedTo)
		{
			try
			{
				string decryptedValue = DecryptString (encryptedValueToBeComparedTo);

				return plainStringToBeCompared.Equals (decryptedValue, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				throw;
			}

		}

		public static string? GetClaimValueFromJwtToken (string token, string claimType)
		{
			var handler = new JwtSecurityTokenHandler ();
			var jwtToken = handler.ReadToken (token) as JwtSecurityToken;

			if (jwtToken == null)
			{
				throw new ArgumentException ("Invalid JWT token");
			}

			var claimValue = jwtToken.Claims.FirstOrDefault (claim => claim.Type.Contains (claimType));

			return claimValue?.Value;
		}

		public static string ConvertToBase64 (IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return null;
			}

			using var memoryStream = new MemoryStream ();
			file.CopyToAsync (memoryStream);
			byte[] fileBytes = memoryStream.ToArray ();
			return Convert.ToBase64String (fileBytes);
		}

		public static bool IsValidEmail (string email)
		{
			// Define the regex pattern for a valid email address
			string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			Regex regex = new (emailPattern);

			// Check if the email matches the pattern
			return regex.IsMatch (email);
		}

		public static ValidateTokenResponse ValidateToken (string? token)
		{
			var response = new ValidateTokenResponse ();
			if (token == null)
			{
				response.IsValid = false;
				response.Remark = "Unable to authenticate user";
				return response;
			}

			var userId = GetClaimValueFromJwtToken (token, "primarysid");
			if (userId == null)
			{
				response.IsValid = false;
				response.Remark = "Unable to authenticate user";
				return response;
			}

			var role = GetClaimValueFromJwtToken (token, "role");
			if (role == null)
			{
				response.IsValid = false;
				response.Remark = "Unable to verify user role";
				return response;
			}

			var country = GetClaimValueFromJwtToken (token, "country");
			if (country == null)
			{
				response.IsValid = false;
				response.Remark = "Unable to verify user's country";
				return response;
			}

			var businessName = GetClaimValueFromJwtToken (token, "name");

			if (businessName == null)
			{
				response.IsValid = false;
				response.Remark = "Update your name/business name in your user profile";
				return response;
			}

			var email = GetClaimValueFromJwtToken (token, "emailaddress");

			if (email == null)
			{
				response.IsValid = false;
				response.Remark = "Unable to verify user email";
				return response;
			}

			response.IsValid = true;
			response.UserId = userId;
			response.UserRole = role;
			response.Country = country;
			response.Email = email;
			response.BusinessName = businessName;
			return response;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameter (string query, int? characterLimit)
		{
			try
			{
				query = query.Trim ();
				var result = new ValidateQueryParameterAndPaginationResponse ();
				if (string.IsNullOrEmpty (query))
				{
					result.IsValid = false;
					result.Remark = "Please input a query parameter with more than 3 characters";

					return result;
				}

				if (query.Length < 3)
				{
					result.IsValid = false;
					result.Remark = "Please input a query parameter with more than 2 characters";

					return result;
				}

				if (characterLimit == null)
				{
					if (query.Length > _appSettings.QueryCharacterLimit)
					{
						result.IsValid = false;
						result.Remark = $"Please input a query parameter with less than {_appSettings.QueryCharacterLimit + 1} characters";

						return result;
					}
				}
				else
				{
					if (query.Length > characterLimit)
					{
						result.IsValid = false;
						result.Remark = $"Please input a query parameter with less than {characterLimit + 1} characters";

						return result;
					}
				}
				string decodedName = HttpUtility.UrlDecode (query);

				result.IsValid = true;
				result.Remark = "Valid";
				result.DecodedString = decodedName;

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static ValidationResponse ValidatePagination (int pageNumber, int pageSize)
		{
			var response = new ValidationResponse ();
			if (pageNumber < 0)
			{
				response.IsValid = false;
				response.Remark = $"Please input a page number equal to or greater than 0";
				return response;
			}

			if (pageSize < 1)
			{
				response.IsValid = false;
				response.Remark = $"Please input a page size greater than 0 and less than {_appSettings.PageSizeLimit + 1}";
				return response;
			}

			if (pageSize > _appSettings.PageSizeLimit)
			{
				response.IsValid = false;
				response.Remark = $"Page size limit exceeded, please input a page size greater than 0 and less than {_appSettings.PageSizeLimit + 1}";
				return response;
			}

			response.IsValid = true;
			response.Remark = "Valid";
			return response;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameterAndPagination (string query, int? characterLimit, int pageNumber, int pageSize)
		{
			ValidateQueryParameterAndPaginationResponse result = new ();
			ValidateQueryParameterAndPaginationResponse queryResponse = ValidateQueryParameter (query, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidationResponse paginationResponse = ValidatePagination (pageNumber, pageSize);
			if (!paginationResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}


			result.IsValid = true;
			result.Remark = "Valid";
			result.DecodedString = queryResponse.DecodedString;

			return result;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameter (string query, string query2, int? characterLimit)
		{
			ValidateQueryParameterAndPaginationResponse result = new ();
			ValidateQueryParameterAndPaginationResponse queryResponse = ValidateQueryParameter (query, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query2Response = ValidateQueryParameter (query2, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}


			result.IsValid = true;
			result.Remark = "Valid";
			result.DecodedString = queryResponse.DecodedString;
			result.DecodedString2 = query2Response.DecodedString;

			return result;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameter (string query, string query2, string query3, int? characterLimit)
		{
			ValidateQueryParameterAndPaginationResponse result = new ();
			ValidateQueryParameterAndPaginationResponse queryResponse = ValidateQueryParameter (query, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query2Response = ValidateQueryParameter (query2, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query3Response = ValidateQueryParameter (query3, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			result.IsValid = true;
			result.Remark = "Valid";
			result.DecodedString = queryResponse.DecodedString;
			result.DecodedString2 = query2Response.DecodedString;
			result.DecodedString3 = query3Response.DecodedString;

			return result;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameterAndPagination (string query, string query2, int? characterLimit, int pageNumber, int pageSize)
		{
			ValidateQueryParameterAndPaginationResponse result = new ();
			ValidateQueryParameterAndPaginationResponse queryResponse = ValidateQueryParameter (query, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query2Response = ValidateQueryParameter (query2, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidationResponse paginationResponse = ValidatePagination (pageNumber, pageSize);
			if (!paginationResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			result.IsValid = true;
			result.Remark = "Valid";
			result.DecodedString = queryResponse.DecodedString;
			result.DecodedString2 = query2Response.DecodedString;

			return result;
		}

		public static ValidateQueryParameterAndPaginationResponse ValidateQueryParameterAndPagination (string query, string query2, string query3, int? characterLimit, int pageNumber, int pageSize)
		{
			ValidateQueryParameterAndPaginationResponse result = new ();
			ValidateQueryParameterAndPaginationResponse queryResponse = ValidateQueryParameter (query, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query2Response = ValidateQueryParameter (query2, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			ValidateQueryParameterAndPaginationResponse query3Response = ValidateQueryParameter (query3, characterLimit);
			if (!queryResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}


			ValidationResponse paginationResponse = ValidatePagination (pageNumber, pageSize);
			if (!paginationResponse.IsValid)
			{
				result.IsValid = false;
				result.Remark = queryResponse.Remark;

				return result;
			}

			result.IsValid = true;
			result.Remark = "Valid";
			result.DecodedString = queryResponse.DecodedString;
			result.DecodedString2 = query2Response.DecodedString;
			result.DecodedString3 = query3Response.DecodedString;

			return result;
		}

		public static string GenerateNUBAN (string bankCode, string serialNumber)
		{
			if (serialNumber.Length != 6)
			{
				throw new ArgumentException ("Serial number must be 6 digits.");
			}

			string baseNumber = bankCode + serialNumber;
			int[] weights = { 3, 7, 3, 3, 7, 3, 3, 7, 3 };
			int sum = 0;

			for (int i = 0; i < 9; i++)
			{
				int digit = int.Parse (baseNumber[i].ToString ());
				sum += digit * weights[i];
			}

			int checkDigit = (10 - (sum % 10)) % 10;
			return bankCode + serialNumber + checkDigit.ToString ();
		}

		public static string GenerateLedgerNumber (string branchCode, string customerNumber, string accountType, string subledgerNumber)
		{

			return $"{branchCode}/{accountType}/{customerNumber}/{subledgerNumber}";
		}

		public static string PadToSixDigits (int number)
		{
			return number < 0 || number > 999999
				? throw new ArgumentOutOfRangeException ("Number must be between 0 and 999999.")
				: number.ToString ("D6");
		}

		public static string PadToThreeDigits (int number)
		{
			return number < 0 || number > 999
				? throw new ArgumentOutOfRangeException ("Number must be between 0 and 999.")
				: number.ToString ("D3");
		}

		public static string PadToTwoDigits (int number)
		{
			return number < 0 || number > 99
				? throw new ArgumentOutOfRangeException ("Number must be between 0 and 99.")
				: number.ToString ("D2");
		}

		public static string GenerateMethodInitiationLog (string methodName)
		{
			string result = string.Empty;
			result = $"{methodName} begins at {DateTime.UtcNow.AddHours (1)}";

			return result;
		}

		public static string GenerateMethodInitiationLog (string methodName, string firstParameterName, string firstParameterValue)
		{
			string result = string.Empty;
			result = $"{methodName} begins at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue}";

			return result;
		}

		public static string GenerateMethodInitiationLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue)
		{
			string result = string.Empty;
			result = $"{methodName} begins at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue} and {secondParameterName}: {secondParameterValue}";

			return result;
		}

		public static string GenerateMethodInitiationLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue, string thirdParameterName, string thirdParameterValue)
		{
			string result = string.Empty;
			result = $"{methodName} begins at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue}, {secondParameterName}: {secondParameterValue}, and {thirdParameterName}: {thirdParameterValue}";

			return result;
		}

		public static string GenerateMethodConclusionLog (string methodName)
		{
			string result = string.Empty;
			result = $"{methodName} ends at {DateTime.UtcNow.AddHours (1)}";

			return result;
		}

		public static string GenerateMethodConclusionLog (string methodName, string remark)
		{
			string result = string.Empty;
			result = $"{methodName} ends at {DateTime.UtcNow.AddHours (1)} with remark: {remark}";

			return result;
		}

		public static string GenerateMethodConclusionLog (string methodName, string firstParameterName, string firstParameterValue, string remark)
		{
			string result = string.Empty;
			result = $"{methodName} ends at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue} with remark: {remark}";

			return result;
		}

		public static string GenerateMethodConclusionLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue, string remark)
		{
			string result = string.Empty;
			result = $"{methodName} ends at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue} and {secondParameterName}: {secondParameterValue} with remark: {remark}";

			return result;
		}

		public static string GenerateMethodConclusionLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue, string thirdParameterName, string thirdParameterValue, string remark)
		{
			string result = string.Empty;
			result = $"{methodName} ends at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue}, {secondParameterName}: {secondParameterValue}, and {thirdParameterName}: {thirdParameterValue} with remark: {remark}";

			return result;
		}

		public static string GenerateMethodExceptionLog (string methodName, string message)
		{
			string result = string.Empty;
			result = $"{methodName} exception occurred at {DateTime.UtcNow.AddHours (1)} with message: {message}";

			return result;
		}

		public static string GenerateMethodExceptionLog (string methodName, string firstParameterName, string firstParameterValue, string message)
		{
			string result = string.Empty;
			result = $"{methodName} exception occurred at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue} with message: {message}";

			return result;
		}

		public static string GenerateMethodExceptionLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue, string message)
		{
			string result = string.Empty;
			result = $"{methodName} exception occurred at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue} and {secondParameterName}: {secondParameterValue} with message: {message}";

			return result;
		}

		public static string GenerateMethodExceptionLog (string methodName, string firstParameterName, string firstParameterValue, string secondParameterName, string secondParameterValue, string thirdParameterName, string thirdParameterValue, string message)
		{
			string result = string.Empty;
			result = $"{methodName} exception occurred at {DateTime.UtcNow.AddHours (1)} for {firstParameterName}: {firstParameterValue}, {secondParameterName}: {secondParameterValue}, and {thirdParameterName}: {thirdParameterValue} with message: {message}";

			return result;
		}
	}
}

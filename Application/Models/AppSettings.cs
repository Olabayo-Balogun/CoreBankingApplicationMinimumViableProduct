namespace Application.Model
{
	public class AppSettings
	{
		public int PostPayloadLimit { get; set; }
		public int PageSizeLimit { get; set; }
		public int QueryCharacterLimit { get; set; }
		public int EmailBatchSizeLimit { get; set; }

		//Authentication And Authorization
		public string Secret { get; set; }
		public string ValidAudience { get; set; }
		public string ValidIssuer { get; set; }

		//Cloudinary
		public string CloudinaryUrl { get; set; }

		//File storage
		public long MaxFileSizeInBytes { get; set; }
		public string BaseUrl { get; set; }
		public string AcceptableFileFormats { get; set; }
		public bool IsProduction { get; set; }
		public bool IsSavingFilesToCloudStorage { get; set; }
		public bool IsSavingFilesToLocalStorage { get; set; }
		public string BaseStoragePath { get; set; }

		//SMTP
		public string SmtpHost { get; set; }
		public string SmtpPassword { get; set; }
		public string SmtpUser { get; set; }
		public int SmtpPort { get; set; }
		public string SenderName { get; set; }
		public string EmailSender { get; set; }
		public bool EnableEmailSsl { get; set; }

		//Smile ID
		public string SmileIdBusinessVerificationEndpoint { get; set; }
		public string SmileIdNinVerificationEndpoint { get; set; }
		public string SmileIdSandboxBaseUrl { get; set; }
		public string SmileIdProductionBaseUrl { get; set; }
		public string SmileIdPartnerId { get; set; }
		public string SmileIdSandboxApiKey { get; set; }
		public string SmileIdProductionApiKey { get; set; }
		public string SmileIdIdTypeForNinSearchUsingNin { get; set; }
		public string SmileIdIdTypeForBvnSearchUsingBvn { get; set; }
		public int SmileIdBusinessEnquiryJobType { get; set; }
		public int SmileIdNinSearchJobType { get; set; }
		public int SmileIdBvnSearchJobType { get; set; }
		public bool IsSmileIdProductionEnvironment { get; set; }
		public decimal SmileIdNinSearchPrice { get; set; }
		public decimal SmileIdBvnSearchPrice { get; set; }
		public decimal SmileIdBusinessEnquiryPrice { get; set; }
		public bool ValidateIndividualRecipient { get; set; }
		public bool ValidateBusinessRecipient { get; set; }


		//Paystack
		public string PaystackTransferEndpoint { get; set; }
		public string PaystackVerificationEndpoint { get; set; }
		public string PaystackBaseUrl { get; set; }
		public bool IsPaystackProductionEnvironment { get; set; }
		public string PaystackProductionSecretKey { get; set; }
		public string PaystackTestSecretKey { get; set; }
		public string PaystackProductionPublicKey { get; set; }
		public string PaystackTestPublicKey { get; set; }
		public string WhitelistedIPAddresses { get; set; }


		//Rate Limiting
		public int GetRequestRateLimit { get; set; }
		public int GetRequestTimeSpanInSeconds { get; set; }
		public int GetRequestQueueLimit { get; set; }
		public int PostRequestRateLimit { get; set; }
		public int PostRequestTimeSpanInSeconds { get; set; }
		public int PostRequestQueueLimit { get; set; }
	}
}

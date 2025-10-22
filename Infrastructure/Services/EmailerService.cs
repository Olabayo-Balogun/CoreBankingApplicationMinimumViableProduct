using System.Net.Mail;

using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Models;
using Application.Models.EmailLogs.Command;
using Application.Utility;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
	public class EmailerService : IEmailerService
	{
		private readonly ILogger<IEmailerService> _logger;
		private readonly IEmailLogRepository _emailLogRepository;
		private readonly AppSettings _appSettings;

		public EmailerService (ILogger<IEmailerService> logger, IEmailLogRepository emailLogRepository, IOptions<AppSettings> appSettings)
		{
			_logger = logger;
			_emailLogRepository = emailLogRepository;
			_appSettings = appSettings.Value;
		}

		public async Task SendUnsentEmailsAsync ()
		{
			try
			{
				_logger.LogInformation ($"SendUnsentEmails begins at {DateTimeOffset.Now}");
				CancellationToken cancellationToken = new ();
				var emailLogs = await _emailLogRepository.GetEmailLogBySentStatusAsync (false, cancellationToken, 1, _appSettings.EmailBatchSizeLimit);
				List<UpdateEmailLogSentStatusCommand> updateEmails = [];

				if (emailLogs.IsSuccessful == true && emailLogs.Data != null)
				{
					_logger.LogInformation ($"SendUnsentEmails retrieved {emailLogs.TotalCount} email logs at {DateTimeOffset.Now}");
					foreach (var email in emailLogs.Data)
					{
						try
						{
							DeleteEmailLogCommand? deleteEmailLogRequestViewModel = new ();
							var isRecipientValidEmail = Utility.IsValidEmail (email.ToRecipient);
							if (isRecipientValidEmail == false)
							{
								deleteEmailLogRequestViewModel.UserId = "SYSTEM";
								deleteEmailLogRequestViewModel.CancellationToken = cancellationToken;
								deleteEmailLogRequestViewModel.Id = email.Id;

								var deleteEmail = await _emailLogRepository.DeleteEmailLogAsync (deleteEmailLogRequestViewModel);
							}

							if (email.CcRecipient != null && email.CcRecipient != "null")
							{
								var isValidCcEmail = Utility.IsValidEmail (email.CcRecipient);

								if (isValidCcEmail == false)
								{
									deleteEmailLogRequestViewModel.UserId = "SYSTEM";
									deleteEmailLogRequestViewModel.CancellationToken = cancellationToken;
									deleteEmailLogRequestViewModel.Id = email.Id;

									var deleteEmail = await _emailLogRepository.DeleteEmailLogAsync (deleteEmailLogRequestViewModel);
								}
							}

							if (email.BccRecipient != null && email.BccRecipient != "null")
							{
								var isValidBccEmail = Utility.IsValidEmail (email.BccRecipient);

								if (isValidBccEmail == false)
								{
									deleteEmailLogRequestViewModel.UserId = "SYSTEM";
									deleteEmailLogRequestViewModel.CancellationToken = cancellationToken;
									deleteEmailLogRequestViewModel.Id = email.Id;

									var deleteEmail = await _emailLogRepository.DeleteEmailLogAsync (deleteEmailLogRequestViewModel);
								}
							}

							if (deleteEmailLogRequestViewModel.Id == 0)
							{
								MailAddress senderDetails = new (email.Sender, _appSettings.SenderName);
								MailMessage message = new ()
								{
									Sender = senderDetails,
									Subject = _appSettings.IsProduction ? email.Subject : $"{email.Subject} Test Mode",
									From = senderDetails,
									IsBodyHtml = email.IsHtml,
									Body = email.Message
								};

								if (email.BccRecipient != null && email.BccRecipient != "null")
								{
									message.Bcc.Add (email.BccRecipient);
								}
								if (email.CcRecipient != null && email.CcRecipient != "null")
								{
									message.CC.Add (email.CcRecipient);
								}
								message.To.Add (email.ToRecipient);

								_logger.LogInformation ($"SendUnsentEmails call to SMTP server begins at {DateTimeOffset.Now} for email: {email.ToRecipient}");

								var client = new SmtpClient (_appSettings.SmtpHost, _appSettings.SmtpPort)
								{
									Credentials = new System.Net.NetworkCredential (_appSettings.SmtpUser, _appSettings.SmtpPassword),
									EnableSsl = _appSettings.EnableEmailSsl,
									DeliveryMethod = SmtpDeliveryMethod.Network,
									//Only needed if the SMTP server requires authentication
									UseDefaultCredentials = false
								};

								//await client.SendMailAsync (message);
								client.Send (message);

								client.Dispose ();
								_logger.LogInformation ($"SendUnsentEmails call to SMTP server ends at {DateTimeOffset.Now} for email: {email.ToRecipient}");

								_logger.LogInformation ($"SendUnsentEmails to update email sent status begins at {DateTimeOffset.Now} for email: {email.ToRecipient}");

								UpdateEmailLogSentStatusCommand updateEmail = new ()
								{
									CancellationToken = cancellationToken,
									LastModifiedBy = "SYSTEM",
									IsSent = true,
									DateSent = DateTime.UtcNow.AddHours (1),
									Id = email.Id
								};

								updateEmails.Add (updateEmail);
							}
							_logger.LogInformation ($"SendUnsentEmails to update email sent status ends at {DateTimeOffset.Now} for email: {email.ToRecipient}");
						}
						catch (Exception ex)
						{
							_logger.LogError ($"SendUnsentEmails for email: {email.ToRecipient} exception occurred at {DateTimeOffset.Now} with message: {ex.Message}");
							continue;
						}

					}

					var updateEmailLog = await _emailLogRepository.UpdateMultipleEmailLogSentStatusAsync (updateEmails);
					_logger.LogInformation ($"SendUnsentEmails ends at {DateTimeOffset.Now}");
				}
				else
				{
					_logger.LogInformation ($"SendUnsentEmails ends at {DateTimeOffset.Now} with no email logs retrieved");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError ($"SendUnsentEmails exception occurred at {DateTimeOffset.Now} with message: {ex.Message}");
				throw;
			}
		}
	}
}

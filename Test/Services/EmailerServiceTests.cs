using Application.Interface.Infrastructure;
using Application.Interface.Persistence;
using Application.Model;
using Application.Model.EmailLogs.Command;
using Application.Model.EmailLogs.Queries;
using Application.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using ThirdPartyIntegrations.Services;

namespace Test.Services
{
	public class EmailerServiceTests
	{
		private readonly Mock<IEmailLogRepository> _emailLogRepoMock;
		private readonly Mock<ILogger<IEmailerService>> _loggerMock;
		private readonly IEmailerService _emailerService;

		public EmailerServiceTests ()
		{
			_emailLogRepoMock = new Mock<IEmailLogRepository> ();
			_loggerMock = new Mock<ILogger<IEmailerService>> ();

			var appSettings = new AppSettings
			{
				EmailBatchSizeLimit = 10,
				SmtpHost = "smtp.test.com",
				SmtpPort = 587,
				SmtpUser = "testuser",
				SmtpPassword = "testpass",
				EnableEmailSsl = true,
				SenderName = "Test Sender",
				IsProduction = false
			};

			var options = Options.Create (appSettings);
			_emailerService = new EmailerService (_loggerMock.Object, _emailLogRepoMock.Object, options);
		}

		[Fact]
		public async Task SendUnsentEmailsAsync_SendsValidEmailAndUpdatesStatus ()
		{
			// Arrange
			var emailLog = new EmailLogResponse
			{
				Id = 1,
				ToRecipient = "valid@example.com",
				CcRecipient = null,
				BccRecipient = null,
				Sender = "sender@example.com",
				Subject = "Test Subject",
				Message = "Test Body",
				IsHtml = true
			};

			var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful ([emailLog], 1, "EmailLogs");

			_emailLogRepoMock
				.Setup (repo => repo.GetEmailLogBySentStatusAsync (false, It.IsAny<CancellationToken> (), 1, 10))
				.ReturnsAsync (response);

			_emailLogRepoMock
				.Setup (repo => repo.UpdateMultipleEmailLogSentStatusAsync (It.IsAny<List<UpdateEmailLogSentStatusCommand>> ()))
				.ReturnsAsync (RequestResponse<List<EmailLogResponse>>.Success (response.Data, 1, "Updated"));

			// Act
			await _emailerService.SendUnsentEmailsAsync ();

			// Assert
			_emailLogRepoMock.Verify (repo => repo.UpdateMultipleEmailLogSentStatusAsync (It.Is<List<UpdateEmailLogSentStatusCommand>> (list => list.Count == 1)), Times.Once);
			_loggerMock.Verify (log => log.LogInformation (It.IsAny<string> ()), Times.AtLeastOnce);
		}

		[Fact]
		public async Task SendUnsentEmailsAsync_DeletesInvalidEmail ()
		{
			// Arrange
			var emailLog = new EmailLogResponse
			{
				Id = 2,
				ToRecipient = "invalid-email",
				CcRecipient = null,
				BccRecipient = null,
				Sender = "sender@example.com",
				Subject = "Invalid Email",
				Message = "Body",
				IsHtml = false
			};

			var response = RequestResponse<List<EmailLogResponse>>.SearchSuccessful ([emailLog], 1, "EmailLogs");

			_emailLogRepoMock
				.Setup (repo => repo.GetEmailLogBySentStatusAsync (false, It.IsAny<CancellationToken> (), 1, 10))
				.ReturnsAsync (response);

			_emailLogRepoMock
				.Setup (repo => repo.DeleteEmailLogAsync (It.IsAny<DeleteEmailLogCommand> ()))
				.ReturnsAsync (RequestResponse<EmailLogResponse>.Success (emailLog, 1, "Deleted"));

			// Act
			await _emailerService.SendUnsentEmailsAsync ();

			// Assert
			_emailLogRepoMock.Verify (repo => repo.DeleteEmailLogAsync (It.Is<DeleteEmailLogCommand> (cmd => cmd.Id == 2)), Times.Once);
		}

	}
}

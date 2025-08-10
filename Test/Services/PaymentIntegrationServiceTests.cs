using Application.Interface.Persistence;
using Application.Model.PaymentIntegration.Command;
using Application.Models;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Persistence;

using ThirdPartyIntegrations.Models.ViewModels.APIViewModels.PaymentIntegration.Request;
using ThirdPartyIntegrations.Services;

namespace Test.Services
{
	public class PaymentIntegrationServiceTests
	{
		private readonly Mock<ILogger<PaymentIntegrationService>> _loggerMock;
		private readonly Mock<ITransactionRepository> _transactionRepoMock;
		private readonly ApplicationDbContext _context;
		private readonly PaymentIntegrationService _service;

		public PaymentIntegrationServiceTests ()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
				.UseInMemoryDatabase (Guid.NewGuid ().ToString ())
				.Options;

			_context = new ApplicationDbContext (options);
			_loggerMock = new Mock<ILogger<PaymentIntegrationService>> ();
			_transactionRepoMock = new Mock<ITransactionRepository> ();

			var appSettings = new AppSettings
			{
				PaystackBaseUrl = "https://api.paystack.co",
				PaystackTransferEndpoint = "/transfer",
				PaystackVerificationEndpoint = "/transaction/verify/{paymentReferenceId}",
				PaystackTestSecretKey = "test_key",
				PaystackProductionSecretKey = "prod_key",
				IsPaystackProductionEnvironment = false
			};

			var optionsWrapper = Options.Create (appSettings);
			_service = new PaymentIntegrationService (optionsWrapper, _context, _loggerMock.Object, _transactionRepoMock.Object);
		}

		[Fact]
		public async Task PaystackPaymentWebhookRequestAsync_ReturnsNotFound_WhenTransactionDoesNotExist ()
		{
			var webhookCommand = new PaystackWebhookCommand
			{
				Data = new PaystackWebhookDataCommand
				{
					offline_reference = "ref123",
					Amount = 5000,
					status = "success"
				}
			};

			var result = await _service.PaystackPaymentWebhookRequestAsync (webhookCommand);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payment", result.Remark);
			Assert.Equal (404, result.StatusCode);
		}

		[Fact]
		public async Task PaystackPaymentWebhookRequestAsync_ReturnsFailed_WhenAlreadyReconciled ()
		{
			_context.Transactions.Add (new Transaction
			{
				PaymentReferenceId = "ref123",
				IsReconciled = true
			});
			await _context.SaveChangesAsync ();

			var webhookCommand = new PaystackWebhookCommand
			{
				Data = new PaystackWebhookDataCommand
				{
					offline_reference = "ref123",
					Amount = 5000,
					status = "success"
				}
			};

			var result = await _service.PaystackPaymentWebhookRequestAsync (webhookCommand);

			Assert.False (result.IsSuccessful);
			Assert.Equal ("Payment already confirmed", result.Remark);
			Assert.Equal (200, result.StatusCode);
		}
	}
}

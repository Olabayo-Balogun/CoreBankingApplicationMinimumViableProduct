using Microsoft.Extensions.DependencyInjection;

using ThirdPartyIntegrations.Interface;
using ThirdPartyIntegrations.Services;

namespace ThirdPartyIntegrations
{
	public static class ThirdPartyIntegrationsServiceRegistration
	{
		public static IServiceCollection AddThirdPartyIntegrationServices (this IServiceCollection services)
		{
			services.AddScoped<IPaymentIntegrationService, PaymentIntegrationService> ();
			services.AddScoped<IEmailerService, EmailerService> ();

			return services;
		}
	}
}

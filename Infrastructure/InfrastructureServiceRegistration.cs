using Application.Interface.Infrastructure;

using Infrastructure.Services;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices (this IServiceCollection services)
        {
            services.AddMediatR (Assembly.GetExecutingAssembly ());
            //services.AddTransient (typeof (IPipelineBehavior<,>), typeof (ValidationBehaviour<,>));
            services.AddScoped<IEmailLogService, EmailLogService> ();
            services.AddScoped<IEmailRequestService, EmailRequestService> ();
            services.AddScoped<IEmailTemplateService, EmailTemplateService> ();
            services.AddScoped<IPaymentIntegrationService, PaymentIntegrationService> ();
            services.AddScoped<IEmailerService, EmailerService> ();
            return services;
        }
    }
}

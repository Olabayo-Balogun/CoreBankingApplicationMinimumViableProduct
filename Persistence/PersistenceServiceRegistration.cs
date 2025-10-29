using Application.Interface.Persistence;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Persistence.Repositories;

using System.Reflection;

namespace Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext> (options =>
                options.UseSqlServer (configuration.GetConnectionString ("DefaultConnection")));

            services.Configure<IdentityOptions> (options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            });

            services.AddAutoMapper (Assembly.GetExecutingAssembly ());
            services.AddMediatR (Assembly.GetExecutingAssembly ());

            services.AddScoped<IAccountRepository, AccountRepository> ();
            services.AddScoped<IAuditLogRepository, AuditLogRepository> ();
            services.AddScoped<IBankRepository, BankRepository> ();
            services.AddScoped<IBranchRepository, BranchRepository> ();
            services.AddScoped<IEmailLogRepository, EmailLogRepository> ();
            services.AddScoped<IEmailRequestRepository, EmailRequestRepository> ();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository> ();
            services.AddScoped<ITransactionRepository, TransactionRepository> ();
            services.AddScoped<IUploadRepository, UploadRepository> ();
            services.AddScoped<IUserRepository, UserRepository> ();
            return services;
        }
    }
}

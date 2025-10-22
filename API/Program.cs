using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

using API.Middleware;

using Application;
using Application.Models;
using Application.Profiles;

using AutoMapper;

using Hangfire;
using Hangfire.SqlServer;

using Infrastructure;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Persistence;

using Scalar.AspNetCore;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder (args);

builder.Services.AddHangfire (configuration => configuration
		.SetDataCompatibilityLevel (CompatibilityLevel.Version_180)
		.UseSimpleAssemblyNameTypeSerializer ()
		.UseRecommendedSerializerSettings ()
		.UseSqlServerStorage (builder.Configuration.GetConnectionString ("DefaultConnection"),
		new SqlServerStorageOptions
		{
			JobExpirationCheckInterval = TimeSpan.FromDays (2)
		}));

builder.Services.AddHangfireServer ();

builder.Configuration
	.AddJsonFile ("appsettings.json", optional: false, reloadOnChange: true)
	.SetBasePath (builder.Environment.ContentRootPath)
	.AddJsonFile ($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
	.AddEnvironmentVariables ();


AppSettings appSettings = new ();
builder.Configuration.GetSection (nameof (AppSettings)).Bind (appSettings);

Application.Utility.Utility.Initialize (appSettings);

Log.Logger = new LoggerConfiguration ()
	.MinimumLevel.Information ()
	.WriteTo.File ($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs\\CoreBankApplicationMinimumViableProduct.txt", rollingInterval: RollingInterval.Day,
	fileSizeLimitBytes: 500 * 1024 * 1024,
	retainedFileCountLimit: 7,
	rollOnFileSizeLimit: true)
	.CreateLogger ();

builder.Host.UseSerilog (Log.Logger);

// Add services to the container.
builder.Services.AddApplicationServices ();
builder.Services.AddInfrastructureServices ();
builder.Services.AddPersistenceServices (builder.Configuration);
builder.Services.Configure<AppSettings> (builder.Configuration.GetSection ("AppSettings"));

builder.Services.AddRateLimiter (x => x
	.AddFixedWindowLimiter (policyName: "GetRequestRateLimit", options =>
	{
		options.PermitLimit = appSettings.GetRequestRateLimit;
		options.Window = TimeSpan.FromSeconds (appSettings.GetRequestTimeSpanInSeconds);
		options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		options.QueueLimit = appSettings.GetRequestQueueLimit;
	})
	.AddFixedWindowLimiter (policyName: "PostRequestRateLimit", options =>
	{
		options.PermitLimit = appSettings.PostRequestRateLimit;
		options.Window = TimeSpan.FromSeconds (appSettings.PostRequestTimeSpanInSeconds);
		options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		options.QueueLimit = appSettings.PostRequestQueueLimit;
	})
	.AddFixedWindowLimiter (policyName: "StrictPostRequestRateLimit", options =>
	{
		options.PermitLimit = 1000;
		options.Window = TimeSpan.FromDays (1);
		options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		options.QueueLimit = 0;
	})
);

// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration (mc =>
{
	mc.AddProfile (new MappingProfiles ());
});

IMapper mapper = mapperConfig.CreateMapper ();
builder.Services.AddSingleton (mapper);


builder.Services.AddControllers (options =>
{
	options.CacheProfiles.Add ("DropdownData",
		new CacheProfile ()
		{
			Duration = 86400,
			VaryByQueryKeys = new[] { "name", "pageNumber", "pageSize" }

		});
});

builder.Services.AddControllersWithViews ();
builder.Services.AddHangfireServer ();

// For Entity Framework
builder.Services.AddDbContext<ApplicationDbContext> (options => options.UseSqlServer (builder.Configuration.GetConnectionString ("DefaultConnection"), b => b.MigrationsAssembly ("Persistence")));

// For Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole> ()
	.AddEntityFrameworkStores<ApplicationDbContext> ()
	.AddDefaultTokenProviders ();

// Adding Authentication
builder.Services.AddAuthentication (options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer (options =>
{
	options.SaveToken = true;
	options.RequireHttpsMetadata = false;
	options.TokenValidationParameters = new TokenValidationParameters ()
	{
		ValidateLifetime = true,
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidAudience = appSettings.ValidAudience,
		ValidIssuer = appSettings.ValidIssuer,
		IssuerSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (appSettings.Secret))
	};
});

builder.Services.AddApiVersioning (options =>
{
	options.DefaultApiVersion = new Asp.Versioning.ApiVersion (1, 0);
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ReportApiVersions = true;

})

.AddApiExplorer (options =>
{
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = true;
});

//Adding Response Caching Service
builder.Services.AddResponseCaching ();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();

builder.Services.AddSwaggerGen (options =>
{
	options.SwaggerDoc ("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Core Banking Application MVP", Version = "v1" });
	var securityScheme = new OpenApiSecurityScheme
	{
		Name = "Authentication",
		Description = "JWT Authorization header using the Bearer scheme",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		In = ParameterLocation.Header,
	};

	options.AddSecurityDefinition ("JWT", securityScheme);

	options.AddSecurityRequirement (new OpenApiSecurityRequirement
{
	{
		new OpenApiSecurityScheme
		{
			Reference = new OpenApiReference
			{
				Type = ReferenceType.SecurityScheme,
				Id = "JWT"
			}
		},
		new List<string>()
	}
});

	options.DescribeAllParametersInCamelCase ();

	options.IncludeXmlComments (Path.Combine (AppContext.BaseDirectory,
		$"{Assembly.GetExecutingAssembly ().GetName ().Name}.xml"));

});

builder.Services.AddCors (options =>
{
	options.AddPolicy ("AllowAllOrigins",
		builder =>
		{
			builder.AllowAnyOrigin ().AllowAnyHeader ().AllowAnyMethod ();
		});
});

var app = builder.Build ();

if (!appSettings.IsProduction)
{
	// Configure the HTTP request pipeline.
	app.UseSwagger (options =>
	{
		options.RouteTemplate = "/openapi/{documentName}.json";
	});
	app.UseSwaggerUI (options =>
	{
		options.DefaultModelsExpandDepth (-1);
		options.DefaultModelRendering (ModelRendering.Example);
		options.SwaggerEndpoint ("/openapi/v1.json", "API V1");
	});
	app.MapScalarApiReference (options =>
	{
		//options.WithModels (false);
		options.Title = "Core Banking Application MVP API";
		options.Theme = ScalarTheme.BluePlanet;
		options.DefaultHttpClient = new (ScalarTarget.Http, ScalarClient.Http11);
		options
	   .WithPreferredScheme ("Bearer") // Security scheme name from the OpenAPI document
	   .WithHttpBearerAuthentication (bearer =>
	   {
		   bearer.Token = "your-bearer-token";
	   });
	});
}

app.UseForwardedHeaders (new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<JwtTokenMiddleware> ();
app.UseHttpsRedirection ();
app.UseRouting ();
app.UseRateLimiter ();

app.MapControllerRoute (
	name: "default",
	pattern: "api/v{version:apiVersion}/{controller}/{action=Index}/{id?}");

app.MapFallbackToFile ("index.html"); ;

app.UseCors ("AllowAllOrigins");

// Authentication & Authorization
app.UseAuthentication ();
app.UseAuthorization ();

//Adding Response Caching Middleware Components
app.UseResponseCaching ();

app.MapControllers ();

app.UseStaticFiles ();

if (!app.Environment.IsDevelopment ())
{
	// Specify a custom file storage location
	var fileProvider = new PhysicalFileProvider (appSettings.BaseStoragePath);
	app.UseStaticFiles (new StaticFileOptions { FileProvider = fileProvider, RequestPath = "/files" });
}

app.UseFileServer ();

app.Run ();

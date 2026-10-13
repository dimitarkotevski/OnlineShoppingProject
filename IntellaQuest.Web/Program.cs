using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using IntellaQeust.BusinessLogic.Security;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Data.NHibernate.Repositories;
using IntellaQuest.Repository.Repositories;
using IntellaQuest.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration / secrets (kept out of source control)
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("ShopDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ShopDb' is not configured. Set it in appsettings.json or the " +
        "ConnectionStrings__ShopDb environment variable.");
}

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
{
    if (builder.Environment.IsDevelopment())
    {
        // Ephemeral development key: tokens are invalidated on restart, which is acceptable
        // locally and keeps a real signing secret out of source control.
        jwtSettings.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        Console.WriteLine(
            "[WARN] Jwt:Key is not configured. Generated an ephemeral development key; " +
            "configure Jwt:Key (user secrets or the Jwt__Key environment variable) for stable tokens.");
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:Key must be configured (at least 32 characters) outside of Development.");
    }
}

if (jwtSettings.ExpiryDays <= 0)
{
    jwtSettings.ExpiryDays = 1;
}

// Restricted CORS: only the configured origins may call the API.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[]
    {
        "http://localhost:4200", "https://localhost:4200",
        "http://localhost:5080", "https://localhost:5080"
    };
}

// ---------------------------------------------------------------------------
// Dependency injection (Autofac, mirroring the old Global.asax registrations)
// ---------------------------------------------------------------------------
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterInstance(jwtSettings).AsSelf().SingleInstance();

    // Build the (expensive) NHibernate session factory once for the whole application.
    container.RegisterInstance(new SessionFactoryProvider(connectionString))
        .As<ISessionFactoryProvider>()
        .SingleInstance();

    container.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

    container.RegisterAssemblyTypes(typeof(IUserRepository).Assembly)
        .Where(t => t.FullName.EndsWith("Repository"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();

    container.RegisterAssemblyTypes(typeof(IUserService).Assembly)
        .Where(t => t.FullName.EndsWith("Service"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();

    // FluentValidation validators referenced by [Validator] attributes.
    container.RegisterAssemblyTypes(typeof(IUserService).Assembly)
        .AsClosedTypesOf(typeof(IValidator<>))
        .InstancePerLifetimeScope();
});

// ---------------------------------------------------------------------------
// MVC / Web API
// ---------------------------------------------------------------------------
builder.Services
    .AddControllersWithViews(options =>
    {
        // Bind action parameters from the JSON request body the way MVC 5's
        // JsonValueProviderFactory did, so the existing Angular client keeps working.
        options.ModelBinderProviders.Insert(0, new JsonBodyModelBinderProvider());
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        // The Angular client reads PascalCase property names (and numeric enums),
        // matching the default Json.NET behaviour of the old ASP.NET MVC 5 site.
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // The controllers validate ModelState themselves and return custom payloads.
    options.SuppressModelStateInvalidFilter = true;
    options.SuppressInferBindingSourcesForParameters = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------------------------------------------------------------------------
// JWT bearer authentication (tokens are issued by the business layer)
// ---------------------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Deterministic, case-insensitive administrator check (independent of claim-type mapping).
    options.AddPolicy("AdminOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.IsAdministrator()));
});

builder.Services.AddSingleton<SpaAssets>();

// Throttle the authentication endpoints to slow down credential brute-forcing.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

// Baseline security headers.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors();

// Serve the Angular CLI build output (my-app/bundles/browser).
var browserPath = Path.Combine(app.Environment.ContentRootPath, "my-app", "bundles", "browser");
if (Directory.Exists(browserPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(browserPath),
        RequestPath = "/my-app/bundles/browser"
    });
}

app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Everything that is not an API call or a static file is served by the SPA shell.
app.MapControllerRoute(
    name: "spa",
    pattern: "{*path}",
    defaults: new { controller = "Home", action = "Home" },
    constraints: new { path = new SpaFallbackConstraint() });

app.Run();

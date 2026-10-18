using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using NexCart.Business.Security;
using NexCart.Business.Services;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Web.Infrastructure;
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

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Product photos uploaded here are saved under the web root (ProductService.SaveImageToFile)
    // and must be served by the shop as well, so both sites share NexCart.Web's wwwroot.
    WebRootPath = Path.Combine("..", "NexCart.Web", "wwwroot")
});

// The web root only gets a file provider when it exists at startup; create it so photos
// uploaded before the first restart are served too.
var webRootPath = Path.GetFullPath(builder.Environment.WebRootPath);
Directory.CreateDirectory(webRootPath);
builder.Environment.WebRootFileProvider = new PhysicalFileProvider(webRootPath);

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

// Restricted CORS: only the configured origins may call the API (the admin app itself is
// served from this host, so this only matters for an Angular dev server).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "http://localhost:5090", "https://localhost:5090" };
}

// ---------------------------------------------------------------------------
// Dependency injection (Autofac, same registrations as NexCart.Web)
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
// Web API
// ---------------------------------------------------------------------------
builder.Services
    .AddControllers(options =>
    {
        // Same body binding and validation as NexCart.Web, so the admin client's
        // { model: ... } / { Id: ... } payloads bind the same way.
        options.ModelBinderProviders.Insert(0, new JsonBodyModelBinderProvider());
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        // The Angular client reads PascalCase property names (and numeric enums).
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
            // Only accept the single algorithm we sign with.
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // A valid signature is not enough: the user must still exist with the same role.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = UserTokenValidation.OnTokenValidated
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Deterministic, case-insensitive administrator check (independent of claim-type mapping).
    options.AddPolicy("AdminOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.IsAdministrator()));
});

// Throttle the sign-in endpoint to slow down credential brute-forcing.
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
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors();

// Serve the Angular CLI build output (admin-app/bundles/browser) from the site root.
var browserPath = Path.Combine(app.Environment.ContentRootPath, "admin-app", "bundles", "browser");
var adminFiles = Directory.Exists(browserPath) ? new PhysicalFileProvider(browserPath) : null;
if (adminFiles != null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = adminFiles
    });
}

// Shared web root (product photos under /images/products).
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Unknown API URLs are a 404, never the Angular shell.
app.MapFallback("/api/{**rest}", () => Results.NotFound());

// Client-side routes (/products, /orders, ...) all load the admin index.html.
if (adminFiles != null)
{
    app.MapFallbackToFile("{**path:nonfile}", "index.html", new StaticFileOptions
    {
        FileProvider = adminFiles
    });
}

app.Run();

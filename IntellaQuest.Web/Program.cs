using System.IO;
using System.Text;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Dependency injection (Autofac, mirroring the old Global.asax registrations)
// ---------------------------------------------------------------------------
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
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
        .AllowAnyOrigin()
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenConstant.ISSUER_CUSTOMERs,
            ValidateAudience = true,
            ValidAudience = TokenConstant.AUDIENCE_CUSTOMERS,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenConstant.KEY)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<SpaAssets>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

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

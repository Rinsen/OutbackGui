using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rinsen.IdentityProvider;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Rinsen.IdentityProvider.Outback.Entities;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Rinsen.IdentityProvider.Configurations;
using Microsoft.OpenApi.Models;
using System;
using NJsonSchema.Generation;
using NJsonSchema;
using System.IdentityModel.Tokens.Jwt;

namespace Rinsen.Outback.Gui;

public class Startup
{
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("Outback");

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("No connection string provided");

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("operation", new OpenApiInfo { Title = "Operational API", Version = "v1", Description = "APIs for running operational tasks on this Outback installation" });
            c.SwaggerDoc("outback", new OpenApiInfo { Title = "Outback", Version = "v1", Description = "APIs for managing the outback client and scope configurations" });
            c.SwaggerDoc("openid", new OpenApiInfo { Title = "OpenId Connect and OAuth APIs", Version = "v1", Description = "Outback OpenId Connect and OAuth APIs" });
            c.SwaggerDoc("session", new OpenApiInfo { Title = "Session information", Version = "v1", Description = "Information about the user session" });
            c.DocInclusionPredicate((name, desc) =>
            {
                switch (name)
                {
                    case "outback":
                        return string.IsNullOrEmpty(desc.RelativePath) ? false : desc.RelativePath.Contains("outback", StringComparison.OrdinalIgnoreCase);
                    case "operation":
                        return string.IsNullOrEmpty(desc.RelativePath) ? false : desc.RelativePath.Contains("admin", StringComparison.OrdinalIgnoreCase);
                    case "session":
                        return string.IsNullOrEmpty(desc.RelativePath) ? false : desc.RelativePath.Contains("session", StringComparison.OrdinalIgnoreCase);
                    case "openid":
                        return string.IsNullOrEmpty(desc.RelativePath) ? false : (desc.RelativePath.Contains(".well-known/openid-configuration", StringComparison.OrdinalIgnoreCase) || desc.RelativePath.Contains(".well-known/openid-configuration/jwks", StringComparison.OrdinalIgnoreCase)
                        || desc.RelativePath.Contains("connect/authorize", StringComparison.OrdinalIgnoreCase) || desc.RelativePath.Contains("connect/token", StringComparison.OrdinalIgnoreCase));
                    default:
                        return false;
                }
            });
            c.EnableAnnotations();
            c.SupportNonNullableReferenceTypes();
        });

        services.AddRinsenIdentity(options => options.ConnectionString = connectionString);
        services.AddRinsenOutback();

        var gelfhost = Configuration["Rinsen:GelfHost"];

        if (string.IsNullOrEmpty(gelfhost))
            throw new Exception("No gelf host");

        services.AddRinsenGelf(options =>
        {
            options.GelfServiceHostNameOrAddress = gelfhost;
            options.GelfServicePort = 12202;
            options.ApplicationName = "Outback";
        });

        AddAuthentication(services, connectionString);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminsOnly", policy => policy.RequireClaim(RinsenClaimTypes.Administrator, "true"));
            options.AddRequiredScopePolicy("CreateNode", "outback.createnode");
        });

        if (_env.IsRunningInContainer())
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Add(IPAddress.Parse("192.169.1.32"));
            });
        }

        services.AddDbContext<OutbackDbContext>(options =>
        options.UseSqlServer(connectionString));

        services.AddDataProtection()
            .PersistKeysToDbContext<DataProtectionKeyDbContext>();

        services.AddDbContext<DataProtectionKeyDbContext>(options =>
        options.UseSqlServer(connectionString));

        var builder = services.AddMvc(o =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            o.Filters.Add(new AuthorizeFilter(policy));

        })
        .AddRinsenOutbackControllers();

#if DEBUG
        if (_env.IsDevelopment())
        {
            builder.AddRazorRuntimeCompilation();
        }
#endif
    }

    public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
    {
        if (_env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseForwardedHeaders();
            app.UseExceptionHandler("/Error");
        }

        if (!_env.IsRunningInContainer())
        {
            app.UseHttpsRedirection();
        }

        // Enable middleware to serve generated Swagger as a JSON endpoint.
        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/outback/swagger.json", "Outback");
            c.SwaggerEndpoint("/swagger/operation/swagger.json", "Operation");
            c.SwaggerEndpoint("/swagger/session/swagger.json", "Session");
            c.SwaggerEndpoint("/swagger/openid/swagger.json", "OpenId Connect and OAuth");
        });

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseStaticFiles();
        
        app.UseEndpoints(routes =>
        {
            routes.MapControllerRoute(
                name: "default",
                pattern: "{controller=Identity}/{action=Index}");
        });

        logger.LogInformation("Starting");
    }

    private void AddAuthentication(IServiceCollection services, string connectionString)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var httpContextAccessor = new HttpContextAccessor();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.SessionStore = new SqlTicketStore(new SessionStorage(connectionString), httpContextAccessor);
                options.LoginPath = "/Identity/Login";
                options.LogoutPath = "/Identity/LogOut";
                options.AccessDeniedPath = "/Identity/AccessDenied";
            })
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration["Rinsen:Outback"];
                options.Audience = Configuration["Rinsen:ClientId"];
            });
    }
}



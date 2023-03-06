using Microsoft.Extensions.DependencyInjection;
using Rinsen.IdentityProvider.LocalAccounts;
using System;
using Rinsen.IdentityProvider.AuditLogging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using Microsoft.Data.SqlClient;
using Rinsen.IdentityProvider.Outback;
using Rinsen.Outback.Claims;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Rinsen.IdentityProvider.Settings;
using Rinsen.Outback.Accessors;

namespace Rinsen.IdentityProvider;

public static class ExtensionMethods
{
    public static void AddRinsenIdentity(this IServiceCollection services, Action<IdentityOptions> identityOptionsAction)
    {
        var identityOptions = new IdentityOptions();

        identityOptionsAction.Invoke(identityOptions);

        services.AddSingleton(identityOptions);

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<PasswordHashGenerator>();
        services.AddScoped<ILocalAccountService, LocalAccountService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ILocalAccountStorage, LocalAccountStorage>();
        services.AddScoped<IIdentityStorage, IdentityStorage>();
        services.AddScoped<IIdentityAccessor, IdentityAccessService>();
        services.AddScoped<IIdentityAttributeStorage, IdentityAttributeStorage>();
        services.AddScoped<ISessionStorage, SessionStorage>();
        services.AddScoped<AuditLog, AuditLog>();
        services.AddScoped<AuditLogStorage, AuditLogStorage>();
        services.AddScoped<ITwoFactorAuthenticationSessionStorage, TwoFactorAuthenticationSessionStorage>();
        services.AddScoped<IIdentityAccessor, IdentityAccessService>();
        services.AddScoped<IUsedTotpLogStorage, UsedTotpLogStorage>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<RandomStringGenerator, RandomStringGenerator>();
        services.AddSingleton<IAuthorizationHandler, RequireScopeHandler>();
        services.AddScoped<ISettingsManager, SettingsManager>();
        services.AddScoped<ISettingsStorage, SettingsStorage>();

        services.AddScoped<IAllowedCorsOriginsAccessor, AllowedCorsOriginsAccessor>();
        services.AddScoped<IClientAccessor, ClientAccessor>();
        services.AddScoped<IGrantAccessor, GrantAccessor>();
        services.AddScoped<IScopeAccessor, ScopeAccessor>();
        services.AddScoped<IUserInfoAccessor, UserInfoAccessor>();
        services.AddScoped<ITokenSigningAccessor, TokenSigningAccessor>();
        services.AddScoped<IWellKnownSigningAccessor, TokenSigningAccessor>();
        services.AddScoped<ClientService>();
        services.AddScoped<ScopeService>();
        services.AddScoped<DefaultInstaller>();

    }

    public static string GetClaimStringValue(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.GetClaimStringValue(m => m.Type == claimType);
    }

    public static string GetClaimStringValue(this ClaimsPrincipal claimsPrincipal, Predicate<Claim> match)
    {
        if (claimsPrincipal.HasClaim(match))
        {
            try
            {
                return claimsPrincipal.Claims.Where(new Func<Claim, bool>(match)).Single().Value;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The claims collection does not contain exactly one element.");
            }

        }
        else
        {
            throw new InvalidOperationException("The claims collection does not contain a element that match.");
        }
    }

    public static int GetClaimIntValue(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.GetClaimIntValue(m => m.Type == claimType);
    }

    public static int GetClaimIntValue(this ClaimsPrincipal claimsPrincipal, Predicate<Claim> match)
    {
        if (!int.TryParse(claimsPrincipal.GetClaimStringValue(match), out var result))
        {
            throw new InvalidOperationException("Parse exception in claims value");
        }

        return result;
    }

    public static Guid GetClaimGuidValue(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.GetClaimGuidValue(m => m.Type == claimType);
    }

    public static Guid GetClaimGuidValue(this ClaimsPrincipal claimsPrincipal, Predicate<Claim> match)
    {
        Guid result;
        if (!Guid.TryParse(claimsPrincipal.GetClaimStringValue(match), out result))
        {
            throw new InvalidOperationException("Parse exception in claims value");
        }

        return result;
    }

    public static Guid GetSubjectId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.GetClaimGuidValue(m => m.Type == StandardClaims.Subject);
    }

    public static bool GetClaimBoolValue(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.GetClaimBoolValue(m => m.Type == claimType);
    }

    public static bool GetClaimBoolValue(this ClaimsPrincipal claimsPrincipal, Predicate<Claim> match)
    {
        if (claimsPrincipal.HasClaim(match))
        {
            try
            {
                return claimsPrincipal.Claims.Where(new Func<Claim, bool>(match)).Single().Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The claims collection does not contain exactly one element.");
            }
        }
        else
        {
            throw new InvalidOperationException("The claims collection does not contain a element that match.");
        }
    }

    public static T? GetValueOrDefault<T>(this SqlDataReader reader, string columnName)
    {
        object value = reader[columnName];

        if (value != DBNull.Value)
            return (T)value;

        return default;
    }

    public static SqlParameter AddWithNullableValue(this SqlParameterCollection collection, string parameterName, object? value)
    {
        if (value == null)
            return collection.AddWithValue(parameterName, DBNull.Value);
        else
            return collection.AddWithValue(parameterName, value);
    }

    public static SqlParameter AddWithNullableValue(this SqlParameterCollection collection, string parameterName, byte?[] value)
    {
        var parameter = new SqlParameter(parameterName, System.Data.SqlDbType.VarBinary);
        collection.Add(parameter);
        if (value == null)
            parameter.Value = DBNull.Value;
        else
            parameter.Value = value;

        return parameter;
    }

    public static SqlParameter AddWithNullableValue(this SqlParameterCollection collection, string parameterName, string value)
    {
        if (string.IsNullOrEmpty(value))
            return collection.AddWithValue(parameterName, DBNull.Value);
        else
            return collection.AddWithValue(parameterName, value);
    }

    public static bool IsRunningInContainer(this IHostEnvironment environment)
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
}

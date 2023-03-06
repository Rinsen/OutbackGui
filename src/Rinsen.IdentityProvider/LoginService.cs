using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OtpNet;
using Rinsen.IdentityProvider.LocalAccounts;
using Rinsen.Outback.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rinsen.IdentityProvider;

public class LoginService : ILoginService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RandomStringGenerator _randomStringGenerator;
    private readonly IIdentityService _identityService;
    private readonly ILogger<LoginService> _log;
    private readonly ILocalAccountService _localAccountService;
    private readonly IIdentityAttributeStorage _identityAttributeStorage;
    private readonly IUsedTotpLogStorage _usedTotpLogStorage;
    private readonly ITwoFactorAuthenticationSessionStorage _twoFactorAuthenticationSessionStorage;

    public LoginService(ILocalAccountService localAccountService,
        IIdentityService identityService,
        IIdentityAttributeStorage identityAttributeStorage,
        IUsedTotpLogStorage usedTotpLogStorage,
        ITwoFactorAuthenticationSessionStorage twoFactorAuthenticationSessionStorage,
        IHttpContextAccessor httpContextAccessor,
        RandomStringGenerator randomStringGenerator,
        ILogger<LoginService> log)
    {
        _localAccountService = localAccountService;
        _identityService = identityService;
        _identityAttributeStorage = identityAttributeStorage;
        _usedTotpLogStorage = usedTotpLogStorage;
        _twoFactorAuthenticationSessionStorage = twoFactorAuthenticationSessionStorage;
        _httpContextAccessor = httpContextAccessor;
        _randomStringGenerator = randomStringGenerator;
        _log = log;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, string host, bool rememberMe)
    {
        LocalAccount localAccount;
        try
        {
            localAccount = await _localAccountService.GetLocalAccountAsync(email, password);
        }
        catch (UnauthorizedAccessException e)
        {
            _log.LogWarning(e, $"Login failed for email {email}", email);

            return LoginResult.Failure();
        }

        if (localAccount == null)
        {
            return LoginResult.Failure();
        }

        if (localAccount.TwoFactorAppNotificationEnabled is not null || localAccount.TwoFactorEmailEnabled is not null
            || localAccount.TwoFactorSmsEnabled is not null || localAccount.TwoFactorTotpEnabled is not null)
        {
            string keyCode = string.Empty;
            if (localAccount.TwoFactorEmailEnabled is object || localAccount.TwoFactorSmsEnabled is object)
            {
                keyCode = _randomStringGenerator.GetRandomString(10);
            }

            var sessionId = _randomStringGenerator.GetRandomString(50);
            var twoFactorAuthenticationSession = new TwoFactorAuthenticationSession
            {
                Created = DateTimeOffset.Now,
                IdentityId = localAccount.IdentityId,
                KeyCode = keyCode,
                SessionId = sessionId,
                Type = TwoFactorType.NotSelected
            };

            await _twoFactorAuthenticationSessionStorage.Create(twoFactorAuthenticationSession);

            return LoginResult.RequireTwoFactor(twoFactorAuthenticationSession.SessionId, localAccount.TwoFactorEmailEnabled is object, localAccount.TwoFactorSmsEnabled is object, localAccount.TwoFactorTotpEnabled is object, localAccount.TwoFactorAppNotificationEnabled is object);
        }

        return await PrivateLogin(host, rememberMe, localAccount);
    }

    public async Task StartTotpFlow(string authSessionId)
    {
        var twoFactorAuthenticationSession = await _twoFactorAuthenticationSessionStorage.Get(authSessionId);

        if (twoFactorAuthenticationSession is not null)
        {
            twoFactorAuthenticationSession.Type = TwoFactorType.Totp;

            await _twoFactorAuthenticationSessionStorage.Update(twoFactorAuthenticationSession);
        }
        else
        {
            throw new Exception("Two factor auth session not found");
        }
    }

    public async Task<LoginResult> ConfirmTotpCode(string authSessionId, string keyCode, string host, bool rememberMe)
    {
        var twoFactorAuthenticationSession = await _twoFactorAuthenticationSessionStorage.Get(authSessionId);
        var localAccount = await _localAccountService.GetLocalAccountAsync(twoFactorAuthenticationSession.IdentityId);

        if (localAccount.TwoFactorTotpEnabled is null || localAccount.SharedTotpSecret is null)
        {
            throw new Exception("Totp is not enabled for this local account");
        }

        var totp = new Totp(localAccount.SharedTotpSecret);
        if(totp.VerifyTotp(keyCode, out var timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            await _usedTotpLogStorage.CreateLog(new UsedTotpLog { IdentityId = localAccount.IdentityId, Code = keyCode, UsedTime = DateTimeOffset.Now });
            
            var loginResult = await PrivateLogin(host, rememberMe, localAccount);

            return loginResult;
        }

        throw new Exception("Totp key not valid");
    }

    private async Task<LoginResult> PrivateLogin(string host, bool rememberMe, LocalAccount localAccount)
    {
        var identity = await _identityService.GetIdentityAsync(localAccount.IdentityId);

        var expiration = DateTimeOffset.UtcNow.AddHours(1);

        if (rememberMe)
        {
            expiration = DateTimeOffset.UtcNow.AddMonths(1);
        }

        var claims = await GetClaimsForIdentityAsync(identity, host, rememberMe, expiration);

        var authenticationProperties = new AuthenticationProperties()
        {
            AllowRefresh = true,
            ExpiresUtc = expiration,
            IsPersistent = rememberMe,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "RinsenPassword", StandardClaims.Name, "role"));

        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("No active http context");
        }

        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);

        return LoginResult.Success(principal, localAccount.LoginId);
    }

    private async Task<List<Claim>> GetClaimsForIdentityAsync(Identity identity, string host, bool rememberMe, DateTimeOffset expiration)
    {
        var sessionId = _randomStringGenerator.GetRandomString(32);

        var timeToExpiration = expiration.Subtract(DateTimeOffset.Now);

        var claims = new List<Claim>
        {
            new Claim(StandardClaims.Name, identity.GivenName + " " + identity.Surname, ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.FamilyName, identity.Surname, ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.GivenName, identity.GivenName, ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.Issuer, host, ClaimValueTypes.String,  RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.Subject, identity.IdentityId.ToString(), ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.SessionId, sessionId, ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider),
            new Claim(StandardClaims.Expiration, expiration.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var identityAttributes = await _identityAttributeStorage.GetIdentityAttributesAsync(identity.IdentityId);

        if (identityAttributes.Any(m => m.Attribute == "Administrator"))
        {
            claims.Add(new Claim(RinsenClaimTypes.Administrator, "true", ClaimValueTypes.String, RinsenIdentityConstants.RinsenIdentityProvider));
        }

        return claims;
    }

    public Task LogoutAsync()
    {
        throw new NotImplementedException();
    }
}

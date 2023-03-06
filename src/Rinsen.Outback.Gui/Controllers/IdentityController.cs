using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rinsen.IdentityProvider;
using Rinsen.IdentityProvider.AuditLogging;
using Rinsen.IdentityProvider.LocalAccounts;
using Rinsen.Outback.Gui.Models;
using Rinsen.Outback.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UAParser;

namespace Rinsen.Outback.Gui.Controllers;

public class IdentityController : Controller
{
    private readonly ILoginService _loginService;
    private readonly IIdentityService _identityService;
    private readonly ILocalAccountService _localAccountService;
    private readonly AuditLog _auditLog;
    private readonly IConfiguration _configuration;
    private readonly IIdentityAccessor _identityAccessor;
    private readonly ISessionStorage _sessionStorage;
    private readonly IWebHostEnvironment _env;

    public IdentityController(ILoginService loginService,
        IIdentityService identityService,
        ILocalAccountService localAccountService,
        AuditLog auditLog,
        IConfiguration configuration,
        IIdentityAccessor identityAccessor,
        ISessionStorage sessionStorage,
        IWebHostEnvironment env)
    {
        _loginService = loginService;
        _identityService = identityService;
        _localAccountService = localAccountService;
        _auditLog = auditLog;
        _configuration = configuration;
        _identityAccessor = identityAccessor;
        _sessionStorage = sessionStorage;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var parser = Parser.GetDefault();
        var sessions = await _sessionStorage.GetAsync(User.GetSubjectId(), true);
        var sessionId = _identityAccessor.ClaimsPrincipal.GetClaimStringValue(StandardClaims.SessionId);
        
        return View(new IdentityOverview
        {
            CurrentSession = GetCurrentSession(sessions, sessionId),
            Sessions = sessions.Where(m => m.SessionId != sessionId).Select(m =>
            {
                var client = parser.Parse(m.UserAgent);
                
                return new SessionModel
                {
                    Id = m.Id,
                    ClientDescrition = client.ToString(),
                    Expired = m.Expires > DateTimeOffset.Now,
                    IpAddress = m.IpAddress,
                    Expires = m.Expires,
                    Created = m.Created,
                    Deleted = m.Deleted is not null
                };
            }).ToList()
        });
    }

    private static SessionModel GetCurrentSession(IEnumerable<Session> sessions, string sessionId)
    {
        var currentSession = sessions.Single(m => m.SessionId == sessionId);

        var parser = Parser.GetDefault();
        var client = parser.Parse(currentSession.UserAgent);

        return new SessionModel
        {
            Id = currentSession.Id,
            ClientDescrition = client.ToString(),
            IpAddress = currentSession.IpAddress,
            Expires = currentSession.Expires,
            Created = currentSession.Created,
            Deleted = currentSession.Deleted is not null
        };
    }

    [HttpGet] 
    [AllowAnonymous]
    public IActionResult Login(string returnUrl)
    {
        var model = new LoginModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _loginService.LoginAsync(model.Email, model.Password, Request.Host.Value, model.RememberMe);

            if (result.Succeeded && result.Principal != null)
            {
                return await LoginSuccess(result.LoginId, model.ReturnUrl, result.Principal);
            }
            else if(result.TwoFactorRequired)
            {
                var twoFactorModel = new TwoFactorModel
                {
                    TypeSelected = TwoFactorType.Totp,
                    TwoFactorAppNotificationEnabled = result.TwoFactorAppNotificationEnabled,
                    ReturnUrl = model.ReturnUrl,
                    RememberMe = model.RememberMe,
                    TwoFactorEmailEnabled = result.TwoFactorEmailEnabled,
                    TwoFactorSmsEnabled = result.TwoFactorSmsEnabled,
                    TwoFactorTotpEnabled = result.TwoFactorTotpEnabled,
                };
                
                Response.Cookies.Append("AuthSessionId", result.TwoFactorAuthenticationSessionId);

                return View("TwoFactor", twoFactorModel);
            }
            else
            {
                await CreateAuditLogEvent("InvalidLoginAttempt", $"Email '{model.Email}'");

                model.InvalidEmailOrPassword = true;
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return View(model);
    }

    private async Task<IActionResult> LoginSuccess(string loginId, string returnUrl, ClaimsPrincipal principal)
    {
        await CreateAuditLogEvent("LoginSuccess", $"LoginId '{loginId}'");

        // Set logged in user to the one just created as this only will be provided at next request by the framework
        HttpContext.User = principal;

        if (_env.IsDevelopment())
        {
            return Redirect(returnUrl);
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("~/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactor(TwoFactorModel model)
    {
        if (ModelState.IsValid)
        {
            if(Request.Cookies.TryGetValue("AuthSessionId", out var authSessionId))
            {
                if (string.IsNullOrEmpty(authSessionId))
                {
                    throw new Exception("Auth session id missing");
                }

                if (model.TypeSelected == TwoFactorType.Totp)
                {
                    if (string.IsNullOrEmpty(model.KeyCode))
                    {
                        await _loginService.StartTotpFlow(authSessionId);

                        return View(model);
                    }

                    Response.Cookies.Delete("AuthSessionId");
                    LoginResult result;
                    try
                    {
                        result = await _loginService.ConfirmTotpCode(authSessionId, model.KeyCode, Request.Host.Value, model.RememberMe);
                    }
                    catch (TotpCodeAlreadyUsedException)
                    {
                        await CreateAuditLogEvent("TotpCodeUsed", $"Code {model.KeyCode} is already used");
                        throw;
                    }

                    if (result.Succeeded && result.Principal != null)
                    {
                        return await LoginSuccess(result.LoginId, model.ReturnUrl, result.Principal);
                    }
                    else
                    {
                        await CreateAuditLogEvent("InvalidLoginAttempt", $"Totp key code not valid");

                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
            }
            else
            {
                await CreateAuditLogEvent("InvalidLoginAttempt", $"Auth Session Id not found");
            }
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Create()
    {
        return View(new CreateIdentityModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateIdentityModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.InviteCode != _configuration["Rinsen:InvitationCode"])
            {
                ModelState.AddModelError("InviteCode", "Invalid invite code.");

                await CreateAuditLogEvent("InvalidInvitationCode", $"Email '{model.Email}', '{model.InviteCode}'");

                return View(model);
            }

            var createIdentityResult = await _identityService.CreateAsync(model.GivenName, model.Surname, model.Email, model.PhoneNumber);

            if (createIdentityResult.Succeeded && createIdentityResult.Identity != null)
            {
                await CreateAuditLogEvent("IdentityCreated", $"Email '{model.Email}'");

                var createLocalAccountResult = await _localAccountService.CreateAsync(createIdentityResult.Identity.IdentityId, model.Email, model.Password);

                if (createLocalAccountResult.Succeeded)
                {
                    var loginResult = await _loginService.LoginAsync(model.Email, model.Password, Request.Host.Value, false);

                    if (loginResult.Succeeded)
                    {
                        return View("UserCreated");
                    }

                    return View("UserCreated");
                }
            }

            await CreateAuditLogEvent("FailedToCreateIdentity", $"Email '{model.Email}'");

            ModelState.AddModelError(string.Empty, "Create user invalid.");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EnableTotp()
    {
        var totpSecret = await _localAccountService.EnableTotp();

        return Ok(totpSecret);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        return SignOut(new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/Identity/LoggedOut" }, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoggedOut()
    {
        return View();
    }

    private Task CreateAuditLogEvent(string eventType, string details)
    {
        if (HttpContext.Connection.RemoteIpAddress is null)
        {
            return _auditLog.Log(eventType, details, "Not known");
        }

        return _auditLog.Log(eventType, details, HttpContext.Connection.RemoteIpAddress.ToString());
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied([FromQuery]string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            ViewBag.ErrorMessage = $"{returnUrl} is not allowed";
        }
        else
        {
            ViewBag.ErrorMessage = "No path";
        }

        return View();
    }
}

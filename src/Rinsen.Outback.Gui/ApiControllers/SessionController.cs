using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rinsen.IdentityProvider;
using Rinsen.Outback.Claims;

namespace Rinsen.Outback.Gui.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : Controller
{
    private readonly IWebHostEnvironment _env;

    public SessionController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<SessionInformation> Get()
    {
        var identity = User.Identity;
        var result = await HttpContext.AuthenticateAsync();

        if (identity != null && identity.IsAuthenticated)
        {
            var expiration = User.GetClaimIntValue(StandardClaims.Expiration);

            return new SessionInformation
            {
                IsAuthenticated = true,
                Expiration = expiration,
                Name = identity.Name ?? string.Empty,
                LoginUrl = string.Empty,
                LogoutUrl = GetLogoutUrl()
            };
        }

        return new SessionInformation { IsAuthenticated = false, LoginUrl = GetLoginUrl() };
    }

    private string GetLogoutUrl()
    {
        if (_env.IsDevelopment())
        {
            return $"https://localhost:44300/Identity/Logout";
        }

        if (Request.IsHttps)
        {
            return $"https://{Request.Host.Value}/Identity/Logout";
        }

        return $"http://{Request.Host.Value}/Identity/Logout";
    }

    private string GetLoginUrl()
    {
        if (_env.IsDevelopment())
        {
            return $"https://localhost:44300/Identity/Login";
        }

        if (Request.IsHttps)
        {
            return $"https://{Request.Host.Value}/Identity/Login";
        }

        return $"http://{Request.Host.Value}/Identity/Login";
    }
}

public class SessionInformation
{
    public bool IsAuthenticated { get; set; }

    public int Expiration { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LoginUrl { get; set; } = string.Empty;

    public string LogoutUrl { get; set; } = string.Empty;
}

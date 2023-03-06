using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Rinsen.Outback.Claims;

namespace Rinsen.IdentityProvider;

public class SqlTicketStore : ITicketStore
{
    private readonly TicketSerializer _ticketSerializer = new();
    private readonly ISessionStorage _sessionStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SqlTicketStore(ISessionStorage sessionStorage,
        IHttpContextAccessor httpContextAccessor)
    {
        _sessionStorage = sessionStorage;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RemoveAsync(string key)
    {
        await _sessionStorage.DeleteAsync(key);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var session = await _sessionStorage.GetAsync(key);

        if (ticket.Principal.Identity is not ClaimsIdentity claimsIdentity)
        {
            throw new Exception("No claims identity found");
        }

        if (session == default)
        {
            throw new Exception($"No session found for key {key}");
        }

        var expirationClaim = claimsIdentity.FindFirst(StandardClaims.Expiration);

        if (expirationClaim != default)
        {
            claimsIdentity.RemoveClaim(expirationClaim);
            claimsIdentity.AddClaim(new Claim(StandardClaims.Expiration, (ticket.Properties.ExpiresUtc ?? DateTimeOffset.Now.AddHours(1)).ToUnixTimeSeconds().ToString()));
        }

        var serializedTicket = _ticketSerializer.Serialize(ticket);

        session.SerializedTicket = serializedTicket;
        session.Updated = DateTimeOffset.Now;
        session.Expires = ticket.Properties.ExpiresUtc ?? DateTimeOffset.Now.AddHours(1);

        await _sessionStorage.UpdateAsync(session);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var session = await _sessionStorage.GetAsync(key);

        if (session == default)
        {
            throw new Exception($"No session found for key {key}");
        }

        var ticket = _ticketSerializer.Deserialize(session.SerializedTicket);

        //if (ticket.Principal.HasClaim(m => m.Type == StandardClaims.Expiration))
        //{
        //    var expiration = ticket.Principal.GetClaimIntValue(StandardClaims.Expiration);

        //    if (ticket.Properties.ExpiresUtc == default)
        //    {
        //        ticket.Properties.ExpiresUtc = DateTimeOffset.FromUnixTimeSeconds(expiration);
        //    }
        //    else
        //    {
        //        var expiresUtcTicket = DateTimeOffset.FromUnixTimeSeconds(expiration);

        //        if (expiresUtcTicket.UtcTicks != ticket.Properties.ExpiresUtc.Value.UtcTicks)
        //        {
        //            ticket.Principal.Claims.First(m => m.Type == StandardClaims.Expiration).Value == ticket.Properties.ExpiresUtc.Value.ToUnixTimeSeconds().ToString();
        //        }
        //    }

        //}
        //else
        //{

        //}


        //DateTimeOffset.UtcNow.AddSeconds(timeToExpiration.TotalSeconds).ToUnixTimeSeconds().ToString()

        return ticket;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var session = new Session
        {
            SessionId = ticket.Principal.GetClaimStringValue(StandardClaims.SessionId),
            IdentityId = ticket.Principal.GetClaimGuidValue(StandardClaims.Subject),
            CorrelationId = Guid.NewGuid(),
            UserAgent = string.Empty,
            IpAddress = string.Empty,
            Created = DateTimeOffset.Now,
            Updated = DateTimeOffset.Now,
            Deleted = null,
            Expires = ticket.Properties.ExpiresUtc ?? DateTimeOffset.Now.AddDays(1),
            SerializedTicket = _ticketSerializer.Serialize(ticket)
        };

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress != null)
            {
                session.IpAddress = remoteIpAddress.ToString();
            }

            var userAgent = httpContext.Request.Headers["User-Agent"];
            if (!string.IsNullOrEmpty(userAgent))
            {
                if (userAgent.Count > 200)
                {
                    session.UserAgent = userAgent.ToString()[..200];
                }
                else
                {
#pragma warning disable CS8601 // Possible null reference assignment. Nooo, I check a few lines before?
                    session.UserAgent = userAgent;
#pragma warning restore CS8601 // Possible null reference assignment.
                }
            }
        }

        await _sessionStorage.CreateAsync(session);

        return session.SessionId;

    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Rinsen.IdentityProvider.Outback;
using Swashbuckle.AspNetCore.Annotations;

namespace Rinsen.Outback.Gui.ApiControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize("AdminsOnly")]
public class AdminController : Controller
{
    private readonly DefaultInstaller _defaultInstaller;
    private readonly IConfiguration _configuration;

    public AdminController(DefaultInstaller defaultInstaller,
        IConfiguration configuration
        )
    {
        _defaultInstaller = defaultInstaller;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("Install")]
    [SwaggerOperation(summary: "Install default clients", OperationId = "Admin_Install")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> InstallDefault()
    {
        var credentials = await _defaultInstaller.Install();

        return Ok(credentials);
    }

    [HttpGet]
    [Route("Configuration")]
    [Authorize("AdminsOnly")]
    [SwaggerOperation(summary: "Get IConfiguration parameters", OperationId = "Admin_Configuration")]
    [ProducesResponseType(200)]
    public IActionResult GetConfiguration()
    {
        var result = ((IConfigurationRoot)_configuration).GetDebugView();

        return Ok(result);
    }

    //public async Task<IActionResult> Diagnostics()
    //{
    //    try
    //    {
    //        using (var httpClient = new HttpClient())
    //        {
    //            var disco = await httpClient.GetDiscoveryDocumentAsync(_configuration["Rinsen:InnovationBoost"]);

    //            if (disco.IsError)
    //                throw new Exception(disco.Error);

    //            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    //            {
    //                Address = disco.TokenEndpoint,
    //                ClientId = _configuration["Rinsen:ClientId"],
    //                ClientSecret = _configuration["Rinsen:ClientSecret"]
    //            });

    //            if (tokenResponse.IsError)
    //            {
    //                throw new Exception(tokenResponse.Error);
    //            }

    //            return Ok(tokenResponse.AccessToken);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        return Ok(e.Message + e.StackTrace);
    //    }
    //}
}

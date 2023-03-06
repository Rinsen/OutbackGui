using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rinsen.IdentityProvider;
using Swashbuckle.AspNetCore.Annotations;

namespace Rinsen.Outback.Gui.ApiControllers;

[ApiController]
[Route("Outback/api/[controller]")]
[Authorize(Policy = "AdminsOnly")]
public class RandomController : Controller
{
    private readonly RandomStringGenerator _randomStringGenerator;

    public RandomController(RandomStringGenerator randomStringGenerator)
    {
        _randomStringGenerator = randomStringGenerator;
    }

    [HttpGet("{count}")]
    [SwaggerOperation(summary: "Get a random string with provided length", OperationId = "Random_Get")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public ActionResult<string> Get(int count)
    {
        return _randomStringGenerator.GetRandomString(count);
    }
}

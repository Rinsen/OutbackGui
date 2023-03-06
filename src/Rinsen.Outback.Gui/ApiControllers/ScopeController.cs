using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rinsen.IdentityProvider.Outback;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Gui.ApiModels;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rinsen.Outback.Gui.ApiControllers;

[Route("Outback/api/[controller]")]
[Authorize(Policy = "AdminsOnly")]
[ApiController]
public class ScopeController : ControllerBase
{
    private readonly ScopeService _scopeService;

    public ScopeController(ScopeService scopeService)
    {
        _scopeService = scopeService;
    }

    // GET: api/<ScopeController>
    [HttpGet]
    [SwaggerOperation(summary: "Get all scopes", OperationId = "Scope_Get")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IEnumerable<OutbackScope>> Get()
    {
        var clients = await _scopeService.GetAllAsync();

        return clients;
    }

    // GET api/<ScopeController>/5
    [HttpGet("{id}")]
    [SwaggerOperation(summary: "Get a specific scope by id", OperationId = "Scope_Get")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OutbackScope>> GetById(int id)
    {
        var scope = await _scopeService.GetScopeAsync(id);

        if (scope == default)
            return NotFound();

        return scope;
    }

    // POST api/<ScopeController>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(403)]
    [SwaggerOperation(summary: "Create a new scope", OperationId = "Scope_Create")]
    public async Task<ActionResult<OutbackScope>> Post([FromBody]CreateScope createScope)
    {
        var scopeId = await _scopeService.CreateScopeAsync(createScope.Name, createScope.ScopeName, createScope.Audience, createScope.Description, createScope.ShowInDiscoveryDocument, createScope.ClaimsInIdToken, createScope.Enabled);

        var scope = await _scopeService.GetScopeAsync(scopeId);

        return CreatedAtAction(nameof(GetById),
           new { id = scopeId }, scope);
    }

    // PUT api/<ScopeController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(summary: "Update scope", OperationId = "Scope_Update")]
    public async Task<ActionResult> Put(int id, [FromBody] OutbackScope scope)
    {
        await _scopeService.UpdateScopeAsync(id, scope);

        return Ok();
    }

    // DELETE api/<ScopeController>/5
    [HttpDelete("{id}")]
    [SwaggerOperation(summary: "Delete a scope", OperationId = "Scope_Delete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _scopeService.DeleteScopeAsync(id);

        return Ok();
    }
}

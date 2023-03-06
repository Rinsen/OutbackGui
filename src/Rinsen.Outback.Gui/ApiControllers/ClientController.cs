using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rinsen.IdentityProvider.Outback;
using Rinsen.IdentityProvider.Outback.Entities;
using Rinsen.Outback.Gui.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Rinsen.Outback.Gui.ApiControllers;

[ApiController]
[Route("Outback/api/[controller]")]
[Authorize(Policy = "AdminsOnly")]
public class ClientController : Controller
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [SwaggerOperation(summary: "Get a list of all clients", OperationId = "Client_Get" )]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<OutbackClient>>> GetAll()
    {
        var clients = await _clientService.GetAll();

        clients.ForEach(c => c.Secrets.ForEach(s => s.Secret = "****"));

        return clients;
    }

    [HttpGet("{id}")]
    [SwaggerOperation(summary: "Get a specific client by id", OperationId = "Client_Get")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OutbackClient>> GetById(string id)
    {
        var client = await _clientService.GetClient(id);

        if (client == default)
            return NotFound();

        client.Secrets.ForEach(s => s.Secret = "****");

        return client;
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(summary: "Delete a client", OperationId = "Client_Delete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(string id)
    {
        await _clientService.DeleteClient(id);

        return Ok();
    }

    [HttpPost]
    [SwaggerOperation(summary: "Create a client", OperationId = "Client_Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OutbackClient>> Create([Required] CreateClient createClient)
    {
        var clientId = Guid.NewGuid().ToString();

        await _clientService.CreateNewClient(clientId, createClient.ClientName, createClient.Description, createClient.FamilyId, createClient.ClientType);

        var client = await _clientService.GetClient(clientId);

        return CreatedAtAction(nameof(GetById),
           new { id = clientId }, client);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(summary: "Update a client", OperationId = "Client_Update")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> Update(string id, [FromBody][Required] OutbackClient client)
    {
        await _clientService.UpdateClient(id, client);

        return Ok();
    }

    [HttpGet]
    [SwaggerOperation(summary: "Get all client families", OperationId = "Client_GetFamily")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [Route("~/Outback/api/[controller]/Family")]
    public async Task<ActionResult<List<OutbackClientFamily>>> GetAllClientFamilies()
    {
        var clientFamilies = await _clientService.GetAllClientFamilies();

        return clientFamilies;
    }

    [HttpPost]
    [SwaggerOperation(summary: "Create client family", OperationId = "Client_CreateFamily")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Route("~/Outback/api/[controller]/Family")]
    public async Task<ActionResult<OutbackClientFamily>> CreateFamily([Required] CreateFamily createFamily)
    {
        var family = await _clientService.CreateNewFamily(createFamily.Name, createFamily.Description);

        return CreatedAtAction(nameof(GetById),
           new { id = family.Id }, family);
    }
}

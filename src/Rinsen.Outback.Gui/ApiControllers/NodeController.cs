//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Rinsen.IdentityProvider;
//using Rinsen.Outback.Gui.ApiModels;

//namespace Rinsen.Outback.Gui.ApiControllers
//{
//    [ApiController]
//    [Authorize(AuthenticationSchemes = "Bearer", Policy = "CreateNode")]
//    [Route("api/[controller]")]
//    public class NodeController : Controller
//    {
//        //private readonly IdentityServerClientBusiness _identityServerClientBusiness;
//        private readonly RandomStringGenerator _randomStringGenerator;

//        public NodeController(
//            //IdentityServerClientBusiness identityServerClientBusiness,
//            RandomStringGenerator randomStringGenerator)
//        {
//            //_identityServerClientBusiness = identityServerClientBusiness;
//            _randomStringGenerator = randomStringGenerator;
//        }

//        [HttpPost]
//        [ProducesResponseType(201)]
//        [ProducesResponseType(400)]
//        [ProducesResponseType(403)]
//        public Task<ActionResult<CreatedNodeResult>> CreateNode([FromBody]CreateNodeModel createNode)
//        {
//            throw new NotImplementedException();
//            //var clientId = await _identityServerClientBusiness.CreateNewTypedClient(createNode.ClientName, createNode.ClientDescription, "Node");

//            //var client = await _identityServerClientBusiness.GetIdentityServerClient(clientId);

//            //var generatedSecret = _randomStringGenerator.GetRandomString(40);
//            //client.ClientSecrets.Add(new IdentityServerClientSecret
//            //{
//            //    Description = "First auto generated secret returned to creator",
//            //    State = ObjectState.Added,
//            //    Type = "SharedSecret",
//            //    Value = generatedSecret.Sha256()
//            //});

//            //client.AllowedGrantTypes.Add(new IdentityServerClientGrantType
//            //{
//            //    GrantType = "client_credentials",
//            //    State = ObjectState.Added
//            //});

//            //await _identityServerClientBusiness.UpdateClient(client);

//            //return new CreatedNodeResult
//            //{
//            //    NodeId = clientId,
//            //    GeneratedSecret = generatedSecret
//            //};
//        }
//    }
//}

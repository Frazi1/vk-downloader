// using System;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using RestSharp;
// using VkDownloader.Pages;
//
// namespace VkDownloader.Shared
// {
//     [Route("{controller}")]
//     public class VkCallbackController : ControllerBase
//     {
//         // private readonly VkAuthLogic _authLogic;
//
//         public VkCallbackController(VkAuthLogic authLogic)
//         {
//             _authLogic = authLogic;
//         }
//
//         [HttpGet]
//         public async Task<IActionResult> OAuthRedirect([FromQuery] string code)
//         {
//             var authTokenUri = _authLogic.BuildGetAccessTokenUri(code);
//             var restResponse = await new RestClient().ExecuteGetAsync(new RestRequest(authTokenUri));
//             var jObject = JsonDocument.Parse(restResponse.Content);
//
//             string? accessToken = jObject.RootElement.EnumerateObject()
//                 .First(p => p.NameEquals("access_token"))
//                 .Value.ToString();
//             
//             //Got access token
//             return Redirect($"/Auth/{accessToken}");
//         }
//     }
// }
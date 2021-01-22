using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using VkDownloader.Pages;
using VkDownloader.Vk;

namespace VkDownloader.Shared
{
    [Route("{controller}")]
    public class VkCallbackController : ControllerBase
    {
        private readonly VkAuthLogic _authLogic;
        private readonly VkCredentialsStorage _credentialsStorage;

        public VkCallbackController(VkAuthLogic authLogic,
            VkCredentialsStorage credentialsStorage)
        {
            _authLogic = authLogic;
            _credentialsStorage = credentialsStorage;
        }

        [HttpGet]
        public async Task<IActionResult> OAuthRedirect([FromQuery] string code)
        {
            var authTokenUri = _authLogic.BuildGetAccessTokenUri(code);
            var restResponse = await new RestClient().ExecuteGetAsync(new RestRequest(authTokenUri));
            var jObject = JsonDocument.Parse(restResponse.Content);

            string? accessToken = jObject.RootElement.EnumerateObject()
                .First(p => p.NameEquals("access_token"))
                .Value.ToString();

            _credentialsStorage.SetAccessToken(accessToken!);

            //Got access token
            return Redirect("/");
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace VkDownloader
{
    [Route("/{controller}/{action}")]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet]
        public async Task<string[]> GetExternalProviders()
        {
            var schemes = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            return (await schemes.GetAllSchemesAsync())
                .Where(scheme => !string.IsNullOrEmpty(scheme.Name))
                .Select(scheme => scheme.Name).ToArray();
        }
        
        public async Task<IActionResult> SignIn([FromQuery] string provider)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = HttpContext.Request.Headers["Referrer"] }, provider);
        }
    }
}
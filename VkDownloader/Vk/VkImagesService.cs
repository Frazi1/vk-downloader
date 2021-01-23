using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Vkontakte;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using VkDownloader.Pages;

namespace VkDownloader.Vk
{
    public class VkImagesService
    {
        private const string BaseApiUrl = "https://api.vk.com/method/";
        private const string DefaultVkVersion = "5.142";


        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IPrincipal _principal;

        private IHttpContextAccessor _context;
        // private readonly VkAuthLogic _authLogic;

        public VkImagesService(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("vk");

        private async Task<string> BuildUrl(string apiType, string methodName, Dictionary<string, string> queryParams)
        {
            string baseQuery = $"{BaseApiUrl}{apiType}.{methodName}";
            string accessToken = await _context.HttpContext!.GetTokenAsync(VkontakteAuthenticationDefaults.AuthenticationScheme, "access_token")
                                 ?? throw new ArgumentNullException("access_token is missing");

            var @params = queryParams
                .Append(new KeyValuePair<string, string>("access_token", accessToken));

            if (!queryParams.ContainsKey("v"))
            {
                @params = @params.Append(new KeyValuePair<string, string>("v", DefaultVkVersion));
            }

            return QueryHelpers.AddQueryString(baseQuery, @params!);
        }

        public async Task<int> GetOwnerIdAsync(UserOrGroupName name)
        {
            using var client = CreateClient();
            string requestUri = await BuildUrl("groups", "getById", new Dictionary<string, string> {{"group_id", name.Name}});
            var response = await client.GetFromJsonAsync<JsonDocument>(requestUri);

            var root = response!.RootElement;

            return root.GetProperty("response")[0].GetProperty("id").GetInt32();
        }

        public async Task<ImmutableArray<string?>> GetImagesAsync(UserOrGroupName userOrGroupName, int postCount, int offset, CancellationToken cancellationToken)
        {
            using var client = CreateClient();
            int ownerId = await GetOwnerIdAsync(userOrGroupName);

            string? requestUri = await BuildUrl("wall", "get", new Dictionary<string, string>
            {
                {"owner_id", $"{-ownerId}"},
                {"v", "5.136"},
                {"count", $"{postCount}"},
                {"offset", $"{offset}"}
            });
            var response = await client.GetFromJsonAsync<WallResponse>(requestUri, cancellationToken);

            var attachments = response!.Response.Items
                .Where(x => x.Attachments != null)
                .SelectMany(i => i.Attachments!)
                .ToArray();

            var photos = attachments
                .Where(a => a.Type == "photo")
                .Select(a => a.Photo)
                .Where(x => x != null);
           
            return photos
                .Select(p => p.Sizes.Last())
                .Select(s => s.Url)
                .ToImmutableArray()!;
        }
    }
}
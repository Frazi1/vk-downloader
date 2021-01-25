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

        public async Task<int> GetOwnerIdAsync(GroupName name)
        {
            using var client = CreateClient();
            string requestUri = await BuildUrl("groups", "getById", new Dictionary<string, string> {{"group_id", name.Name}});
            var response = await client.GetFromJsonAsync<JsonDocument>(requestUri);

            var root = response!.RootElement;

            return root.GetProperty("response")[0].GetProperty("id").GetInt32();
        }

        public async Task<int> GetPostsCountAsync(GroupName groupName, CancellationToken token)
        {
            WallResponse response = await GetWallResponseAsync(groupName, 1, 0, token);

            return response.Response.Count;
        }

        public async Task<ImmutableArray<string>> GetImagesAsync(GroupName groupName, int postCount, int offset, CancellationToken cancellationToken)
        {
            var response = await GetWallResponseAsync(groupName, postCount, offset, cancellationToken);

            return ExtractPhotos(response);
        }

        public async Task<ImmutableArray<string>> GetImagesAsync(int ownerId, int count, int offset, CancellationToken cancellationToken)
        {
            var response = await GetWallResponseAsync(ownerId, count, offset, cancellationToken);

            return ExtractPhotos(response);
        }

        private static ImmutableArray<string> ExtractPhotos(WallResponse response)
        {
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

        private async Task<WallResponse> GetWallResponseAsync(GroupName groupName, int postCount, int offset, CancellationToken cancellationToken)
        {
            using var client = CreateClient();
            int ownerId = await GetOwnerIdAsync(groupName);

            return await GetWallResponseAsync(ownerId, postCount, offset, cancellationToken);
        }

        public async Task<WallResponse> GetWallResponseAsync(int ownerId, int postCount, int offset, CancellationToken cancellationToken)
        {
            using var client = CreateClient();
            string requestUri = await BuildUrl("wall", "get", new Dictionary<string, string>
            {
                {"owner_id", $"{-ownerId}"},
                {"v", "5.136"},
                {"count", $"{postCount}"},
                {"offset", $"{offset}"}
            });
            return await client.GetFromJsonAsync<WallResponse>(requestUri, cancellationToken);
        }
    }
}
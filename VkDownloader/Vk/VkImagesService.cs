using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using VkDownloader.Pages;

namespace VkDownloader.Vk
{
    public class VkImagesService
    {
        private const string BaseApiUrl = "https://api.vk.com/method/";
        private const string DefaultVkVersion = "5.142";


        private readonly VkCredentialsStorage _credentialsStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VkAuthLogic _authLogic;

        public VkImagesService(
            VkCredentialsStorage credentialsStorage,
            IHttpClientFactory httpClientFactory)
        {
            _credentialsStorage = credentialsStorage;
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("vk");

        private string BuildUrl(string apiType, string methodName, Dictionary<string, string> queryParams)
        {
            string baseQuery = $"{BaseApiUrl}{apiType}.{methodName}";
            string accessToken = _credentialsStorage.GetAccessToken() ?? throw new ArgumentNullException("access_token is missing");

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
            var response = await client.GetFromJsonAsync<JsonDocument>(BuildUrl("groups", "getById", new Dictionary<string, string>
            {
                {"group_id", name.Name}
            }));

            var firstResponse = response!.RootElement.GetProperty("response")[0];

            return firstResponse.GetProperty("id").GetInt32();
        }

        public async Task<ImmutableArray<string?>> GetImagesAsync(UserOrGroupName userOrGroupName, int postCount = 10, int offset = 0)
        {
            using var client = CreateClient();
            int ownerId = await GetOwnerIdAsync(userOrGroupName);

            var response = await client.GetFromJsonAsync<WallResponse>(BuildUrl("wall", "get", new Dictionary<string, string>
            {
                {"owner_id", $"{-ownerId}"},
                {"v", "5.136"},
                {"count", $"{postCount}"},
                {"offset", $"{offset}"}
            }));

            var attachments = response!.Response.Items.SelectMany(i => i.Attachments);
            var photos = attachments.Where(a => a.Type == "photo").Select(a => a.Photo);
            return photos
                .Select(p => p.Sizes.Last())
                .Select(s => s.Url)
                .ToImmutableArray()!;
        }
    }
}
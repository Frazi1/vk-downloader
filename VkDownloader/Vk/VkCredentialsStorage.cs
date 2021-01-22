using Microsoft.AspNetCore.Http;

namespace VkDownloader.Vk
{
    public class VkCredentialsStorage
    {
        private readonly ISession _session;
        public VkCredentialsStorage(ISession session) => _session = session;

        public void SetAccessToken(string token) => _session.SetString("vk_access_token", token);
        public string? GetAccessToken() => _session.GetString("vk_access_token");
    }
}
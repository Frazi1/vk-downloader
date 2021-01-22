using System;
using Microsoft.Extensions.Options;
using VkDownloader.Vk;

namespace VkDownloader.Pages
{
    public class VkAuthLogic
    {
        private readonly IOptions<VkSettings> _settings;

        public VkAuthLogic(IOptions<VkSettings> settings)
        {
            _settings = settings;
        }

        private static string GetCallbackUri()
        {
            return "http://localhost:5000/VkCallback";
        }

        public Uri BuildAuthDialogUri()
        {
            string authUri = $"https://oauth.vk.com/authorize?client_id={_settings.Value.AppId}&display=page&redirect_uri={GetCallbackUri()}&scope=groups&response_type=code&v=5.126";

            return new Uri(authUri);
        }

        public Uri BuildGetAccessTokenUri(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException(nameof(code));
            
            string accessTokenUri = $"https://oauth.vk.com/access_token?client_id={_settings.Value.AppId}&client_secret={_settings.Value.ClientSecret}&redirect_uri={GetCallbackUri()}&code={code}";

            return new Uri(accessTokenUri);
        }
    }
}
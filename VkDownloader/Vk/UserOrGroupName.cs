using System;

namespace VkDownloader.Vk
{
    public class UserOrGroupName
    {
        public string Name { get; }
        public UserOrGroupName(string fullLinkOrName)
        {
            Name = fullLinkOrName.Contains("/") ? new Uri(fullLinkOrName).Segments[^1] : fullLinkOrName;
        }
    }
}
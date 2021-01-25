using System;

namespace VkDownloader.Vk
{
    public class GroupName
    {
        public string Name { get; }
        public GroupName(string fullLinkOrName)
        {
            Name = fullLinkOrName.Contains("/") ? new Uri(fullLinkOrName).Segments[^1] : fullLinkOrName;
        }
    }
}
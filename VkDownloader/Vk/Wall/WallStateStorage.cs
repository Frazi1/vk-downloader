using System.Threading.Tasks;
using Blazored.SessionStorage;

namespace VkDownloader.Vk.Wall
{
    public class WallState
    {
        public string GroupName { get; set; }
        public int Offset { get; set; }

        public WallState(string groupName, int offset)
        {
            GroupName = groupName;
            Offset = offset;
        }
    }
    
    public class WallStateStorage
    {
        private readonly ISessionStorageService _sessionStorage;

        public WallStateStorage(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveLastViewedWall(WallState state)
        {
            await _sessionStorage.SetItemAsync("wall:state", state);
        }

        public Task<WallState?> GetLastViewedWall()
        {
            return _sessionStorage.GetItemAsync<WallState?>("wall:state");
        }
    }
}
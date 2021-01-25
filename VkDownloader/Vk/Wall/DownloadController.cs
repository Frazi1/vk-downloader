using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VkDownloader.Vk.Wall
{
    [Route("{controller}/{action}")]
    public class DownloadController : ControllerBase
    {
        private readonly VkImageDownloader _vkImageDownloader;
        private readonly ILogger<DownloadController> _logger;

        public DownloadController(VkImageDownloader vkImageDownloader,
            ILogger<DownloadController> logger)
        {
            _vkImageDownloader = vkImageDownloader;
            _logger = logger;
        }

        public async Task<IActionResult> DownloadImagesZip([FromQuery] string group, int offset, int count)
        {
            if (string.IsNullOrWhiteSpace(group)) return BadRequest("Group name must not be empty");
            if (offset < 0) return BadRequest("Offset must be positive or zero");

            int maxDownloadCount = _vkImageDownloader.GetMaxDownloadCount();
            if (count <= 0 || count > maxDownloadCount) return BadRequest($"Count value must be from {0} to {maxDownloadCount}");

            string filePath = await _vkImageDownloader.DownloadImagesToTempZipFile(new GroupName(@group), offset, count, CancellationToken.None);


            return new CallbackFileStreamResult(System.IO.File.OpenRead(filePath),
                "application/zip",
                () =>
                {
                    try
                    {
                        _logger.LogInformation("Deleting file {fileName}", filePath);
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("File {fileName} has been deleted", filePath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error deleting {fileName} file", filePath);
                    }
                });
        }
    }
}
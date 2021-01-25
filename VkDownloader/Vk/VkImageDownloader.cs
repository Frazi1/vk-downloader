using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VkDownloader.Vk
{
    public class VkImageDownloader
    {
        private readonly VkImagesService _service;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<VkImageDownloaderSettings> _settings;
        private readonly ILogger<VkImageDownloader> _logger;

        public VkImageDownloader(VkImagesService service,
            IHttpClientFactory clientFactory,
            IOptions<VkImageDownloaderSettings> settings,
            ILogger<VkImageDownloader> logger)
        {
            _service = service;
            _clientFactory = clientFactory;
            _settings = settings;
            _logger = logger;
        }

        public int GetMaxDownloadCount() => _settings.Value.MaxDownloadCount;
        public async Task<string> DownloadImagesToTempZipFile(GroupName groupName, int offset, int count, CancellationToken token)
        {
            int maxDownloadCount = GetMaxDownloadCount();
            if (count > maxDownloadCount)
            {
                throw new Exception($"Downloading more than {maxDownloadCount} is forbidden");
            }
            
            string zipFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _logger.LogInformation("Starting download process. Output file name: {outputFile}", zipFilePath);

            await DownloadImagesToTempZipFileCodeAsync(groupName, offset, count, token, zipFilePath);
            
            _logger.LogInformation("Download process for {outputFile} finished", zipFilePath);

            return zipFilePath;
        }

        private async Task DownloadImagesToTempZipFileCodeAsync(GroupName groupName, int offset, int count, CancellationToken token, string zipFilePath)
        {
            await using FileStream fileStream = File.Create(zipFilePath);
            await using ZipOutputStream outputStream = new(fileStream);

            var zipAccessLock = new SemaphoreSlim(1);
            using var client = _clientFactory.CreateClient();

            int filesDownloaded = 0;
            await foreach (ImmutableArray<string> batch in GetImageLinksInBatchesAsync(groupName, offset, count, token))
            {
                var tasks = batch.Select(async (url, index) =>
                    {
                        byte[] imageBytes = await client.GetByteArrayAsync(url, token);
                        await zipAccessLock.WaitAsync(token);

                        int imageIndex = Interlocked.Increment(ref filesDownloaded) + 1;
                        try
                        {
                            await WriteZipEntryAsync($"{imageIndex}.jpg", imageBytes, outputStream, token);
                        }
                        finally
                        {
                            zipAccessLock.Release();
                        }
                    })
                    .AsParallel()
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .WithCancellation(token);

                await Task.WhenAll(tasks);
            }

            outputStream.Finish();
        }

        private async IAsyncEnumerable<ImmutableArray<string>> GetImageLinksInBatchesAsync(GroupName groupName, int offset, int count, CancellationToken token)
        {
            int ownerId = await _service.GetOwnerIdAsync(groupName);
            int batchSize = 100;

            int currentOffset = offset;

            int totalBatchCount = count / batchSize + 1;

            _logger.LogInformation("Downloading {imageCount} images from {groupName} group starting at {downloadOffset} ({batchCount} batches)",
                count, groupName.Name, offset, totalBatchCount);
            
            for (int i = 0; i < totalBatchCount; i++)
            {
                var batch = await _service.GetImagesAsync(ownerId, batchSize, currentOffset += batchSize, token);
                
                _logger.LogInformation("Batch {batchIndex} from {groupName} downloaded", i, groupName.Name);
                yield return batch;
                await Task.Delay(TimeSpan.FromMilliseconds(1000), token);
            }
        }

        private static async Task WriteZipEntryAsync(string zippedFileName, byte[] input, ZipOutputStream outputStream, CancellationToken token)
        {
            ZipEntry entry = new(Path.GetFileName(zippedFileName))
            {
                DateTime = DateTime.UtcNow
            };

            outputStream.PutNextEntry(entry);


            await outputStream.WriteAsync(input, token);

            outputStream.CloseEntry();
        }
    }
}
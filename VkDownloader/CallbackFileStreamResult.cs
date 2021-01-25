using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace VkDownloader
{
    public class CallbackFileStreamResult : FileStreamResult
    {
        private readonly Action _callback;

        public CallbackFileStreamResult(Stream fileStream, string contentType, Action callback) : base(fileStream, contentType) => _callback = callback;

        public CallbackFileStreamResult(Stream fileStream, MediaTypeHeaderValue contentType, Action callback) : base(fileStream, contentType)
            => _callback = callback;

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                await base.ExecuteResultAsync(context);
            }
            finally
            {
                _callback();
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rocket.Chat.Net.Common
{
    internal class ProgressableStreamContent : HttpContent
    {
        private const int _defaultBufferSize = 5 * 4096;
        private readonly int _bufferSize;
        private readonly HttpContent _content;
        private readonly Action<long, long> _progress;
        public ProgressableStreamContent(HttpContent content, Action<long, long> progress) : this(content,
            _defaultBufferSize, progress)
        {
        }

        public ProgressableStreamContent(HttpContent content, int bufferSize, Action<long, long> progress)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this._content = content ?? throw new ArgumentNullException(nameof(content));
            this._bufferSize = bufferSize;
            this._progress = progress;

            foreach (var h in content.Headers)
            {
                Headers.Add(h.Key, h.Value);
            }
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(async () =>
            {
                var buffer = new byte[_bufferSize];
                TryComputeLength(out var size);
                var uploaded = 0;


                using var sinput = await _content.ReadAsStreamAsync();
                while (true)
                {
                    var length = sinput.Read(buffer, 0, buffer.Length);
                    if (length <= 0)
                    {
                        break;
                    }

                    //downloader.Uploaded = uploaded += length;
                    uploaded += length;
                    _progress?.Invoke(uploaded, size);

                    //System.Diagnostics.Debug.WriteLine($"Bytes sent {uploaded} of {size}");

                    stream.Write(buffer, 0, length);
                    stream.Flush();
                }

                stream.Flush();
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Headers.ContentLength.GetValueOrDefault();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _content.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
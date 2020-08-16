using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Represents a reader for a MJPEG stream
    /// </summary>
    public sealed class MjpegStreamReader : IDisposable
    {
        #region Private
        private const byte CarriageReturn = 13;
        private const byte LineFeed = 10;
        private const int SeparatorLength = 4;
        private readonly byte[] separatorBytes = new byte[SeparatorLength] { CarriageReturn, LineFeed, CarriageReturn, LineFeed };
        private const int HeadBufferSize = 100;
        private const int MaxBufferOffset = HeadBufferSize - SeparatorLength;
        private readonly byte[] header = new byte[HeadBufferSize];
        private readonly Regex contentRegex = new Regex(@"Content-Length:\s?(?<length>[0-9]+)\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly BufferedStream reader;
        private bool isDisposed;
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MjpegStreamReader"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        public MjpegStreamReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            reader = new BufferedStream(stream);
        }
        #endregion

        /************************************************************************/

        #region Events
        /// <summary>
        /// Occurs when a new frame is received.
        /// </summary>
        public event EventHandler<RawJpegFrame> FrameReceived;
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Asynchonously begins receiving data.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>A task that represents the asynchronous receive operation.</returns>
        public async Task ReceiveAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                ArraySegment<byte> data = await GetNextPartAsync(token).ConfigureAwait(false);
                if (data != null)
                {
                    FrameReceived?.Invoke(this, new RawJpegFrame(DateTime.Now, data));
                }
            }
        }
        #endregion

        /************************************************************************/

        #region IDisposable
        /// <summary>
        /// Disposes of the stream.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                reader?.Dispose();
            }

            isDisposed = true;
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private async Task<ArraySegment<byte>> GetNextPartAsync(CancellationToken token)
        {
            try
            {
                int length = await GetContentLengthAsync(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (length == 0) return null;

                byte[] result = new byte[length];
                await ReadBufferedBytesAsync(result, 0, length, token).ConfigureAwait(false);
                return new ArraySegment<byte>(result);
            }
            catch
            {
                return null;
            }
        }

        private async Task<int> GetContentLengthAsync(CancellationToken token)
        {
            await ReadBufferedBytesAsync(header, 0, 4, token).ConfigureAwait(false);

            int offset = 4;

            for (int k = 0; k < MaxBufferOffset; k++)
            {
                if (HaveSeparatorBytes(k)) break;
                await ReadBufferedBytesAsync(header, offset, 1, token).ConfigureAwait(false);
                offset++;
            }

            string headerStr = Encoding.UTF8.GetString(header, 0, offset);
            Match m = contentRegex.Match(headerStr);
            return int.TryParse(m.Groups["length"].Value, out int result) ? result : 0;
        }

        private async Task<int> ReadBufferedBytesAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                bytesRead += await reader.ReadAsync(buffer, offset + bytesRead, count - bytesRead, token).ConfigureAwait(false);
                if (bytesRead < count)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
            return bytesRead;
        }

        private bool HaveSeparatorBytes(int pos)
        {
            for (int i = pos, j = 0; j < SeparatorLength; i++, j++)
            {
                if (header[i] != separatorBytes[j]) return false;
            }
            return true;
        }
        #endregion
    }
}
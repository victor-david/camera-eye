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
        // Specs say that the body of each part and it's header are separated by two CRLFs
        private readonly byte[] separatorBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
        private readonly byte[] headerBytes = new byte[100];
        private readonly Regex contentRegex = new Regex(@"Content-Length:\s?(?<length>[0-9]+)\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly BinaryReader reader;
        private bool isDisposed;
        #endregion

        /************************************************************************/

        #region Constructor
        public MjpegStreamReader(Stream stream)
        {
            reader = new BinaryReader(new BufferedStream(stream));
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
        /// <returns>The task</returns>
        public async Task ReceiveAsync(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                byte[] data = await GetNextPartAsync().ConfigureAwait(false);
                if (data != null)
                {
                    FrameReceived?.Invoke(this, new RawJpegFrame(DateTime.Now, new ArraySegment<byte>(data)));
                }
            }
        }
        #endregion

        /************************************************************************/

        #region IDisposable
        /// <summary>
        /// Dispoases.
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
                // free managed resources
                reader?.Dispose();
            }

            isDisposed = true;
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private Task<byte[]> GetNextPartAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // every part has it's own headers
                    string headerSection = ReadContentHeaderSection(reader);
                    // let's parse the header section for the content-length
                    int length = GetPartLength(headerSection);
                    // now let's get the image
                    return reader.ReadBytes(length);
                }
                catch
                {
                    return null;
                }
            });
        }

        private string ReadContentHeaderSection(BinaryReader stream)
        {
            // headers and content in multi part are separated by two \r\n
            bool found;

            int count = 4;
            stream.Read(headerBytes, 0, 4);
            for (int i = 0; i < headerBytes.Length; i++)
            {
                found = SeparatorBytesExistsInArray(i, headerBytes);
                if (!found)
                {
                    headerBytes[count] = stream.ReadByte();
                    count++;
                }
                else
                    break;
            }
            return Encoding.UTF8.GetString(headerBytes, 0, count);
        }

        private int GetPartLength(string headerSection)
        {
            Match m = contentRegex.Match(headerSection);
            return int.Parse(m.Groups["length"].Value);
        }

        private bool SeparatorBytesExistsInArray(int position, byte[] array)
        {
            bool result = false;
            for (int i = position, j = 0; j < separatorBytes.Length; i++, j++)
            {
                result = array[i] == separatorBytes[j];
                if (!result)
                {
                    break;
                }
            }
            return result;
        }
        #endregion
    }
}
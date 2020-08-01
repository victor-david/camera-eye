using System;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Represents a reader for a <see cref="MultiPartStream"/>.
    /// </summary>
    public class MultiPartReader
    {
        #region Private
        private readonly MultiPartStream stream;
        private bool isReading = false;
        private bool isStreamClosed = false;
        #endregion

        /************************************************************************/

        #region Constructor
        public MultiPartReader(MultiPartStream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }
        #endregion

        /************************************************************************/

        #region Public methods
        public async void StartProcessing()
        {
            try
            {
                isReading = true;
                while (isReading)
                {
                    OnPartReady(await stream.NextPartAsync().ConfigureAwait(false));
                }
            }
            catch { }
            finally
            {
                stream.Close();
                isStreamClosed = true;
            }
        }

        public async Task StopProcessing()
        {
            isReading = false;
            while (!isStreamClosed)
            {
                await Task.Delay(25);
            }
        }
        #endregion

        /************************************************************************/

        #region Events
        public event EventHandler<byte[]> PartReady;

        protected virtual void OnPartReady(byte[] currentPart)
        {
            if (currentPart != null)
            {
                PartReady?.Invoke(this, currentPart);
            }
        }
        #endregion
    }
}
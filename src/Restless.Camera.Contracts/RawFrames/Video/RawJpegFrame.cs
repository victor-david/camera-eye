using System;

namespace Restless.Camera.Contracts.RawFrames.Video
{
    /// <summary>
    /// Represents a raw jpeg frame.
    /// </summary>
    public class RawJpegFrame : RawVideoFrame
    {
        public static readonly byte[] StartMarkerBytes = { 0xFF, 0xD8 };
        public static readonly byte[] EndMarkerBytes = { 0xFF, 0xD9 };

        /// <summary>
        /// Initializes a new instance of the <see cref="RawJpegFrame"/> class.
        /// </summary>
        /// <param name="timestamp">Frame timestamp.</param>
        /// <param name="segment">Frame segment.</param>
        public RawJpegFrame(DateTime timestamp, ArraySegment<byte> segment) : base(timestamp, segment)
        {
        }
    }
}

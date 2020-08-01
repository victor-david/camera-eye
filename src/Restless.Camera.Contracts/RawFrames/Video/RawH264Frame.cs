using System;

namespace Restless.Camera.Contracts.RawFrames.Video
{
    /// <summary>
    /// Represents a raw H264 frame. This class must be inherited.
    /// </summary>
    public abstract class RawH264Frame : RawVideoFrame
    {
        public static readonly byte[] StartMarker = { 0, 0, 0, 1 };

        protected RawH264Frame(DateTime timestamp, ArraySegment<byte> frameSegment) : base(timestamp, frameSegment)
        {
        }
    }
}

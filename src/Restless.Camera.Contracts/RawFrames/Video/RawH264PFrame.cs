using System;

namespace Restless.Camera.Contracts.RawFrames.Video
{
    /// <summary>
    /// Represents a raw H264P frame.
    /// </summary>
    public class RawH264PFrame : RawH264Frame
    {
        public RawH264PFrame(DateTime timestamp, ArraySegment<byte> frameSegment) : base(timestamp, frameSegment)
        {
        }
    }
}

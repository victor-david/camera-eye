using System;

namespace Restless.Camera.Contracts.RawFrames.Video
{
    /// <summary>
    /// Represents a raw H264I frame.
    /// </summary>
    public class RawH264IFrame : RawH264Frame
    {
        public ArraySegment<byte> SpsPpsSegment { get; }

        public RawH264IFrame(DateTime timestamp, ArraySegment<byte> frameSegment, ArraySegment<byte> spsPpsSegment)
            : base(timestamp, frameSegment)
        {
            SpsPpsSegment = spsPpsSegment;
        }
    }
}
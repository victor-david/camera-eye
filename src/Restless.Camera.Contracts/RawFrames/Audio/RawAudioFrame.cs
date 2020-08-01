using System;

namespace Restless.Camera.Contracts.RawFrames.Audio
{
    /// <summary>
    /// Represents a raw audio frame. This class must be inherited.
    /// </summary>
    public abstract class RawAudioFrame : RawFrame
    {
        /// <summary>
        /// Gets the frame type (audio)
        /// </summary>
        public override FrameType Type => FrameType.Audio;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawAudioFrame"/> class.
        /// </summary>
        /// <param name="timestamp">Frame timestamp.</param>
        /// <param name="segment">Frame segment.</param>
        protected RawAudioFrame(DateTime timestamp, ArraySegment<byte> segment) : base(timestamp, segment)
        {
        }
    }
}
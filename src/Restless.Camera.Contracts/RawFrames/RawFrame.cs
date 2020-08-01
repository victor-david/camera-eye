using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Represents a raw frame. This class must be inherited.
    /// </summary>
    public abstract class RawFrame
    {
        #region Properties
        /// <summary>
        /// Gets the timestamp of the frame.
        /// </summary>
        public DateTime Timestamp { get; }
        /// <summary>
        /// Gets the segment data of the frame.
        /// </summary>
        public ArraySegment<byte> Segment { get; }
        /// <summary>
        /// Gets the type of frame.
        /// </summary>
        public abstract FrameType Type { get; }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RawFrame"/> class.
        /// </summary>
        /// <param name="timestamp">Frame timestamp.</param>
        /// <param name="segment">Frame segment.</param>
        protected RawFrame(DateTime timestamp, ArraySegment<byte> segment)
        {
            Timestamp = timestamp;
            Segment = segment;
        }
        #endregion
    }
}
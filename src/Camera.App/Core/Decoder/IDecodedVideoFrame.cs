using System;

namespace Restless.App.Camera.Core
{
    public interface IDecodedVideoFrame
    {
        /// <summary>
        /// Gets the decoded video frame parameters.
        /// </summary>
        DecodedVideoFrameParameters FrameParms { get; }
        void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParms);
    }
}
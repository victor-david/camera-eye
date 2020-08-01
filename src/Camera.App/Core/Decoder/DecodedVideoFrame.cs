using System;

namespace Restless.App.Camera.Core
{
    public class DecodedVideoFrame : IDecodedVideoFrame
    {
        private readonly Action<IntPtr, int, TransformParameters> transformAction;
        // Action<IntPtr, int,TransformParameters> transformAction

        /// <summary>
        /// Gets the decoded video frame parameters.
        /// </summary>
        public DecodedVideoFrameParameters FrameParms 
        {
            get;
        }

        public DecodedVideoFrame(Action<IntPtr, int, TransformParameters> transformAction, DecodedVideoFrameParameters frameParms)
        {
            this.transformAction = transformAction ?? throw new ArgumentNullException(nameof(transformAction));
            FrameParms = frameParms ?? throw new ArgumentNullException(nameof(frameParms));
        }

        public void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParms)
        {
            transformAction(buffer, bufferStride, transformParms);
        }
    }
}
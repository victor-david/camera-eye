using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Restless.App.Camera.Core
{
    public class VideoDecoder : IDisposable
    {
        private readonly IntPtr decoderHandle;
        private readonly VideoCodecId videoCodecId;

        private DecodedVideoFrameParameters currentFrameParms;
        private readonly Dictionary<TransformParameters, DecodedVideoScaler> scalersMap;

        // Lock object to ensure this object remains disposed (or not) for the duration of a action
        private readonly object disposalLock = new object();

        private byte[] extraData = new byte[0];
        private bool disposed;

        private VideoDecoder(VideoCodecId videoCodecId, IntPtr decoderHandle)
        {
            this.videoCodecId = videoCodecId;
            this.decoderHandle = decoderHandle;
            currentFrameParms = new DecodedVideoFrameParameters(0, 0, FFmpegPixelFormat.None);
            scalersMap = new Dictionary<TransformParameters, DecodedVideoScaler>();
        }

        ~VideoDecoder()
        {
            Dispose();
        }

        public static VideoDecoder CreateDecoder(VideoCodecId videoCodecId)
        {
            int resultCode = FFmpegVideoPInvoke.CreateVideoDecoder(videoCodecId, out IntPtr decoderPtr);

            if (resultCode != 0)
            {
                throw new DecoderException($"An error occurred while creating video decoder for {videoCodecId} codec, code: {resultCode}");
            }

            return new VideoDecoder(videoCodecId, decoderPtr);
        }

        public unsafe IDecodedVideoFrame TryDecode(RawVideoFrame frame)
        {
            fixed (byte* rawBufferPtr = &frame.Segment.Array[frame.Segment.Offset])
            {
                int resultCode;

                if (frame is RawH264IFrame rawH264IFrame)
                {
                    if (rawH264IFrame.SpsPpsSegment.Array != null && !extraData.SequenceEqual(rawH264IFrame.SpsPpsSegment))
                    {
                        if (extraData.Length != rawH264IFrame.SpsPpsSegment.Count)
                        {
                            extraData = new byte[rawH264IFrame.SpsPpsSegment.Count];
                        }

                        Buffer.BlockCopy(rawH264IFrame.SpsPpsSegment.Array, rawH264IFrame.SpsPpsSegment.Offset,
                            extraData, 0, rawH264IFrame.SpsPpsSegment.Count);

                        fixed (byte* initDataPtr = &extraData[0])
                        {
                            resultCode = FFmpegVideoPInvoke.SetVideoDecoderExtraData(decoderHandle, (IntPtr)initDataPtr, extraData.Length);

                            if (resultCode != 0)
                            {
                                throw new DecoderException($"An error occurred while setting video extra data, {videoCodecId} codec, code: {resultCode}");
                            }
                        }
                    }
                }

                lock (disposalLock)
                {
                    if (disposed)
                    {
                        return null;
                    }

                    resultCode = 
                        FFmpegVideoPInvoke.DecodeFrame(decoderHandle, (IntPtr)rawBufferPtr, frame.Segment.Count, out int width, out int height, out FFmpegPixelFormat pixelFormat);

                    if (resultCode != 0)
                    {
                        return null;
                    }


                    if (!currentFrameParms.Equals(width, height, pixelFormat))
                    {
                        currentFrameParms = new DecodedVideoFrameParameters(width, height, pixelFormat);
                        DropAllVideoScalers();
                    }
                }

                return new DecodedVideoFrame(TransformTo, currentFrameParms);
            }
        }

        public void Dispose()
        {
            lock (disposalLock) 
            {
                if (disposed) return;
                disposed = true;
                FFmpegVideoPInvoke.RemoveVideoDecoder(decoderHandle);
                DropAllVideoScalers();
                GC.SuppressFinalize(this);
            }
        }

        private void DropAllVideoScalers()
        {
            foreach (var scaler in scalersMap.Values)
            {
                scaler.Dispose();
            }
            scalersMap.Clear();
        }

        private void TransformTo(IntPtr buffer, int bufferStride, TransformParameters parameters)
        {
            if (!scalersMap.TryGetValue(parameters, out DecodedVideoScaler videoScaler))
            {
                videoScaler = DecodedVideoScaler.Create(currentFrameParms, parameters);
                scalersMap.Add(parameters, videoScaler);
            }

            lock (disposalLock)
            {
                if (disposed)
                {
                    return;
                }

                int resultCode = FFmpegVideoPInvoke.ScaleDecodedVideoFrame(decoderHandle, videoScaler.Handle, buffer, bufferStride);

                if (resultCode != 0)
                {
                    throw new DecoderException($"An error occurred while converting decoding video frame, {videoCodecId} codec, code: {resultCode}");
                }
            }
        }
    }
}
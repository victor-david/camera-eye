using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Manages the video decoders
    /// </summary>
    public class VideoDecoderManager
    {
        #region Private
        private readonly Dictionary<VideoCodecId, VideoDecoder> videoDecodersMap;
        #endregion

        #region Constructors
        public VideoDecoderManager()
        {
            videoDecodersMap = new Dictionary<VideoCodecId, VideoDecoder>();
        }
        #endregion


        public IDecodedVideoFrame Decode(RawVideoFrame frame)
        {
            VideoDecoder decoder = GetDecoderForFrame(frame);
            return decoder?.TryDecode(frame);
        }

        private VideoDecoder GetDecoderForFrame(RawVideoFrame videoFrame)
        {
            VideoCodecId codecId = DetectCodecId(videoFrame);
            if (!videoDecodersMap.TryGetValue(codecId, out VideoDecoder decoder))
            {
                decoder = VideoDecoder.CreateDecoder(codecId);
                videoDecodersMap.Add(codecId, decoder);
            }

            return decoder;
        }

        private VideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame) return VideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame) return VideoCodecId.H264;
            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }
    }
}
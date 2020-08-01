using System;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    public class DecodedVideoScaler : IDisposable
    {
        private const double MaxAspectRatioError = 0.1;
        private bool disposed;

        public IntPtr Handle { get; }
        public int ScaledWidth { get; }
        public int ScaledHeight { get; }
        public PixelFormat ScaledPixelFormat { get; }

        private DecodedVideoScaler(IntPtr handle, int scaledWidth, int scaledHeight, PixelFormat scaledPixelFormat)
        {
            Handle = handle;
            ScaledWidth = scaledWidth;
            ScaledHeight = scaledHeight;
            ScaledPixelFormat = scaledPixelFormat;
        }

        ~DecodedVideoScaler()
        {
            Dispose();
        }

        /// <exception cref="DecoderException"></exception>
        public static DecodedVideoScaler Create(DecodedVideoFrameParameters decodedVideoFrameParms, TransformParameters transformParms)
        {
            if (decodedVideoFrameParms == null) throw new ArgumentNullException(nameof(decodedVideoFrameParms));
            if (transformParms == null) throw new ArgumentNullException(nameof(transformParms));

            int sourceLeft = 0;
            int sourceTop = 0;
            int sourceWidth = decodedVideoFrameParms.Width;
            int sourceHeight = decodedVideoFrameParms.Height;
            int scaledWidth = decodedVideoFrameParms.Width;
            int scaledHeight = decodedVideoFrameParms.Height;

            if (!transformParms.Region.IsEmpty)
            {
                sourceLeft = transformParms.Region.Left;
                sourceTop = transformParms.Region.Top;
                sourceWidth = transformParms.Region.Width;
                sourceHeight = transformParms.Region.Height;
            }

            if (!transformParms.TargetSize.IsEmpty)
            {
                scaledWidth = transformParms.TargetSize.Width;
                scaledHeight = transformParms.TargetSize.Height;

                ScalingPolicy scalingPolicy = transformParms.ScalePolicy;

                float srcAspectRatio = (float) sourceWidth / sourceHeight;
                float destAspectRatio = (float) scaledWidth / scaledHeight;

                if (scalingPolicy == ScalingPolicy.Auto)
                {
                    float relativeChange = Math.Abs(srcAspectRatio - destAspectRatio) / srcAspectRatio;

                    scalingPolicy = relativeChange > MaxAspectRatioError
                        ? ScalingPolicy.RespectAspectRatio
                        : ScalingPolicy.Stretch;
                }

                if (scalingPolicy == ScalingPolicy.RespectAspectRatio)
                {
                    if (destAspectRatio < srcAspectRatio)
                        scaledHeight = sourceHeight * scaledWidth / sourceWidth;
                    else
                        scaledWidth = sourceWidth * scaledHeight / sourceHeight;
                }
            }

            FFmpegPixelFormat pixelFormat = ToFFmpegPixelFormat(transformParms.PixelFormat);
            FFmpegScalingQuality scaleQuality = ToFFmpegScaleQuality(transformParms.ScaleQuality);

            int resultCode = FFmpegVideoPInvoke.CreateVideoScaler
                (
                    sourceLeft, sourceTop, sourceWidth, sourceHeight,
                    decodedVideoFrameParms.PixelFormat,
                    scaledWidth, scaledHeight, 
                    pixelFormat, scaleQuality, 
                    out var handle
                );

            if (resultCode != 0)
            {
                throw new DecoderException(@"An error occurred while creating scaler, code: {resultCode}");
            }

            return new DecodedVideoScaler(handle, scaledWidth, scaledHeight, transformParms.PixelFormat);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            FFmpegVideoPInvoke.RemoveVideoScaler(Handle);
            GC.SuppressFinalize(this);
        }

        private static FFmpegScalingQuality ToFFmpegScaleQuality(ScalingQuality scalingQuality)
        {
            return scalingQuality switch
            {
                ScalingQuality.Nearest => FFmpegScalingQuality.Point,
                ScalingQuality.Bilinear => FFmpegScalingQuality.Bilinear,
                ScalingQuality.FastBilinear => FFmpegScalingQuality.FastBilinear,
                ScalingQuality.Bicubic => FFmpegScalingQuality.Bicubic,
                _ => throw new ArgumentOutOfRangeException(nameof(scalingQuality))
            };
        }

        private static FFmpegPixelFormat ToFFmpegPixelFormat(PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormats.Pbgra32) return FFmpegPixelFormat.BGRA;
            if (pixelFormat == PixelFormats.Gray8) return FFmpegPixelFormat.GRAY8;
            if (pixelFormat == PixelFormats.Bgr24) return FFmpegPixelFormat.BGR24;
            throw new ArgumentOutOfRangeException(nameof(pixelFormat));
        }
    }
}
namespace Restless.App.Camera.Core
{
    public class DecodedVideoFrameParameters
    {
        public int Width { get; }

        public int Height { get; }

        public FFmpegPixelFormat PixelFormat { get; }

        public DecodedVideoFrameParameters(int width, int height, FFmpegPixelFormat pixelFormat)
        {
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
        }

        public bool Equals(int width, int height, FFmpegPixelFormat pixelFormat)
        {
            return Width == width && Height == height && PixelFormat == pixelFormat;
        }

        protected bool Equals(DecodedVideoFrameParameters other)
        {
            return Width == other.Width && Height == other.Height && PixelFormat == other.PixelFormat;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DecodedVideoFrameParameters) obj);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Width, Height, PixelFormat);
        }

        public override string ToString()
        {
            return $"Size {Width}x{Height}, Pixel: {PixelFormat}";
        }
    }
}
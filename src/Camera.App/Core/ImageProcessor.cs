using Restless.Camera.Contracts;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using Imaging = System.Drawing.Imaging;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Static helper class to provide image processing
    /// </summary>
    public static class ImageProcessor
    {
        #region Methods
        /// <summary>
        /// Creates a bitmap from a byte array.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>A bitmap</returns>
        public static Bitmap ByteArrayToBitmap(byte[] bytes, Orientation orientation)
        {
            try
            {
                if (bytes == null) return null;

                using MemoryStream memoryStream = new MemoryStream(bytes);
                Bitmap img = (Bitmap)Image.FromStream(memoryStream);
                switch (orientation)
                {
                    case Orientation.Mirror:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case Orientation.Flip:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case Orientation.FlipAndMirror:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                        break;
                }
                return img;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an imaging pixel format to a media pixel format.
        /// </summary>
        /// <param name="pixelFormat">The imaging pixel format.</param>
        /// <returns>Corresponding media pixel format</returns>
        /// <exception cref="NotSupportedException">The pixel format is not supported.</exception>
        /// <remarks>
        /// https://github.com/mathieumack/PixelFormatsConverter
        /// </remarks>
        public static System.Windows.Media.PixelFormat Convert(this Imaging.PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                Imaging.PixelFormat.Format16bppGrayScale => PixelFormats.Gray16,
                Imaging.PixelFormat.Format16bppRgb555 => PixelFormats.Bgr555,
                Imaging.PixelFormat.Format16bppRgb565 => PixelFormats.Bgr565,
                Imaging.PixelFormat.Indexed => PixelFormats.Bgr101010,
                Imaging.PixelFormat.Format1bppIndexed => PixelFormats.Indexed1,
                Imaging.PixelFormat.Format4bppIndexed => PixelFormats.Indexed4,
                Imaging.PixelFormat.Format8bppIndexed => PixelFormats.Indexed8,
                Imaging.PixelFormat.Format24bppRgb => PixelFormats.Bgr24,
                Imaging.PixelFormat.Format32bppArgb => PixelFormats.Bgr32,
                Imaging.PixelFormat.Format32bppPArgb => PixelFormats.Pbgra32,
                Imaging.PixelFormat.Format32bppRgb => PixelFormats.Bgr32,
                Imaging.PixelFormat.Format48bppRgb => PixelFormats.Rgb48,
                Imaging.PixelFormat.Format64bppArgb => PixelFormats.Prgba64,
                _ => throw new NotSupportedException($"{pixelFormat}: Convertion not supported")
            };
        }
        #endregion
    }
}
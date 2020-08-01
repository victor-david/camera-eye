using System.Drawing;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    public class TransformParameters
    {
        #region Properties
        /// <summary>
        /// Gets the target frame size
        /// </summary>
        public Size TargetSize { get; }

        /// <summary>
        /// Gets the target pixel format
        /// </summary>
        public PixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets the scaling policy.
        /// </summary>
        public ScalingPolicy ScalePolicy { get; }

        /// <summary>
        /// Gets the scaling quality.
        /// </summary>
        public ScalingQuality ScaleQuality { get; }

        /// <summary>
        /// Gets the region within <see cref="TargetSize"/> to extract, or empty rectangle for all.
        /// </summary>
        public Rectangle Region { get; }
        #endregion

        /************************************************************************/

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformParameters"/> class.
        /// </summary>
        /// <param name="targetSize">The target size.</param>
        /// <param name="pixelFormat">The pixel format. Only PixelFormats.Pbgra32, Bgr24, and Gray8 are supported.</param>
        /// <param name="scalePolicy">Scale policy.</param>
        /// <param name="scaleQuality">Scale quality.</param>
        /// <param name="region">Region of interest, or empty rectangle for all.</param>
        public TransformParameters(Size targetSize, PixelFormat pixelFormat, ScalingPolicy scalePolicy, ScalingQuality scaleQuality, Rectangle region)
        {
            TargetSize = targetSize;
            PixelFormat = pixelFormat;
            ScalePolicy = scalePolicy;
            ScaleQuality = scaleQuality;
            Region = region;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformParameters"/> class using default values 
        /// for <see cref="ScalePolicy"/>, <see cref="ScaleQuality"/>, and <see cref="Region"/>
        /// </summary>
        /// <param name="targetSize">The target size</param>
        /// <param name="pixelFormat">Pixel format. Only PixelFormats.Pbgra32, Bgr24, and Gray8 are supported.</param>
        public TransformParameters(Size targetSize, PixelFormat pixelFormat) 
            : this(targetSize, pixelFormat, ScalingPolicy.RespectAspectRatio, ScalingQuality.FastBilinear, Rectangle.Empty)
        {
        }
        #endregion

        /************************************************************************/

        #region Methods
        protected bool Equals(TransformParameters other)
        {
            return 
                Region.Equals(other.Region) &&
                TargetSize.Equals(other.TargetSize) &&
                PixelFormat == other.PixelFormat && 
                ScaleQuality == other.ScaleQuality;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TransformParameters) obj);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Region, TargetSize, PixelFormat, ScaleQuality);
        }
        #endregion
    }
}
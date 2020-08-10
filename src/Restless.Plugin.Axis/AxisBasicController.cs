using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System.ComponentModel.Composition;

namespace Restless.Plugin.Axis
{
    [Export(typeof(ICameraPluginCreator))]
    public class AxisBasicCreator : PluginCreatorBase, ICameraPluginCreator
    {
        /// <summary>
        /// Gets the display name for the plugin.
        /// </summary>
        public string DisplayName => "Axis Basic MJPEG Camera Viewer";
        /// <summary>
        /// Gets the description for the plugin.
        /// </summary>
        public string Description => "Provides basic camera viewing services for Axis IP cameras";
        /// <summary>
        /// Gets a unique id for the plugin.
        /// </summary>
        public string UniqueId => "1664E015-5C00-4765-9530-351E6154C0DA";
        /// <summary>
        /// Creates an instance of the plugin.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        /// <returns>An instance of <see cref="AxisBasicController"/></returns>
        public ICameraPlugin Create(ConnectionParameters parms)
        {
            return new AxisBasicController(parms);
        }
    }

    public class AxisBasicController : MjpegPluginBase, ICameraPlugin
    {
        #region Public properties
        #endregion

        /************************************************************************/

        #region Constructor (internal)
        internal AxisBasicController(ConnectionParameters parms) : base(parms)
        {
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Http, "mjpg/video.mjpg", "MJPEG Stream"));
        }
        #endregion
    }
}
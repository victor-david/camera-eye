using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using System;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides static methods to create a camera plugin
    /// </summary>
    public static class PluginFactory
    {
        /// <summary>
        /// Creates an <see cref="ICameraPlugin"/> for the specified camera.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <returns>The ICameraPlugin</returns>
        /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
        /// <exception cref="InvalidPluginException">The camera does not have a plugin assigned.</exception>
        /// <exception cref="InvalidPluginException">The plugin cannot be found.</exception>
        public static ICameraPlugin Create(CameraRow camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));

            if (camera.PluginId == PluginTable.Defs.Values.NullPluginId)
            {
                throw new InvalidPluginException("The selected camera does not have an associated plugin.");
            }

            ICameraPluginCreator creator = CompositionManager.Instance.GetCameraPluginCreator(camera.PluginGuid);
            if (creator == null)
            {
                throw new InvalidPluginException($"The plugin configured for the camera [{camera.PluginName}, Id: {camera.PluginGuid} cannot be found.");
            }

            return  creator.Create(new ConnectionParameters(camera.IpAddress, camera.Port, camera.UserId, camera.Password));
        }
    }
}
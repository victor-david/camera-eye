using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System;
using System.Collections.Generic;

namespace Restless.App.Camera
{
    public class CameraConfigViewModel : ApplicationViewModel
    {
        #region Private
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the camera row object.
        /// </summary>
        public CameraRow Camera
        {
            get;
        }

        /// <summary>
        /// Gets an enumerable of discovered plugins.
        /// </summary>
        public IEnumerable<PluginRow> CameraPlugins => DatabaseController.Instance.GetTable<PluginTable>().EnumeratePlugins();

        #endregion

        /************************************************************************/

        #region Constructor
        public CameraConfigViewModel(CameraRow camera)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));
            DisplayName = Camera.Name;
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        #endregion

        /************************************************************************/

        #region Private methods
        //private void RunOpenCameraCommand(object parm)
        //{
        //    if (Camera == null) return;
        //    try
        //    {
        //        /* This throws if the plugin is not specified (null plugin) or the plugin itself cannot be found */
        //        var window = WindowFactory.Camera.Create(Camera);
        //        window.Show();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorText = ex.Message;
        //        IsError = true;
        //    }
        //}


        #endregion
    }
}
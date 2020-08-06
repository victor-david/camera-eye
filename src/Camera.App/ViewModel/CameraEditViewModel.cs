using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Restless.App.Camera
{
    public class CameraEditViewModel : ApplicationViewModel
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
        public CameraEditViewModel(CameraRow camera)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));
            DisplayName = Camera.Name;
            Commands.Add("ChangeStatusBanner", RelayCommand.Create(RunChangeStatusBannerCommand));
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        #endregion

        /************************************************************************/

        #region Private methods
        private void RunChangeStatusBannerCommand(object parm)
        {
            if (parm is CameraFlags flag)
            {
                Camera.ChangeFlags(flag, StatusFlagsToRemove(flag));
            }
        }

        private CameraFlags StatusFlagsToRemove(CameraFlags flag)
        {
            return flag switch
            {
                CameraFlags.StatusTop => CameraFlags.StatusBottom,
                CameraFlags.StatusBottom => CameraFlags.StatusTop,
                _ => CameraFlags.StatusTop | CameraFlags.StatusBottom,
            };
        }
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
using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Controls;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;

namespace Restless.App.Camera
{
    public class CameraConfigViewModel : DataGridViewModel<CameraTable>
    {
        #region Private
        private CameraRow camera;
        private bool isError;
        private string errorText;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the selected camera row object.
        /// </summary>
        public CameraRow Camera
        {
            get => camera;
            private set => SetProperty(ref camera, value);
        }

        /// <summary>
        /// Gets an enumerable of discovered plugins.
        /// </summary>
        public IEnumerable<PluginRow> CameraPlugins => DatabaseController.Instance.GetTable<PluginTable>().EnumeratePlugins();

        /// <summary>
        /// Gets a boolean value that indicates if an error has occured.
        /// </summary>
        public bool IsError
        {
            get => isError;
            private set => SetProperty(ref isError, value);
        }

        public string ErrorText
        {
            get => errorText;
            private set
            {
                SetProperty(ref errorText, value);
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public CameraConfigViewModel()
        {
            DisplayName = "Registered Cameras";
            Columns.Create("Name", CameraTable.Defs.Columns.Name).MakeFixedWidth(160);
            Columns.Create("Description", CameraTable.Defs.Columns.Description);
            Columns.Create("Ip Address", CameraTable.Defs.Columns.IpAddress).MakeFixedWidth(110);
            Columns.Create("Port", CameraTable.Defs.Columns.Port);

            /* defined in commands to expose a binding source (double click command) */
            Commands.Add("Open", RunOpenCameraCommand, CanRunIfIsItemSelected);

            MenuItems.AddItem("Open (or double click the row)", Commands["Open"]).AddImageResource("ImageCameraMenu");
            MenuItems.AddSeparator();
            MenuItems.AddItem("Add camera", RelayCommand.Create(RunCreateCameraCommand)).AddImageResource("ImageAddMenu");
            MenuItems.AddSeparator();
            MenuItems.AddItem("Remove camera", RelayCommand.Create(RunRemoveCameraCommand, CanRunIfIsItemSelected)).AddImageResource("ImageRedXMenu");
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        protected override void OnSelectedItemChanged()
        {
            Camera = CameraRow.Create(SelectedDataRow);
            IsError = false;
        }

        protected override int OnDataRowCompare(DataRow item1, DataRow item2)
        {
            return DataRowCompareString(item1, item2, CameraTable.Defs.Columns.Name);
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void RunOpenCameraCommand(object parm)
        {
            if (Camera == null) return;
            try
            {
                /* This throws if the plugin is not specified (null plugin) or the plugin itself cannot be found */
                var window = WindowFactory.Camera.Create(Camera);
                window.Show();
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                IsError = true;
            }
        }

        private void RunCreateCameraCommand(object parm)
        {
            Table.Add();
            Refresh();
        }

        private void RunRemoveCameraCommand(object parm)
        {
            if (Messages.ShowYesNo("Remove camera from list?"))
            {
                SelectedDataRow?.Delete();
                Refresh();
            }
        }
        #endregion
    }
}
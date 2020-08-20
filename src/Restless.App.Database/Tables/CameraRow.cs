using Restless.App.Database.Core;
using Restless.Tools.Database.SQLite;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using Columns = Restless.App.Database.Tables.CameraTable.Defs.Columns;

namespace Restless.App.Database.Tables
{
    /// <summary>
    /// Represents database data about a single camera
    /// </summary>
    public class CameraRow : RowObjectBase<CameraTable>, INotifyPropertyChanged
    {
        #region Public fields
        /// <summary>
        /// Gets the maximum allowed value for <see cref="MaxPreset"/>.
        /// </summary>
        public const int MaxMaxPreset = 10;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the id (primary key)
        /// </summary>
        public long Id => GetInt64(Columns.Id);

        /// <summary>
        /// Gets the creation date/time.
        /// </summary>
        public DateTime Created => GetDateTime(Columns.Created);

        /// <summary>
        /// Gets or sets the camera name.
        /// </summary>
        public string Name
        {
            get => GetString(Columns.Name);
            set => SetValue(Columns.Name, !string.IsNullOrWhiteSpace(value) ? value : "(no name)");
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => GetString(Columns.Description);
            set => SetValue(Columns.Description, !string.IsNullOrWhiteSpace(value) ? value : "(no description)");
        }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        public string IpAddress
        {
            get => GetString(Columns.IpAddress);
            set => SetValue(Columns.IpAddress, !string.IsNullOrWhiteSpace(value) ? value : "127.0.0.1");
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public long Port
        {
            get => GetInt64(Columns.Port);
            set => SetValue(Columns.Port, value);
        }

        /// <summary>
        /// Gets or sets the camera plugin id.
        /// </summary>
        public long PluginId
        {
            get => GetInt64(Columns.PluginId);
            set => SetValue(Columns.PluginId, value);
        }

        /// <summary>
        /// Gets or sets the camera flags.
        /// </summary>
        public CameraFlags Flags
        {
            get => (CameraFlags)GetInt64(Columns.Flags);
            set => SetValue(Columns.Flags, (long)value);
        }

        /// <summary>
        /// Gets or sets the row where the camera is on the wall.
        /// </summary>
        public long WallRow
        {
            get => GetInt64(Columns.WallRow);
            set => SetValue(Columns.WallRow, value);
        }

        /// <summary>
        /// Gets or sets the column where the camera is on the wall.
        /// </summary>
        public long WallColumn
        {
            get => GetInt64(Columns.WallColumn);
            set => SetValue(Columns.WallColumn, value);
        }

        /// <summary>
        /// Gets or sets the established motion speed for the camera.
        /// Clamped between 0-100 inclusive.
        /// </summary>
        public long MotionSpeed
        {
            get => GetInt64(Columns.MotionSpeed);
            set => SetValue(Columns.MotionSpeed, Math.Min(Math.Max(value, 0), 100));
        }

        /// <summary>
        /// Gets or sets the maximum number of presets to use for the camera.
        /// Clamped between 1-<see cref="MaxMaxPreset"/> inclusive.
        /// </summary>
        public long MaxPreset
        {
            get => GetInt64(Columns.MaxPreset);
            set => SetValue(Columns.MaxPreset, Math.Min(Math.Max(value, 1), MaxMaxPreset));
        }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string Note
        {
            get => GetString(Columns.Note);
            set => SetValue(Columns.Note, value);
        }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string UserId
        {
            get => GetString(Columns.UserId);
            set => SetValue(Columns.UserId, value);
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password
        {
            get => GetString(Columns.Password);
            set => SetValue(Columns.Password, value);
        }

        /// <summary>
        /// Gets the associated plugin name.
        /// </summary>
        public string PluginName => GetString(Columns.Calculated.PluginName);

        /// <summary>
        /// Gets the associated plugin guid.
        /// </summary>
        public string PluginGuid => GetString(Columns.Calculated.PluginGuid);
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraRow"/> class
        /// </summary>
        /// <param name="row">The row</param>
        public CameraRow(DataRow row) : base(row)
        {
        }

        /// <summary>
        /// Creates a <see cref="CameraRow"/> if <paramref name="row"/> is not null;
        /// </summary>
        /// <param name="row">The row, or null</param>
        /// <returns>The <see cref="CameraRow"/>, or null if <paramref name="row"/> is null</returns>
        public static CameraRow Create(DataRow row)
        {
            return row != null ? new CameraRow(row) : null;
        }
        #endregion

        /************************************************************************/

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Changes the <see cref="Flags property"/>
        /// </summary>
        /// <param name="add">The flags to add.</param>
        /// <param name="remove">The flags to remove</param>
        public void ChangeFlags(CameraFlags add, CameraFlags remove)
        {
            /* change a local copy to trigger property changed only once */
            CameraFlags current = Flags;
            current |= add;
            current &= ~remove;
            Flags = current;
        }

        /// <summary>
        /// Toggles the specified flag according to the specified boolean.
        /// </summary>
        /// <param name="flag">The flag to set or clear.</param>
        /// <param name="value">true to set the flag; false to clear the flag.</param>
        public void ToggleFlag(CameraFlags flag, bool value)
        {
            CameraFlags add = value ? flag : CameraFlags.None;
            CameraFlags remove = value ? CameraFlags.None : flag;
            ChangeFlags(add, remove);
        }

        /// <summary>
        /// Sets wall properties. Adds <see cref="CameraFlags.IncludeOnWall"/>, sets <see cref="WallRow"/> and <see cref="WallColumn"/>.
        /// </summary>
        /// <param name="row">The row</param>
        /// <param name="column">The column.</param>
        public void SetWallProperties(int row, int column)
        {
            ChangeFlags(CameraFlags.IncludeOnWall, CameraFlags.None);
            WallRow = row;
            WallColumn = column;
        }

        /// <summary>
        /// Removes wall properties. Removes <see cref="CameraFlags.IncludeOnWall"/>
        /// </summary>
        public void RemoveWallProperties()
        {
            ChangeFlags(CameraFlags.None, CameraFlags.IncludeOnWall);
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when a call to SetValue() results in a changed value.
        /// </summary>
        /// <param name="columnName">The name of the column that changed its value.</param>
        /// <param name="value">The new value.</param>
        protected override void OnSetValue(string columnName, object value)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(columnName));
        }
        #endregion

        /************************************************************************/

        #region IComparer class
        /// <summary>
        /// Provides a comparer for <see cref="CameraRow"/>
        /// </summary>
        public class Comparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x is CameraRow s1 && y is CameraRow s2)
                {
                    return s1.Name.CompareTo(s2.Name);
                }
                return 0;
            }
        }
        #endregion
    }
}
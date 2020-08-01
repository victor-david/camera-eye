﻿using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Database.SQLite;
using System;
using System.Data;
using Columns = Restless.App.Database.Tables.CameraTable.Defs.Columns;

namespace Restless.App.Database.Tables
{
    /// <summary>
    /// Represents database data about a single camera
    /// </summary>
    public class CameraRow : RowObjectBase<CameraTable>
    {
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
            set => SetValue(Columns.Name, value);
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => GetString(Columns.Description);
            set => SetValue(Columns.Description, value);
        }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        public string IpAddress
        {
            get => GetString(Columns.IpAddress);
            set => SetValue(Columns.IpAddress, value);
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

        #region Public methods
        /// <summary>
        /// Changes the <see cref="Flags property"/>
        /// </summary>
        /// <param name="add">The flags to add.</param>
        /// <param name="remove">The flags to remove</param>
        public void ChangeFlags(CameraFlags add, CameraFlags remove)
        {
            Flags |= add;
            Flags &= ~remove;
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
        #endregion;
    }
}
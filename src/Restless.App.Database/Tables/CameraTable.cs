using Restless.App.Database.Core;
using Restless.Tools.Database.SQLite;
using System;
using System.Collections.Generic;
using System.Data;

namespace Restless.App.Database.Tables
{
    public class CameraTable : ApplicationTableBase
    {
        #region Public properties
        /// <summary>
        /// Provides static definitions for table properties such as column names and relation names.
        /// </summary>
        public static class Defs
        {
            /// <summary>
            /// Specifies the name of this table.
            /// </summary>
            public const string TableName = "camera";

            /// <summary>
            /// Provides static column names for this table.
            /// </summary>
            public static class Columns
            {
                /// <summary>
                /// The name of the id column. This is the table's primary key.
                /// </summary>
                public const string Id = DefaultPrimaryKeyName;

                /// <summary>
                /// Created column.Date/time the record was created.
                /// </summary>
                public const string Created = "created";

                /// <summary>
                /// Holds the camera name.
                /// </summary>
                public const string Name = "name";

                /// <summary>
                /// Holds the decsription.
                /// </summary>
                public const string Description = "description";

                /// <summary>
                /// Holds the ip address.
                /// </summary>
                public const string IpAddress = "ipaddress";

                /// <summary>
                /// Holds the port.
                /// </summary>
                public const string Port = "port";

                /// <summary>
                /// Holds the camera plugin id id.
                /// </summary>
                public const string PluginId = "pluginid";

                /// <summary>
                /// Holds bitwise flags.
                /// </summary>
                public const string Flags = "flags";

                /// <summary>
                /// The row where the camera is on the wall.
                /// </summary>
                public const string WallRow = "wallrow";

                /// <summary>
                /// The column where the camera is on the wall.
                /// </summary>
                public const string WallColumn = "wallcol";

                /// <summary>
                /// Holds the configured motion speed. 1-100
                /// </summary>
                public const string MotionSpeed = "motionspeed";

                /// <summary>
                /// Hold a user defined note.
                /// </summary>
                public const string Note = "note";

                /// <summary>
                /// Holds the user id.
                /// </summary>
                public const string UserId = "userid";

                /// <summary>
                /// Holds the password.
                /// </summary>
                public const string Password = "password";

                /// <summary>
                /// Provides static column names for columns that are calculated from other values.
                /// </summary>
                public class Calculated
                {
                    /// <summary>
                    /// Associated plugin name.
                    /// </summary>
                    public const string PluginName = "CalcPluginName";
                    /// <summary>
                    /// Associated plugin guid.
                    /// </summary>
                    public const string PluginGuid = "CalcPluginGuid";
                }
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraTable"/> class.
        /// </summary>
        public CameraTable() : base(Defs.TableName)
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Loads the data
        /// </summary>
        public override void Load()
        {
            Load(null, Defs.Columns.Name);
        }

        public CameraRow Add()
        {
            int cameraNum = Rows.Count + 1;
            DataRow newRow = MakeNewRow((row) =>
            {
                row[Defs.Columns.Created] = DateTime.Now;
                row[Defs.Columns.Description] = "Camera description";
                row[Defs.Columns.IpAddress] = "127.0.0.1";
                row[Defs.Columns.Name] = $"Camera {cameraNum}";
                row[Defs.Columns.Port] = 80;
                row[Defs.Columns.PluginId] = 0;
                row[Defs.Columns.Flags] = (long)CameraFlags.StatusTop;
                row[Defs.Columns.WallRow] = 0;
                row[Defs.Columns.WallColumn] = 0;
                row[Defs.Columns.MotionSpeed] = 50;
            });
            return new CameraRow(newRow);
        }

        /// <summary>
        /// Provides an IEnumerable that returns all camera rows.
        /// </summary>
        /// <returns>An IEnumerable that returns all camera rows.</returns>
        public IEnumerable<CameraRow> EnumerateAll()
        {
            foreach (DataRow row in EnumerateRows())
            {
                yield return new CameraRow(row);
            }
            yield break;
        }

        /// <summary>
        /// Provides an IEnumerable that returns camera rows that are flagged to appear on the camera wall.
        /// </summary>
        /// <returns>An IEnumerable that returns camera rows that are flagged to appear on the camera wall.</returns>
        public IEnumerable<CameraRow> EnumerateFlaggedForWall()
        {
            foreach (DataRow row in EnumerateRows())
            {
                CameraRow camera = new CameraRow(row);
                if (camera.Flags.HasFlag(CameraFlags.IncludeOnWall))
                {
                    yield return camera;
                }
            }
            yield break;
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Creates the <see cref="Defs.Columns.Calculated"/> columns for this table.
        /// </summary>
        protected override void UseDataRelations()
        {
            CreateChildToParentColumn(Defs.Columns.Calculated.PluginName, PluginTable.Defs.Relations.ToCamera, PluginTable.Defs.Columns.Name);
            CreateChildToParentColumn(Defs.Columns.Calculated.PluginGuid, PluginTable.Defs.Relations.ToCamera, PluginTable.Defs.Columns.Guid);
        }

        /// <summary>
        /// Gets the column definitions for this table.
        /// </summary>
        /// <returns>A <see cref="ColumnDefinitionCollection"/>.</returns>
        protected override ColumnDefinitionCollection GetColumnDefinitions()
        {
            return new ColumnDefinitionCollection()
            {
                { Defs.Columns.Id, ColumnType.Integer, true },
                { Defs.Columns.Created, ColumnType.Timestamp },
                { Defs.Columns.Name, ColumnType.Text },
                { Defs.Columns.Description, ColumnType.Text },
                { Defs.Columns.IpAddress, ColumnType.Text },
                { Defs.Columns.Port, ColumnType.Integer, false, false, 80 },
                { Defs.Columns.PluginId, ColumnType.Integer, false, false, 0 },
                { Defs.Columns.Flags, ColumnType.Integer, false, false, 0 },
                { Defs.Columns.WallRow, ColumnType.Integer, false, false, 0 },
                { Defs.Columns.WallColumn, ColumnType.Integer, false, false, 0 },
                { Defs.Columns.MotionSpeed, ColumnType.Integer, false, false, 50 },
                { Defs.Columns.Note, ColumnType.Text, false, true },
                { Defs.Columns.UserId, ColumnType.Text, false, true },
                { Defs.Columns.Password, ColumnType.Text, false, true },
            };
        }
        #endregion
    }
}
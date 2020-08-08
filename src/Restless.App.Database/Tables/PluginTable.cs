using Restless.App.Database.Core;
using Restless.Tools.Database.SQLite;
using System;
using System.Collections.Generic;
using System.Data;

namespace Restless.App.Database.Tables
{
    /// <summary>
    /// Represents the table that contains information about discovered plugins.
    /// </summary>
    public class PluginTable : ApplicationTableBase
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
            public const string TableName = "plugin";

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
                /// The name of the plugin.
                /// </summary>
                public const string Name = "name";

                /// <summary>
                /// Plugin description.
                /// </summary>
                public const string Description = "description";

                /// <summary>
                /// Assembly name.
                /// </summary>
                public const string AssemblyName = "assembly";

                /// <summary>
                /// Version
                /// </summary>
                public const string Version = "version";

                /// <summary>
                /// Globally unique id for the plugin.
                /// </summary>
                public const string Guid = "guid";
            }
            /// <summary>
            /// Provides static relation names.
            /// </summary>
            public static class Relations
            {
                /// <summary>
                /// The name of the relation that relates the <see cref="PluginTable"/> to the <see cref="CameraTable"/>.
                /// </summary>
                public const string ToCamera = "PluginToCamera";

            }
            public static class Values
            {
                public const long NullPluginId = 0;
                public const string NullPluginVersion = "0.0.0.0";
                public const string NullPluginGuid = "00000000-0000-0000-0000-000000000000";
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginTable"/> class.
        /// </summary>
        public PluginTable() : base(Defs.TableName)
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Loads the data from the database into the Data collection for this table.
        /// </summary>
        public override void Load()
        {
            Load(null, Defs.Columns.Name);
        }

        /// <summary>
        /// Adds a new service record if one identified by <paramref name="guid"/> doesn't already exist
        /// </summary>
        /// <param name="guid">The unique identifier. Obtained from service contract.</param>
        /// <param name="initializer">Initializer</param>
        public void AddOrUpdatePlugin(string guid, Action<PluginRow> initializer)
        {
            PluginRow plugin = GetPlugin(guid);

            if (plugin == null)
            {
                var obj = new PluginRow(NewRow())
                {
                    Name = "New Service",
                    Guid = guid,
                };
                initializer(obj);
                Rows.Add(obj.Row);
            }
            else
            {
                initializer(plugin);
            }
            Save();
        }

        /// <summary>
        /// Removes the specified service.
        /// </summary>
        /// <param name="guid">The guid of the service to remove.</param>
        public void RemovePlugin(string guid)
        {
            var obj = GetPlugin(guid);
            if (obj != null)
            {
                obj.Row.Delete();
                Save();
            }
        }

        /// <summary>
        /// Gets the service record identified by <paramref name="guid"/>, or null if none.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>A row object, or null if none.</returns>
        public PluginRow GetPlugin(string guid)
        {
            DataRow[] rows = Select($"{Defs.Columns.Guid}='{guid}'");
            if (rows.Length > 1) throw new InvalidOperationException($"More than one service record with {guid}");
            if (rows.Length == 1)
            {
                return new PluginRow(rows[0]);
            }
            return null;
        }
        #endregion

        /************************************************************************/

        #region Public methods (enumeration)
        /// <summary>
        /// Provides an IEnumerable that returns all plugins.
        /// </summary>
        /// <returns>An IEnumerable of <see cref="PluginRow"/>.</returns>
        public IEnumerable<PluginRow> EnumeratePlugins()
        {
            foreach (DataRow row in EnumerateRows(null, Defs.Columns.Name))
            {
                yield return new PluginRow(row);
            }
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Establishes parent / child relationships with other tables.
        /// </summary>
        protected override void SetDataRelations()
        {
            CreateParentChildRelation<CameraTable>(Defs.Relations.ToCamera, Defs.Columns.Id, CameraTable.Defs.Columns.PluginId);
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
                { Defs.Columns.Name, ColumnType.Text },
                { Defs.Columns.Description, ColumnType.Text },
                { Defs.Columns.AssemblyName, ColumnType.Text },
                { Defs.Columns.Version, ColumnType.Text },
                { Defs.Columns.Guid, ColumnType.Text, false, false, null, IndexType.Unique },
            };
        }

        protected override List<string> GetPopulateColumnList()
        {
            return new List<string>()
            {
                Defs.Columns.Id, Defs.Columns.Name, Defs.Columns.Description, Defs.Columns.AssemblyName, Defs.Columns.Version, Defs.Columns.Guid
            };
        }
        protected override IEnumerable<object[]> EnumeratePopulateValues()
        {
            yield return new object[] { Defs.Values.NullPluginId, "(none)", "---", "---", Defs.Values.NullPluginVersion, Defs.Values.NullPluginGuid };
        }
        #endregion
    }
}
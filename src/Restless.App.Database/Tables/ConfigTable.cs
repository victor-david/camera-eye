using Restless.App.Database.Core;
using Restless.Toolkit.Core.Database.SQLite;
using System;
using System.Data;

namespace Restless.App.Database.Tables
{
    /// <summary>
    /// Represents the table that contains the application configuration.
    /// </summary>
    public class ConfigTable : ApplicationTableBase
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
            public const string TableName = "config";

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
                /// The name of the value column.
                /// </summary>
                public const string Value = "value";

            }

            /// <summary>
            /// Provides static names of primary key id values.
            /// </summary>
            public static class FieldIds
            {
                /// <summary>
                /// The id value that identifies the date format.
                /// </summary>
                public const string DateFormat = "DateFormat";
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigTable"/> class.
        /// </summary>
        public ConfigTable() : base(DatabaseController.Instance, DatabaseController.CameraSchemaName, Defs.TableName)
        {
            IsDeleteRestricted = true;
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Loads the data from the database into the Data collection for this table.
        /// </summary>
        public override void Load()
        {
            Load(null, Defs.Columns.Id);
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Sets extended properties on certain columns. See the base implementation <see cref="TableBase.SetColumnProperties"/> for more information.
        /// </summary>
        protected override void SetColumnProperties()
        {
            SetColumnProperty(Columns[Defs.Columns.Id], DataColumnPropertyKey.ExcludeFromUpdate);
        }

        /// <summary>
        /// Gets the column definitions for this table.
        /// </summary>
        /// <returns>A <see cref="ColumnDefinitionCollection"/>.</returns>
        protected override ColumnDefinitionCollection GetColumnDefinitions()
        {
            return new ColumnDefinitionCollection()
            {
                { Defs.Columns.Id, ColumnType.Text, true },
                { Defs.Columns.Value, ColumnType.Text, false, true }
            };
        }
        #endregion

        /************************************************************************/

        #region Internal methods
        /// <summary>
        /// From within this assembly, gets a configuration value specified by id.
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The value</returns>
        internal string GetRowValue(string id)
        {
            DataRow[] rows = Select(string.Format("{0}='{1}'", Defs.Columns.Id, id));
            if (rows.Length == 1)
            {
                return rows[0][Defs.Columns.Value].ToString();
            }

            throw new IndexOutOfRangeException();
        }
        #endregion
    }
}
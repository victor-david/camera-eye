using Restless.Toolkit.Core.Database.SQLite;
using System.Data;
using Columns = Restless.App.Database.Tables.PluginTable.Defs.Columns;

namespace Restless.App.Database.Tables
{
    /// <summary>
    /// Encapsulates a single row from the <see cref="PluginTable"/>.
    /// </summary>
    public class PluginRow : RowObjectBase<PluginTable>
    {
        #region Public properties
        /// <summary>
        /// Gets the id for this row object.
        /// </summary>
        public long Id
        {
            get => GetInt64(Columns.Id);
        }

        /// <summary>
        /// Gets or sets the service name.
        /// </summary>
        public string Name
        {
            get => GetString(Columns.Name);
            set => SetValue(Columns.Name, value);
        }

        /// <summary>
        /// Gets or sets the service description.
        /// </summary>
        public string Description
        {
            get => GetString(Columns.Description);
            set => SetValue(Columns.Description, value);
        }

        /// <summary>
        /// Gets or sets the assembly name of the service.
        /// </summary>
        public string AssemblyName
        {
            get => GetString(Columns.AssemblyName);
            set => SetValue(Columns.AssemblyName, value);
        }

        /// <summary>
        /// Gets or sets the version of the service.
        /// </summary>
        public string Version
        {
            get => GetString(Columns.Version);
            set => SetValue(Columns.Version, value);
        }

        /// <summary>
        /// Gets or sets the guid for the service.
        /// </summary>
        public string Guid
        {
            get => GetString(Columns.Guid);
            internal set => SetValue(Columns.Guid, value);
        }

        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginRow"/> class.
        /// </summary>
        /// <param name="row">The data row</param>
        public PluginRow(DataRow row) : base(row)
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Gets the string representation of this object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"{Name} {Version} {Description}";
        }
        #endregion
    }
}
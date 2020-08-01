using Restless.Tools.Database.SQLite;

namespace Restless.App.Database.Core
{
    /// <summary>
    /// Represents the base class for application tables. This class must be inherited.
    /// </summary>
    public abstract class ApplicationTableBase : ExtendedTableBase
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTableBase"/> class using <see cref="DatabaseController.CameraSchemaName"/>.
        /// </summary>
        /// <param name="tableName">The table name</param>
        protected ApplicationTableBase(string tableName) : base(DatabaseController.Instance, DatabaseController.CameraSchemaName, tableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTableBase"/> class using the specified schema name.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="tableName">The table name</param>
        protected ApplicationTableBase(string schemaName, string tableName) : base(DatabaseController.Instance, schemaName, tableName)
        {
        }
        #endregion

        /************************************************************************/

        #region Properties
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Override to perform an update on the table. This method is called by the controller
        /// when the table version is greater than its stored version. After performing
        /// the update, the controller updates the SchemaVersion table.
        /// The base method does nothing.
        /// </summary>
        protected virtual internal void PerformUpdate()
        {
        }
        #endregion
    }
}
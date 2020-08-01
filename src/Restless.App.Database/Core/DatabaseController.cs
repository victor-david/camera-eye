using Restless.App.Database.Tables;
using Restless.Tools.Database.SQLite;
using System;
using System.IO;

namespace Restless.App.Database.Core
{
    /// <summary>
    /// Represents the database controller
    /// </summary>
    public class DatabaseController : DatabaseControllerBase
    {
        #region Private
        private string rootFolder;
#if DEBUG
        private const string DefaultDbFileName = "CameraData(Debug)." + Extension;
#else
        private const string DefaultDbFileName = "CameraData." + Extension;
#endif
        #endregion

        /************************************************************************/

        #region Public Fields
        /// <summary>
        /// Gets the name of the default database directory.
        /// </summary>
        public const string DefaultDbDirectory = "db";

        /// <summary>
        /// Gets the extension for a database file used by the application.
        /// Does not include the "."
        /// </summary>
        public const string Extension = "db";

        /// <summary>
        /// Gets the name for the attached schema. This schema holds all the main tables.
        /// </summary>
        public const string CameraSchemaName = "cam";
        #endregion

        /************************************************************************/

        #region Singleton access and constructor
        /// <summary>
        /// Gets the singleton instance of this class
        /// </summary>
        public static DatabaseController Instance { get; } = new DatabaseController();

        /// <summary>
        /// Constructor (private)
        /// </summary>
        private DatabaseController() : base()
        {
        }

        /// <summary>
        /// Static constructor. Tells C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static DatabaseController()
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Initializes the database controller by creating and registering 
        /// all of the tables for the application.
        /// </summary>
        /// <param name="rootFolder">The installation folder for the application.</param>
        public void Init(string rootFolder)
        {
            this.rootFolder = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));
            string fileName = Path.Combine(rootFolder, DefaultDbDirectory, DefaultDbFileName);
            CreateAndOpen(MemoryDatabase);
            AttachCameraDatabase(fileName);
        }

        /// <summary>
        /// Attaches the main finance database
        /// </summary>
        /// <param name="databaseName">The database name</param>
        public void AttachCameraDatabase(string databaseName)
        {
            Attach(CameraSchemaName, databaseName, () =>
            {
                CreateAndRegisterTable<CameraTable>();
                CreateAndRegisterTable<PluginTable>();
                TableRegistrationComplete(CameraSchemaName);
            });
        }
        #endregion
    }
}
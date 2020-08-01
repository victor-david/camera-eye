using Restless.App.Database.Core;
using System;
using System.IO;
using System.Reflection;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// A singleton class that provides information about the application.
    /// </summary>
    public sealed class ApplicationInfo
    {

        #region Private
        private const string DefaultRootFileName = "DevelopmentRoot.txt";
        private const string DefaultRootEntry = "DevelopmentRoot=";
        #endregion

        /************************************************************************/

        #region Public properties
        /// <summary>
        /// Gets the root folder for the application.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is used to locate the database file. The sub-directory "db" is always appended. 
        /// </para>
        /// <para>
        /// During normal execution, this property returns the location of the application executable.
        /// During development, you can specify a folder to be used by modifying DevelopmentRoot.txt.
        /// That way, you can use a database located outside of the development environment.
        /// </para>
        /// </remarks>
        public string RootFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the database file name
        /// </summary>
        public string DatabaseFileName
        {
            get => DatabaseController.Instance.GetAttachedFileName(DatabaseController.CameraSchemaName);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the current process is a 64 bit process.
        /// </summary>
        public bool Is64Bit
        {
            get => Environment.Is64BitProcess;
        }
        #endregion

        /************************************************************************/

        #region Singleton access and constructors
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static ApplicationInfo Instance { get; } = new ApplicationInfo();

        private ApplicationInfo()
        {
            RootFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string loc = RootFolder.ToLower();
            if (
                loc.Contains(@"bin\debug") || loc.Contains(@"bin\release") ||
                loc.Contains(@"bin\x86\debug") || loc.Contains(@"bin\x86\release") ||
                loc.Contains(@"bin\x64\debug") || loc.Contains(@"bin\x64\release")
               )
            {
                string devName = Path.Combine(RootFolder, DefaultRootFileName);
                if (File.Exists(devName))
                {
                    string[] lines = File.ReadAllLines(devName);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(DefaultRootEntry))
                        {
                            RootFolder = line.Substring(DefaultRootEntry.Length);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Static constructor. Tells C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static ApplicationInfo()
        {
        }
        #endregion
    }
}
using Restless.Camera.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents a singleton class that manages a composition container.
    /// </summary>
    public class CompositionManager
    {
        #region Public properties
        /// <summary>
        /// Gets the composition container
        /// </summary>
        public CompositionContainer Container
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the camera plugins.
        /// </summary>
        [ImportMany]
        public IEnumerable<Lazy<ICameraPluginCreator>> PluginCreators
        {
            get;
            set;
        }
        #endregion

        /************************************************************************/

        #region Singleton access and contructor
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static CompositionManager Instance { get; } = new CompositionManager();

        private CompositionManager()
        {
        }

        /// <summary>
        /// Static constructor. Tells C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static CompositionManager()
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Gets an instance of the camera plugin with the specified guid
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <returns>The plugin creator, or null if none.</returns>
        public ICameraPluginCreator GetCameraPluginCreator(string guid)
        {
            foreach (var service in PluginCreators)
            {
                if (service.Value.UniqueId == guid)
                {
                    return service.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes the composition manager
        /// </summary>
        /// <param name="app">The application.</param>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is null.</exception>
        public void Initialize(object app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            var catalogs = new AggregateCatalog();
            catalogs.Catalogs.Add(new AssemblyCatalog(app.GetType().Assembly));
            catalogs.Catalogs.Add(new DirectoryCatalog(Path.Combine(Environment.CurrentDirectory, "Plugins")));
            Container = new CompositionContainer(catalogs);
            Container.ComposeParts(this);
        }

        /// <summary>
        /// Shuts down the composition manager.
        /// </summary>
        public void Shutdown()
        {
            try { Container.Dispose(); }
            catch { }
        }
        #endregion
    }
}
using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System;
using System.Windows;

namespace Restless.App.Camera
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                RunApplication(e);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Called when the application is exiting to save any pending database updates.
        /// </summary>
        /// <param name="e">The exit event args</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            CompositionManager.Instance.Shutdown();
            DatabaseController.Instance.Shutdown(saveTables: true);
        }

        /// <summary>
        /// Called from OnStartup(e) separately so we can catch an assembly missing.
        /// OnStartup() runs, then the runtime does a JIT for this method which needs other assemblies.
        /// If something is missing, the try/catch in OnStartup() handles it gracefully.
        /// </summary>
        /// <param name="e">The same parameter passed to OnStartup(e)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void RunApplication(StartupEventArgs e)
        {
            CompositionManager.Instance.Initialize(this);
            DatabaseController.Instance.Init(ApplicationInfo.Instance.RootFolder);
            InitializePlugins();
            WindowFactory.Main.Create().Show();
        }

        private void InitializePlugins()
        {
            foreach (var creator in CompositionManager.Instance.PluginCreators)
            {
                DatabaseController.Instance.GetTable<PluginTable>().AddPlugin(creator.Value.UniqueId, (row) =>
                {
                    row.AssemblyName = creator.Value.AssemblyName;
                    row.Description = creator.Value.Description;
                    row.Name = creator.Value.DisplayName;
                    row.Version = creator.Value.Version;
                });
            }
        }


    }
}
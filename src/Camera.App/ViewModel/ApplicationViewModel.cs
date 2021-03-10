using Restless.Toolkit.Mvvm;

namespace Restless.App.Camera
{
    public abstract class ApplicationViewModel : ViewModelBase
    {

        /// <summary>
        /// Gets the singleton instance of the configuration object. 
        /// Although derived classes can access the singleton instance directly,
        /// this enables easy binding to certain configuration properties
        /// </summary>
        public Core.Config Config
        {
            get => Core.Config.Instance;
        }

        public ApplicationViewModel()
        {
        }
    }
}
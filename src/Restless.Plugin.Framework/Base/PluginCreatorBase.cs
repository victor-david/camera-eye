using System.Reflection;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides a base class for implementations of <see cref="ICameraPluginCreator"/>
    /// which implements portions of the interface. This class must be inherited.
    /// </summary>
    public abstract class PluginCreatorBase
    {
        #region Properties
        /// <summary>
        /// Gets the plugin assembly.
        /// </summary>
        protected Assembly Assembly
        {
            get;
        }

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public virtual string AssemblyName
        {
            get => Assembly.GetName().FullName;
        }

        /// <summary>
        /// Gets the full version of the assembly.
        /// </summary>
        public virtual string Version => Assembly.GetName().Version.ToString();
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCreatorBase"/> class.
        /// </summary>
        protected PluginCreatorBase()
        {
            Assembly = Assembly.GetAssembly(GetType());
        }
        #endregion
    }
}
namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that all camera plugin creators must implement.
    /// </summary>
    public interface ICameraPluginCreator
    {
        /// <summary>
        /// Gets the display name for the plugin.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the description for the plugin.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the assembly of the plugin
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Gets the version for the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets a unique id for the plugin.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// Creates an instance of the plugin.
        /// </summary>
        ICameraPlugin Create(ConnectionParameters parms);

    }
}

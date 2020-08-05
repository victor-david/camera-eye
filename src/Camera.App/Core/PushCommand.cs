namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents a command that is passed via property assignment to the <see cref="CameraWallControl"/>
    /// </summary>
    public class PushCommand
    {
        #region Properties
        /// <summary>
        /// Gets the command type.
        /// </summary>
        public PushCommandType CommandType
        {
            get;
        }

        /// <summary>
        /// Gets the associated id.
        /// </summary>
        public long Id
        {
            get;
        }
        #endregion

        /************************************************************************/

        #region Constructors
        private PushCommand(PushCommandType commandType, long id)
        {
            CommandType = commandType;
            Id = id;
        }

        /// <summary>
        /// Creates a <see cref="PushCommand"/>
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="id">The associated id</param>
        /// <returns>An instance of <see cref="PushCommand"/>.</returns>
        public static PushCommand Create(PushCommandType commandType, long id)
        {
            return new PushCommand(commandType, id);
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Gets a string representation of this onject.
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return $"PushCommand: {CommandType} {Id}";
        }
        #endregion
    }
}
namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// A generic Mettler Toledo exception
    /// </summary>
    public abstract class MettlerToledoException : System.Exception
    {
        /// <summary>
        /// The command that triggered this exception
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public MettlerToledoException(string command) => Command = command;
    }
}

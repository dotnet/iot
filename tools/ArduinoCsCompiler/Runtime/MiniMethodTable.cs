using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This is the method type struct. We just replace it's methods, so that <see cref="MiniRuntimeHelpers.GetMethodTable"/> works.
    /// This is tricky, because the original that we replace here is a struct
    /// </summary>
    [ArduinoReplacement("System.Runtime.CompilerServices.MethodTable", IncludingPrivates = true)]
    internal class MiniMethodTable
    {
        public bool IsMultiDimensionalArray
        {
            [ArduinoImplementation]
            get
            {
                return false; // TODO
            }
        }
    }
}

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Diagnostics.Debugger), true)]
    internal class MiniDebugger
    {
        public static bool IsAttached
        {
            get
            {
                return false;
            }
        }

        public static void NotifyOfCrossThreadDependency()
        {
        }
    }
}

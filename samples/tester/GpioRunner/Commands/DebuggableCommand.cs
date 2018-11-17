using CommandLine;

namespace GpioRunner
{
    public class DebuggableCommand
    {
        [Option("wait-for-debugger", HelpText = "When true, the program will pause during startup until a debugger is attached.", Required = false, Default = false)]
        public bool WaitForDebugger { get; set; }
    }
}

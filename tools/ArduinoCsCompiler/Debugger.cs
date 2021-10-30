using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public class Debugger
    {
        private readonly MicroCompiler _compiler;
        private readonly ExecutionSet _set;

        internal Debugger(MicroCompiler compiler, ExecutionSet set)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _set = set ?? throw new ArgumentNullException(nameof(set));
        }

        public void ProcessCommandLine(string currentInput)
        {
            throw new NotImplementedException();
        }

        public void WriteCurrentState()
        {
            Console.Write("Debugger > ");
        }

        public void ProcessExecutionState(byte[] data)
        {
           // This gets the whole data block from the execution engine
           // Lets start decoding where we are.
        }
    }
}

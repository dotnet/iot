using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(StackTrace), true, false, IncludingPrivates = true)]
    internal class MiniStackTrace
    {
        private Exception _exception;
        public MiniStackTrace(Exception e, bool fNeedsFileInfo)
        {
            _exception = e;
        }

        public int FrameCount
        {
            get
            {
                return 0;
            }
        }

        public StackFrame? GetFrame(int idx)
        {
            return null;
        }

        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        public string ToString(int traceFormat)
        {
            return "Stack Trace";
        }
    }
}

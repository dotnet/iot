using System;
using System.Diagnostics;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(StackTrace), true, IncludingPrivates = true)]
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

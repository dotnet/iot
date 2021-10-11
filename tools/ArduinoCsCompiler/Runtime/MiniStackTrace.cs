using System;
using System.Diagnostics;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(StackTrace), true, IncludingPrivates = true)]
    internal class MiniStackTrace
    {
        private Exception _exception;
        public MiniStackTrace(Exception e, bool fNeedsFileInfo)
        {
            _exception = e;
        }

        public MiniStackTrace(bool fNeedsFileInfo)
        {
            _exception = new Exception("Unknown exception in current thread");
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
            return "Stack Trace with exception " + _exception.Message;
        }

        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        internal void ToString(int traceFormat, System.Text.StringBuilder sb)
        {
            sb.AppendLine(ToString());
        }
    }
}

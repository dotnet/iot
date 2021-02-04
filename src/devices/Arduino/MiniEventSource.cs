using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Diagnostics.Tracing.EventSource), true, IncludingPrivates = true)]
    internal class MiniEventSource : IDisposable
    {
        public MiniEventSource(Guid g, string s)
        {
        }

        public bool IsEnabled()
        {
            return false;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        protected unsafe void WriteEventCore(int eventId, int eventDataCount, void* data)
        {
        }

        protected void WriteEvent(int eventId, int arg1, int arg2)
        {
        }

        protected void WriteEvent(int eventId, int arg1, int arg2, int arg3)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
        }
    }
}

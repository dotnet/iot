using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Diagnostics.Tracing.EventSource), true, IncludingPrivates = true)]
    internal class MiniEventSource : IDisposable
    {
        public MiniEventSource()
        {
        }

        public MiniEventSource(Guid g, string s)
        {
        }

        public static bool IsSupported
        {
            [ArduinoImplementation]
            get { return false; }
        }

        public virtual ReadOnlySpan<byte> ProviderMetadata
        {
            get
            {
                return ReadOnlySpan<byte>.Empty;
            }
        }

        public bool IsEnabled()
        {
            return false;
        }

        public Boolean IsEnabled(System.Diagnostics.Tracing.EventLevel level, System.Diagnostics.Tracing.EventKeywords keywords)
        {
            return false;
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        protected unsafe void WriteEventWithRelatedActivityIdCore(System.Int32 eventId, System.Guid* relatedActivityId, System.Int32 eventDataCount, void* data)
        {
        }

        protected virtual void OnEventCommand(System.Diagnostics.Tracing.EventCommandEventArgs command)
        {
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

        protected void WriteEvent(Int32 eventId, Int32 arg1)
        {
        }

        protected void WriteEvent(System.Int32 eventId, System.Int64 arg1)
        {
        }

        protected void WriteEvent(System.Int32 eventId, System.Int64 arg1, System.Int64 arg2, System.Int64 arg3)
        {
        }

        public static void SetCurrentThreadActivityId(Guid activityId)
        {
        }

        public static void SetCurrentThreadActivityId(Guid activityId, out Guid oldActivityThatWillContinue)
        {
            oldActivityThatWillContinue = default;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return "UnsupportedEventSource";
        }
    }
}

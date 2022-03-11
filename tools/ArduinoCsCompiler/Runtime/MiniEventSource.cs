// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        [ArduinoImplementation("NoOp", CompareByParameterNames = true)]
        protected unsafe void WriteEventWithRelatedActivityIdCore(System.Int32 eventId, System.Guid* relatedActivityId, System.Int32 eventDataCount, void* data)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected virtual void OnEventCommand(System.Diagnostics.Tracing.EventCommandEventArgs command)
        {
        }

        [ArduinoImplementation("NoOp", CompareByParameterNames = true)]
        protected unsafe void WriteEventCore(int eventId, int eventDataCount, void* data)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(int eventId, int arg1, int arg2)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(int eventId, int arg1, int arg2, int arg3)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(Int32 eventId, Int32 arg1)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 eventId, System.Int64 arg1)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 eventId, System.Int64 arg1, System.Int64 arg2, System.Int64 arg3)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 EventId, System.String arg1)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 EventId, System.String arg1, System.String arg2)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 EventId, System.Int32 arg1, System.String arg2)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 eventId, System.String arg1, System.String arg2, System.String arg3)
        {
        }

        [ArduinoImplementation("NoOp")]
        protected void WriteEvent(System.Int32 EventId)
        {
        }

        [ArduinoImplementation("NoOp")]
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

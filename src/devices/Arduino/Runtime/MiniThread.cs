using System;
using System.Threading;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Threading.Thread), false, IncludingPrivates = true)]
    internal class MiniThread
    {
#pragma warning disable 414
        private ExecutionContext? _executionContext;
#pragma warning restore 414

        public MiniThread()
        {
            _executionContext = null;
        }

        /// <summary>
        /// This method performs busy waiting for a specified number of milliseconds.
        /// It is not implemented as low-level function because this allows other code to continue.
        /// That means this does not block other tasks (and particularly the communication) from working.
        /// </summary>
        /// <param name="delayMs">Number of milliseconds to sleep</param>
        [ArduinoImplementation]
        public static void Sleep(int delayMs)
        {
            if (delayMs <= 0)
            {
                return;
            }

            int ticks = Environment.TickCount;
            int endTicks = ticks + delayMs;
            if (ticks > endTicks)
            {
                // There will be a wraparound
                int previous = ticks;
                // wait until the tick count wraps around
                while (previous < ticks)
                {
                    previous = ticks;
                    ticks = Environment.TickCount;
                }
            }

            while (endTicks > ticks)
            {
                // Busy waiting is ok here - the microcontroller has no sleep state
                ticks = Environment.TickCount;
            }
        }

        [ArduinoImplementation]
        public static void Sleep(TimeSpan delay)
        {
            Sleep((int)delay.TotalMilliseconds);
        }

        [ArduinoImplementation]
        public static bool Yield()
        {
            // We are running in a single-thread environment, so this is effectively a no-op
            return false;
        }

        [ArduinoImplementation(NativeMethod.ArduinoNativeHelpersGetMicroseconds)]
        public static void SpinWait(int micros)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.None)]
        public static int GetCurrentProcessorId()
        {
            return 0;
        }

        [ArduinoImplementation]
        public static int GetCurrentProcessorNumber()
        {
            return 0;
        }

        [ArduinoImplementation(NativeMethod.None)]
        public static Thread GetCurrentThreadNative()
        {
            return MiniUnsafe.As<Thread>(new MiniThread());
        }

        [ArduinoImplementation(NativeMethod.None, CompareByParameterNames = true)]
        public static DeserializationTracker GetThreadDeserializationTracker(ref int stackMark)
        {
            stackMark = 0;
            return new DeserializationTracker();
        }

        [ArduinoReplacement("System.Runtime.Serialization.DeserializationTracker", null, true)]
        internal sealed class DeserializationTracker
        {
            public bool DeserializationInProgress
            {
                get;
                set;
            }
        }
    }
}

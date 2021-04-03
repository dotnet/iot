using System;
using System.Threading;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Threading.Thread), true)]
    internal class MiniThread
    {
        /// <summary>
        /// Current thread value. This field must be made Thread-Local if multiple threads are supported
        /// </summary>
        private static MiniThread? s_currentThread;
        public static MiniThread CurrentThread => s_currentThread ?? InitializeCurrentThread();

        private string? _name;
        // private Delegate? _delegate; // Delegate
        // private object? _threadStartArg;
        internal MiniThread()
        {
            _name = "Main Thread";
            // _delegate = null;
            // _threadStartArg = null;
        }

        public string? Name
        {
            get
            {
                return _name;
            }
        }

        public int ManagedThreadId => 1;

        /// <summary>
        /// This method performs busy waiting for a specified number of milliseconds.
        /// It is not implemented as low-level function because this allows other code to continue.
        /// That means this does not block other tasks (and particularly the communication) from working.
        /// </summary>
        /// <param name="delayMs">Number of milliseconds to sleep</param>
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

        public static MiniThread InitializeCurrentThread()
        {
            s_currentThread = new MiniThread();
            return s_currentThread;
        }

        public static void Sleep(TimeSpan delay)
        {
            Sleep((int)delay.TotalMilliseconds);
        }

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

        public static int OptimalMaxSpinWaitsPerSpinIteration
        {
            get
            {
                return 1;
            }
        }

        [ArduinoImplementation(NativeMethod.None)]
        public static int GetCurrentProcessorId()
        {
            return 0;
        }

        public void Join()
        {
            // Threads are not yet supported
        }
    }
}

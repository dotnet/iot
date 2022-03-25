// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Threading.Thread), false, IncludingPrivates = true)]
    internal class MiniThread
    {
#pragma warning disable 414, SA1306
        private IntPtr _DONT_USE_InternalThread;
        private int _managedThreadId;
        private ExecutionContext? _executionContext;
#pragma warning restore 414, SA1306

        public MiniThread()
        {
            _executionContext = null;
            _DONT_USE_InternalThread = IntPtr.Zero;
            _managedThreadId = 1;
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

        public static int OptimalMaxSpinWaitsPerSpinIteration
        {
            [ArduinoImplementation]
            get
            {
                return 1;
            }
        }

        public int ManagedThreadId
        {
            [ArduinoImplementation]
            get
            {
                return _managedThreadId;
            }
        }

        [ArduinoImplementation]
        public static void Sleep(TimeSpan delay)
        {
            Sleep((int)delay.TotalMilliseconds);
        }

        [ArduinoImplementation("ThreadYield")]
        public static bool Yield()
        {
            return false;
        }

        [ArduinoImplementation]
        public static void SpinWait(int micros)
        {
            // No op, we're not fast enough
        }

        [ArduinoImplementation]
        public static int GetCurrentProcessorId()
        {
            return 0;
        }

        [ArduinoImplementation]
        public static int GetCurrentProcessorNumber()
        {
            return 0;
        }

        [ArduinoImplementation]
        public static Thread GetCurrentThreadNative()
        {
            return MiniUnsafe.As<Thread>(new MiniThread());
        }

        /// <summary>
        /// First arg is of type ThreadHandle, but this is a value type over an IntPtr, so their layout is identical
        /// </summary>
        [ArduinoImplementation("ThreadStartInternal", CompareByParameterNames = true)]
        public static unsafe void StartInternal(IntPtr t, int stackSize, int priority, char* pThreadName)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ThreadInitialize")]
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ThreadJoin")]
        public bool Join(int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }
    }
}

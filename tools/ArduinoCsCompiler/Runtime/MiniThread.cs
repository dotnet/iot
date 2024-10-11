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
        private SynchronizationContext? _synchronizationContext;
        private string _name;
#pragma warning restore 414, SA1306

        public MiniThread()
        {
            _executionContext = null;
            _DONT_USE_InternalThread = IntPtr.Zero;
            _managedThreadId = 0;
            _synchronizationContext = null;
            _name = string.Empty;
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
                    Yield();
                    ticks = Environment.TickCount;
                }
            }

            while (endTicks > ticks)
            {
                // Busy waiting is ok here - the microcontroller has no sleep state
                Yield();
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

        public bool IsThreadPoolThread
        {
            // The backend doesn't do much with this field, but if we implement it here,
            // we need to add it's backing field to the class, which would require some
            // special handling
            [ArduinoImplementation("Thread_get_IsThreadPoolThread")]
            get;

            [ArduinoImplementation("Thread_set_IsThreadPoolThread")]
            set;
        }

        public bool IsBackground
        {
            // The backend doesn't do much with this field, but if we implement it here,
            // we need to add it's backing field to the class, which would require some
            // special handling
            [ArduinoImplementation("Thread_get_IsBackground")]
            get;

            [ArduinoImplementation("Thread_set_IsBackground")]
            set;
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
        public static void UninterruptibleSleep0()
        {
            Yield();
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

        [ArduinoImplementation("ThreadGetCurrentThreadNative")]
        public static Thread GetCurrentThreadNative()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// First arg is of type ThreadHandle, but this is a value type over an IntPtr, so their layout is identical
        /// </summary>
        [ArduinoImplementation("ThreadStartInternal", CompareByParameterNames = true)]
        public static unsafe void StartInternal(IntPtr t, int stackSize, int priority, char* pThreadName)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static void MemoryBarrier()
        {
            // Nothing to do here
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

        [ArduinoImplementation]
        public void SetThreadPoolWorkerThreadName()
        {
            // Nothing to do, really (we don't keep thread names)
        }
    }
}

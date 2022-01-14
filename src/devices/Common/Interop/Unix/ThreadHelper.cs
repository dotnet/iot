// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Field should begin with upper-case letter
#pragma warning disable CS1591

using System.Runtime.InteropServices;

partial class Interop
{
    public class ThreadHelper
    {
        public static bool SetCurrentThreadHighPriority()
        {
            IntPtr thread = pthread_self();
            sched_param sched = new sched_param();

            sched.sched_priority = sched_get_priority_min(SCHED_FIFO);
            return pthread_setschedparam(thread, SCHED_FIFO, ref sched) == 0;
        }

        public static void SetCurrentThreadNormalHighPriority()
        {
            nice(19);
        }

        // Native Interop
        private const int SYS_gettid = 224;
        private const int SCHED_NORMAL = 0;
        private const int SCHED_FIFO = 1;
        private const int SCHED_RR = 2;
        private const int SCHED_BATCH = 3;
        private const int SCHED_OTHER = 4;
        private const int SCHED_IDLE = 5;
        private const int SCHED_RESET_ON_FORK = 0x40000000;

        [DllImport("libc")]
        private static extern int nice(int inc);

        [DllImport("libc")]
        private static extern IntPtr pthread_self();

        [DllImport("libc")]
        private static extern int sched_get_priority_min(int policy);

        [DllImport("libc")]
        private static extern int sched_get_priority_max(int policy);

        [DllImport("libc")]
        private static extern int pthread_setschedparam(IntPtr thread, int policy, ref sched_param param);

        [DllImport("libc")]
        internal static extern int sched_yield();

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct sched_param
        {
            public int sched_priority;
            public int sched_curpriority;
            public fixed int reserved[8];
        }
    }
}

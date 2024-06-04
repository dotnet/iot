// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable SX1309 // Inconsistent naming (names must match the CLR)
namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Threading.TimerQueue", "System.Private.Corelib.dll", false, typeof(string), IncludingPrivates = true)]
    internal class MiniTimerQueue
    {
        // Implementation partially copied from the CLR, minus debugging stuff and minus native timers
        private int _id;
        private FireAfterTimeout? m_appDomainTimer;
        // The current threshold, an absolute time where any timers scheduled to go off at or
        // before this time must be queued to the short list.
        private long _currentAbsoluteThreshold = Environment.TickCount64 + 333;

        private object? _shortTimers; // Actual type: TimerQueueTimer
        private object? _longTimers;

        private bool _isTimerScheduled;
        private long _currentTimerStartTicks;
        private uint _currentTimerDuration;

        private MiniTimerQueue(int id)
        {
            _id = id;
            _shortTimers = null;
            _longTimers = null;
            _isTimerScheduled = false;
            _currentTimerStartTicks = 0;
            _currentTimerDuration = 0;
            _shortTimers = null;
            _longTimers = null;
        }

        public static MiniTimerQueue[] Instances { get; } = CreateTimerQueues();

        private static MiniTimerQueue[] CreateTimerQueues()
        {
            var queues = new MiniTimerQueue[Environment.ProcessorCount];
            for (int i = 0; i < queues.Length; i++)
            {
                queues[i] = new MiniTimerQueue(i);
            }

            return queues;
        }

        public long ActiveCount { get; private set; }

        /// <summary>
        /// This method internally calls back on AppDomainTimerCallback(int) from the C# implementation
        /// </summary>
        [ArduinoImplementation("MiniTimerQueueFireCallback", InternalCall = true)]
        public static void FireTimerInternal(int id)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public bool SetTimer(uint actualDuration)
        {
            lock (this)
            {
                if (m_appDomainTimer == null || m_appDomainTimer.Done)
                {
                    m_appDomainTimer = new FireAfterTimeout(actualDuration, _id);
                    return true;
                }
                else
                {
                    m_appDomainTimer.Duration = actualDuration;
                    return true;
                }
            }
        }

        /// <summary>
        /// Dummy method to suppress warnings (not referenced!)
        /// </summary>
        /// <returns></returns>
        public bool DummyUseFields()
        {
            if (_isTimerScheduled)
            {
                var end = _currentTimerStartTicks + _currentTimerDuration;
                return end > 0;
            }

            return _shortTimers != null && _longTimers != null;
        }

        private class FireAfterTimeout
        {
            private int _id;
            private Thread _thread;
            private bool _aborted;
            public FireAfterTimeout(uint duration, int id)
            {
                Duration = duration;
                _id = id;
                _aborted = false;
                Done = false;
                StartTime = Environment.TickCount64;
                _thread = new Thread(WaitUntilFire);
                _thread.Start();
            }

            public uint Duration
            {
                get;
                set;
            }

            private long StartTime
            {
                get;
            }

            public bool Done
            {
                get;
                private set;
            }

            public void Abort()
            {
                _aborted = true;
                _thread.Join();
            }

            private void WaitUntilFire()
            {
                // This way, any change of duration will immediately have an impact
                while (StartTime + Duration > Environment.TickCount64)
                {
                    if (_aborted)
                    {
                        return;
                    }

                    Thread.Yield();
                }

                Done = true;
                MiniTimerQueue.FireTimerInternal(_id);
            }
        }
    }
}

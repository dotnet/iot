// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;
using Xunit;

namespace ArduinoCsCompiler.Tests
{
    public class ThreadingTests
    {
        private static int s_data = 0;

        [ThreadStatic]
        private static int s_threadStatic = 0;

        public static int StartAndStopThread(int a, int b)
        {
            s_data = 0;
            Thread t = new Thread(MyStaticThreadStart1);
            t.Start();
            t.Join();
            return s_data;
        }

        public static int UseThreadStatic(int a, int b)
        {
            s_threadStatic = 1;
            Thread t = new Thread(UseThreadStaticVariable);
            t.Start();
            t.Join();

            MiniAssert.That(s_threadStatic == 1);
            return 1;
        }

        public static int DiningPhilosophers(int a, int b)
        {
            DiningPhilosopher.StartDinner();
            return 1;
        }

        private static void MyStaticThreadStart1()
        {
            s_data = 1;
        }

        private static void UseThreadStaticVariable()
        {
            MiniAssert.That(s_threadStatic == 0);
            s_threadStatic = 2;
            MiniAssert.That(s_threadStatic == 2);
        }
    }
}

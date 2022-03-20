// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Tests
{
    public class ThreadingTests
    {
        public static int _data = 0;
        public static int StartAndStopThread(int a, int b)
        {
            _data = 0;
            Thread t = new Thread(MyStaticThreadStart1);
            t.Start();
            t.Join();
            return _data;
        }

        public static int DiningPhilosophers(int a, int b)
        {
            DiningPhilosopher.StartDinner();
            return 1;
        }

        private static void MyStaticThreadStart1()
        {
            _data = 1;
        }
    }
}

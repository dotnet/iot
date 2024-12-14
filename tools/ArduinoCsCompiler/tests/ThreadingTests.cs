// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;
using Xunit;

namespace Iot.Device.Arduino.Tests
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
            Assert.False(t.IsThreadPoolThread);
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

        public static int UseThreadStaticInSystem(int a, int b)
        {
            Console.WriteLine($"Outside, we print {a}");
            Thread t = new Thread(PrintNumber);
            t.Start(a);
            t.Join();
            Console.WriteLine($"And then, we print {a}+{b}={a + b}");
            MiniAssert.That(s_data == a);
            return 1;
        }

        public static int DiningPhilosophers(int a, int b)
        {
            DiningPhilosopher.StartDinner();
            return 1;
        }

        public static int UseArrayPool(int a, int b)
        {
            var firstArray = ArrayPool<char>.Shared.Rent(0x100);
            firstArray[0] = 'x';
            firstArray[1] = 'y';

            Thread t = new Thread(ArrayPoolThread);
            t.Start('a');
            t.Join();
            t = new Thread(ArrayPoolThread);
            t.Start('b');
            t.Join();
            MiniAssert.That(firstArray[0] == 'x');
            MiniAssert.That(firstArray[1] == 'y');
            ArrayPool<char>.Shared.Return(firstArray);
            var yetTheSameArray = ArrayPool<char>.Shared.Rent(0x100);
            MiniAssert.That(ReferenceEquals(firstArray, yetTheSameArray));
            yetTheSameArray[2] = 'z';
            ArrayPool<char>.Shared.Return(yetTheSameArray);
            return 1;
        }

        public static int TestTask(int a, int b)
        {
            var t = Task.Factory.StartNew(() => 1);

            MiniAssert.That(t.Result == 1);
            return t.Result;
        }

        public static int AsyncAwait(int a, int b)
        {
            var t = DoSomeExpensiveCalculation();
            return t.Result;
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

        private static void PrintNumber(object? o)
        {
            if (o == null)
            {
                Console.WriteLine("Our parameter is null");
                return;
            }

            int i = (int)o;
            s_data = i;
            Console.WriteLine($"The number is {i}.");
            Console.WriteLine($"And later, the number will be {i + 1}");
        }

        private static void ArrayPoolThread(object? o)
        {
            char c = (char)o!;
            var firstArray = ArrayPool<char>.Shared.Rent(0x100);
            MiniAssert.That(firstArray[0] == 0);
            firstArray[0] = c;
            firstArray[1] = c;
            ArrayPool<char>.Shared.Return(firstArray);
        }

        private static async Task<int> DoSomeExpensiveCalculation()
        {
            int b = 2 - 1;
            await Task.Delay(1000);
            return b;
        }
    }
}

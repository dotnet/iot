// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// A simple implementation of the Dining Philosophers problem, to test locking and threading
    /// </summary>
    internal class DiningPhilosopher
    {
        private const int NumPhilosophers = 5;
        private const int AmountOfSphagetti = 10;
        private static Random _random = new Random(4711);
        private readonly int _number;
        private readonly Fork _left;
        private readonly Fork _right;
        private int _amountOfSpaghetti;

        public DiningPhilosopher(int number, Fork left, Fork right, int amountOfSpaghetti)
        {
            _number = number;
            _left = left;
            _right = right;
            _amountOfSpaghetti = amountOfSpaghetti;
        }

        public static void StartDinner()
        {
            List<Thread> threads = new List<Thread>();
            List<Fork> forks = new List<Fork>();
            for (int i = 0; i < NumPhilosophers + 1; i++)
            {
                forks.Add(new Fork(i));
            }

            for (int i = 0; i < NumPhilosophers; i++)
            {
                DiningPhilosopher p;
                if (i != NumPhilosophers - 1)
                {
                    p = new DiningPhilosopher(i, forks[i], forks[i + 1], AmountOfSphagetti);
                }
                else
                {
                    p = new DiningPhilosopher(i, forks[i], forks[0], AmountOfSphagetti);
                }

                threads.Add(new Thread(p.ThinkAndEatThread));
            }

            Console.WriteLine("Dinner starts");
            foreach (var t in threads)
            {
                t.Start();
            }

            foreach (var t in threads)
            {
                t.Join();
            }

            Console.WriteLine("Everything has been eaten up");
        }

        public void ThinkAndEatThread()
        {
            while (_amountOfSpaghetti > 0)
            {
                int decision = _random.Next(10);
                if (decision < 5)
                {
                    TryEat();
                }
                else
                {
                    Console.WriteLine($"Philosopher {_number} is thinking");
                    Thread.Sleep(decision * 100);
                }
            }

            Console.WriteLine($"Philosopher {_number} has eaten up");
        }

        private void TryEat()
        {
            if (!_left.Take())
            {
                Console.WriteLine($"Philosopher {_number} cannot get left fork");
                return;
            }

            if (!_right.Take())
            {
                Console.WriteLine($"Philosopher {_number} cannot get right fork");
                _left.Return();
                return;
            }

            Console.WriteLine($"Philosopher {_number} is eating. He has {_amountOfSpaghetti} portions left");
            _amountOfSpaghetti--;

            _right.Return();
            _left.Return();
        }

        internal sealed class Fork
        {
            public int Number { get; }
            private object _lock;

            public Fork(int number)
            {
                Number = number;
                _lock = new object();
            }

            public bool Take()
            {
                return Monitor.TryEnter(_lock, 500);
            }

            public void Return()
            {
                Monitor.Exit(_lock);
            }
        }
    }
}

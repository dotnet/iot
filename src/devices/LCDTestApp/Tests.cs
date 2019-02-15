using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LCDTestApp
{
    public static class Tests
    {
        public static void MeasureSpin()
        {
            Console.WriteLine("Measuring spin duration...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                System.Threading.Thread.SpinWait(1);
            }
            long elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks per Thread.SpinWait(1): {elapsed / 10000}");
            stopwatch.Restart();
            for (int i = 0; i < 10000; i++)
            {
                System.Threading.Thread.Yield();
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks per Thread.Yield(): {elapsed / 10000}");
            var sw = new System.Threading.SpinWait();
            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                sw.SpinOnce();
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks per SpinWait.SpinOnce(): {elapsed / 1000}");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                sw.SpinOnce();
                sw.SpinOnce();
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks per 2x SpinWait.SpinOnce(): {elapsed / 2000}");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                while (!sw.NextSpinWillYield)
                {
                    sw.SpinOnce();
                }
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks until yield: {elapsed / 1000}");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                while (!sw.NextSpinWillYield)
                {
                    sw.SpinOnce();
                }
                sw.SpinOnce();
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks with one yield: {elapsed / 1000}");

            int spins = 0;
            sw.Reset();
            while (!sw.NextSpinWillYield)
            {
                sw.SpinOnce();
                spins++;
            }

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                for (int j = 0; j < spins; j++)
                {
                    sw.SpinOnce();
                }
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks until yield (no check): {elapsed / 1000}");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                sw.Reset();
                for (int j = 0; j <= spins; j++)
                {
                    sw.SpinOnce();
                }
            }
            elapsed = stopwatch.ElapsedTicks;
            Console.WriteLine($"Ticks until yield (no check): {elapsed / 1000}");
        }
    }
}

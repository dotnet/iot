// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace ArduinoCsCompiler.Runtime
{
    internal static class ThreadIdAllocator
    {
        private static int s_lastId;

        public static int Allocate()
        {
            int previous;
            do
            {
                previous = s_lastId;
                if (previous == int.MaxValue)
                {
                    throw new InvalidOperationException("Managed thread IDs exhausted.");
                }
            }
            while (Interlocked.CompareExchange(ref s_lastId, previous + 1, previous) != previous);

            return previous + 1;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Threading.Tasks;

namespace System.Device.Gpio.Tests
{
    public static class TimeoutHelper
    {
        public static void CompletesInTime(Action test, TimeSpan timeout)
        {
            Task task = Task.Run(test);
            bool completedInTime = Task.WaitAll(new[] { task }, timeout);

            if (task.Exception != null)
            {
                if (task.Exception.InnerExceptions.Count == 1)
                {
                    throw task.Exception.InnerExceptions[0];
                }

                throw task.Exception;
            }

            if (!completedInTime)
            {
                throw new TimeoutException($"Test did not complete in the specified timeout: {timeout.TotalSeconds} seconds.");
            }
        }
    }
}

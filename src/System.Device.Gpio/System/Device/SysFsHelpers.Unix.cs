// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;
using System.Threading;

namespace System.Device
{
    internal static class SysFsHelpers
    {
        // Max acceptable wait time to make progress before throwing an exception
        private static readonly long MaxWaitTime = 1 * Stopwatch.Frequency; // 1s
        private static readonly TimeSpan MinRetryInterval = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// Ensures path can be read-write accessed or throws UnauthorizedAccessException.
        /// This method is meant to be used with sysfs drivers and access is checked for maximum period of 1 second.
        /// Waiting period is used to give time for udev to update permissions.
        /// </summary>
        /// <param name="path">Path to be checked for access</param>
        public static void EnsureReadWriteAccessToPath(string path)
        {
            if (!WaitWithRetriesUntil(() => PathHasReadWriteAccess(path)))
            {
                throw new UnauthorizedAccessException($"Timeout exceeded while waiting for access to `{path}`.");
            }
        }

        /// <summary>
        /// Ensures directory exists and can be read-write accessed or throws UnauthorizedAccessException.
        /// This method is meant to be used with sysfs drivers and exception is thrown if progress is not made for one second.
        /// Waiting period is used to give time for udev to create directory or update permissions.
        /// </summary>
        /// <param name="directory">Directory to be checked for access</param>
        public static void EnsureDirectoryExistsAndHasReadWriteAccess(string directory)
        {
            if (!WaitWithRetriesUntil(() => Directory.Exists(directory)))
            {
                throw new UnauthorizedAccessException($"Timeout exceeded while waiting for `{directory}` to appear.");
            }

            EnsureReadWriteAccessToPath(directory);
        }

        private static bool WaitWithRetriesUntil(Func<bool> isDone)
        {
            // quick return for happy path
            if (isDone())
            {
                return true;
            }

            long expiration = Stopwatch.GetTimestamp() + MaxWaitTime;

            do
            {
                Thread.Sleep(MinRetryInterval);

                if (isDone())
                {
                    return true;
                }
            }
            while (Stopwatch.GetTimestamp() < expiration);

            return false;
        }

        private static bool PathHasReadWriteAccess(string path)
        {
            return Interop.access(path, AccessModeFlags.R_OK | AccessModeFlags.W_OK) == 0;
        }
    }
}

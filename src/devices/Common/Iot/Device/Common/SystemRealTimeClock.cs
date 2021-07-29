// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Common
{
    /// <summary>
    /// Contains methods to access and update the system real time clock ("Bios clock")
    /// </summary>
    public static class SystemRealTimeClock
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1);

        /// <summary>
        /// Set the system time to the given date/time.
        /// The time must be given in utc.
        /// The method requires elevated permissions. On Windows, the calling user must either be administrator or the right
        /// "Change the system clock" must have been granted to the "Users" group (in Security policy management).
        /// On Unix, the current user must be root or it must be able to sudo without password.
        /// </summary>
        /// <param name="dt">Date/time to set the system clock to. This must be in UTC</param>
        /// <exception cref="PlatformNotSupportedException">This method is not supported on this platform</exception>
        /// <exception cref="IOException">There was an error executing a system command</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have permissions to set the system clock</exception>
        public static void SetSystemTimeUtc(DateTime dt)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetSystemTimeUtcWindows(dt);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                SetDateTimeUtcUnix(dt);
            }
            else
            {
                throw new PlatformNotSupportedException($"No implementation available for {Environment.OSVersion}");
            }
        }

        /// <summary>
        /// Gets the current system time directly using OS calls.
        /// Normally, this should return the same as <see cref="DateTime.UtcNow"/>
        /// </summary>
        /// <returns>The current system time</returns>
        public static DateTime GetSystemTimeUtc()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return GetSystemTimeUtcWindows();
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return GetDateTimeUtcUnix();
            }
            else
            {
                throw new PlatformNotSupportedException($"No implementation available for {Environment.OSVersion}");
            }
        }

        private static void SetSystemTimeUtcWindows(DateTime dt)
        {
            Interop.SystemTime st = DateTimeToSystemTime(dt);
            try
            {
                if (!Interop.SetSystemTime(ref st))
                {
                    throw new IOException("SetSystemTime returned an unspecified error");
                }
            }
            catch (System.Security.SecurityException x)
            {
                throw new UnauthorizedAccessException("Permission denied for setting the clock", x);
            }
        }

        private static DateTime GetSystemTimeUtcWindows()
        {
            Interop.SystemTime st;

            DateTime dt;
            try
            {
                if (!Interop.GetSystemTime(out st))
                {
                    throw new IOException("GetSystemTime returned an unspecified error");
                }

                dt = SystemTimeToDateTime(ref st);
            }
            catch (System.Security.SecurityException x)
            {
                throw new UnauthorizedAccessException("Permission denied for getting the clock", x);
            }

            return dt;
        }

        private static Interop.SystemTime DateTimeToSystemTime(DateTime dt)
        {
            Interop.SystemTime st;

            st.Year = (ushort)dt.Year;
            st.Day = (ushort)dt.Day;
            st.Month = (ushort)dt.Month;
            st.Hour = (ushort)dt.Hour;
            st.Minute = (ushort)dt.Minute;
            st.Second = (ushort)dt.Second;
            st.Milliseconds = (ushort)dt.Millisecond;
            st.DayOfWeek = (ushort)dt.DayOfWeek;

            return st;
        }

        private static DateTime SystemTimeToDateTime(ref Interop.SystemTime st)
        {
            return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);
        }

        private static DateTime GetDateTimeUtcUnix()
        {
            string date = RunDateCommandUnix("-u +%s.%N", false); // Floating point seconds since epoch
            if (Double.TryParse(date, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                DateTime dt = UnixEpochStart + TimeSpan.FromSeconds(result);
                return dt;
            }

            throw new IOException($"The return value of the date command could not be parsed: {date} (seconds since 01/01/1970)");
        }

        private static void SetDateTimeUtcUnix(DateTime dt)
        {
            string formattedTime = dt.ToString("yyyy-MM-dd HH:mm:ss.fffff", CultureInfo.InvariantCulture);
            RunDateCommandUnix($"-u -s \"{formattedTime}\"", true);
        }

        private static string RunDateCommandUnix(string commandLine, bool asRoot)
        {
            var si = new ProcessStartInfo()
            {
                FileName = asRoot ? "sudo" : "date",
                Arguments = asRoot ? "-n date " + commandLine : commandLine,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var process = new Process();
            string outputData;
            process.StartInfo = si;
            {
                process.Start();
                outputData = process.StandardOutput.ReadToEnd();
                var errorData = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new UnauthorizedAccessException($"Error running date command. Error {process.ExitCode}: {errorData}");
                }
            }

            return outputData;
        }
    }
}

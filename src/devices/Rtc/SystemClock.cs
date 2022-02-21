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
using System.Runtime.InteropServices;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Contains methods to access and update the system real time clock ("Bios clock")
    /// </summary>
    public class SystemClock : RtcBase
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1);

        /// <summary>
        /// We always use UTC when reading/writing the system clock, makes things easier.
        /// Technically, the BIOS RTC is configured in local time by default on Windows, but in UTC on Linux (causing
        /// weird effects when dual-booting). Both systems allow changing this setting, though.
        /// </summary>
        public override TimeZoneInfo LocalTimeZone
        {
            get
            {
                return TimeZoneInfo.Utc;
            }
            set
            {
                if (!value.Equals(TimeZoneInfo.Utc))
                {
                    throw new NotSupportedException(
                        "The time zone configuration for the system clock cannot be changed");
                }
            }
        }

        /// <summary>
        /// Set the system time to the given date/time.
        /// The time must be given in utc.
        /// The method requires elevated permissions. On Windows, the calling user must either be administrator or the right
        /// "Change the system clock" must have been granted to the "Users" group (in Security policy management).
        /// On Unix and MacOs, the current user must be root or the "date" command must have the setUid bit set.
        /// </summary>
        /// <remarks>
        /// This method is primarily intended for setting the system clock from an external clock source, such as a DS1307 or a GNSS source when no
        /// internet connection is available. If an internet connection is available, most operating systems will by default automatically sync the
        /// time to a network server, which might interfere with this operation. So when using this method, the clock synchronization should be disabled,
        /// or it should only be done if the time difference is large.
        /// </remarks>
        /// <param name="dt">Date/time to set the system clock to. This must be in UTC</param>
        /// <exception cref="PlatformNotSupportedException">This method is not supported on this platform</exception>
        /// <exception cref="IOException">There was an error executing a system command</exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have permissions to set the system clock</exception>
        public static void SetSystemTimeUtc(DateTime dt)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetSystemTimeUtcWindows(dt);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SetDateTimeUtcUnix(dt);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                SetDateTimeUtcMacOs(dt);
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetSystemTimeUtcWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetDateTimeUtcUnix();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetDateTimeUtcMacOs();
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
                // Let's be exhaustive here, because without google, it's next to impossible to find this setting.
                throw new UnauthorizedAccessException("Permission denied for setting the clock. Either run this program with elevated permissions or make sure " +
                                                      "the current user has permission to change the clock. Open 'gpedit.msc' and go to Computer Configuration > Windows Settings > " +
                                                      "Security Settings > Local Policies > User Rights Assignments and add the user or his group to the setting " +
                                                      "'Change System Time'.", x);
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
            string date = RunDateCommandUnix("-u +%s.%N", out _); // Floating point seconds since epoch
            if (Double.TryParse(date, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                DateTime dt = UnixEpochStart + TimeSpan.FromSeconds(result);
                return dt;
            }

            throw new IOException($"The return value of the date command could not be parsed: {date} (seconds since 01/01/1970)");
        }

        private static DateTime GetDateTimeUtcMacOs()
        {
            string date = RunDateCommandUnix("-u +%s", out _); // Floating point seconds since epoch
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
            int exitCode;
            // Try to run the date command as user first (maybe it has the set-user-id bit set) otherwise, try root.
            string output = RunDateCommandUnix($"-u -s \"{formattedTime}\"", out exitCode);
            if (exitCode != 0)
            {
                throw new UnauthorizedAccessException($"Error running date command. Error {exitCode}: {output}");
            }
        }

        private static void SetDateTimeUtcMacOs(DateTime dt)
        {
            // The format is "[[[mm]dd]HH]MM[[cc]yy][.ss]" from https://www.unix.com/man-page/osx/1/date/ - pretty weird to do this without delimiters
            string formattedTime = dt.ToString("MMddHHmmyyyy.ss", CultureInfo.InvariantCulture);
            int exitCode;
            // Try to run the date command as user. If user doesn't have permissions, then command will fail and we throw UnauthorizedAccessException
            string output = RunDateCommandUnix($"{formattedTime}", out exitCode);
            if (exitCode != 0)
            {
                throw new UnauthorizedAccessException($"Error running date command. Error {exitCode}: {output}. Either run this program as root or ensure " +
                                                      $"/bin/date has the setuid bit set (execute 'chmod +s /bin/date' once as root)");
            }
        }

        private static string RunDateCommandUnix(string commandLine, out int exitCode)
        {
            var si = new ProcessStartInfo()
            {
                FileName = "date",
                Arguments = commandLine,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var process = new Process();
            string outputData;
            process.StartInfo = si;

            process.Start();
            outputData = process.StandardOutput.ReadToEnd();
            var errorData = process.StandardError.ReadToEnd();
            process.WaitForExit();

            exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                outputData += errorData;
            }

            return outputData;
        }

        /// <inheritdoc />
        protected override void SetTime(DateTime time)
        {
            SetSystemTimeUtc(time);
        }

        /// <inheritdoc />
        protected override DateTime ReadTime()
        {
            var dt = GetSystemTimeUtc();
            return dt;
        }
    }
}

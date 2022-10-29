// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.IO.Path))]
    internal static partial class MiniPath
    {
        [ArduinoImplementation]
        public static char[] GetInvalidFileNameChars() => new char[] { '\0', '/' };

        [ArduinoImplementation]
        public static char[] GetInvalidPathChars() => new char[] { '\0' };

        [ArduinoImplementation]
        private static string RemoveLongPathPrefix(string path)
        {
            return path; // nop.  There's nothing special about "long" paths on Unix.
        }

        [ArduinoImplementation]
        public static string GetTempPath()
        {
            const string TempEnvVar = "TMPDIR";
            const string DefaultTempPath = "/tmp/";

            // Get the temp path from the TMPDIR environment variable.
            // If it's not set, just return the default path.
            // If it is, return it, ensuring it ends with a slash.
            string? path = Environment.GetEnvironmentVariable(TempEnvVar);
            return
                string.IsNullOrEmpty(path) ? DefaultTempPath :
                MiniPathInternal.IsDirectorySeparator(path[path.Length - 1]) ? path :
                path + MiniPathInternal.DirectorySeparatorChar;
        }

        [ArduinoImplementation]
        public static string GetTempFileName()
        {
            const string Suffix = ".tmp";
            const int SuffixByteLength = 4;

            // mkstemps takes a char* and overwrites the XXXXXX with six characters
            // that'll result in a unique file name.
            string template = GetTempPath() + "tmpXXXXXX" + Suffix + "\0";
            byte[] name = Encoding.UTF8.GetBytes(template);

            // Create, open, and close the temp file.
            IntPtr fd = MiniInterop.CheckIo(MiniInterop.Sys.MksTemps(name, SuffixByteLength));
            MiniInterop.Sys.Close(fd); // ignore any errors from close; nothing to do if cleanup isn't possible

            // 'name' is now the name of the file
            return Encoding.UTF8.GetString(name, 0, name.Length - 1); // trim off the trailing '\0'
        }

        [ArduinoImplementation]
        public static bool IsPathRooted(
#if NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        string? path)
        {
            if (path == null)
            {
                return false;
            }

            return IsPathRooted(path.AsSpan());
        }

        [ArduinoImplementation]
        public static bool IsPathRooted(ReadOnlySpan<char> path)
        {
            return path.Length > 0 && path[0] == MiniPathInternal.DirectorySeparatorChar;
        }

        /// <summary>
        /// Returns the path root or null if path is empty or null.
        /// </summary>
        [ArduinoImplementation]
        public static string? GetPathRoot(string? path)
        {
            if (MiniPathInternal_File.IsEffectivelyEmpty(path))
            {
                return null;
            }

            return IsPathRooted(path) ? MiniPathInternal.DirectorySeparatorCharAsString : string.Empty;
        }

        [ArduinoImplementation]
        public static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
        {
            return IsPathRooted(path) ? MiniPathInternal.DirectorySeparatorCharAsString.AsSpan() : ReadOnlySpan<char>.Empty;
        }

        [ArduinoImplementation]
        public static string GetFullPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("SR.Arg_PathEmpty", nameof(path));
            }

            if (path.Contains('\0'))
            {
                throw new ArgumentException("SR.Argument_InvalidPathChars", nameof(path));
            }

            // Expand with current directory if necessary
            if (!IsPathRooted(path))
            {
                path = Path.Combine(MiniInterop.Sys.GetCwd(), path);
            }

            // We would ideally use realpath to do this, but it resolves symlinks, requires that the file actually exist,
            // and turns it into a full path, which we only want if fullCheck is true.
            string collapsedString = MiniPathInternal.RemoveRelativeSegments(path, MiniPathInternal.GetRootLength(path));

            string result = collapsedString.Length == 0 ? MiniPathInternal.DirectorySeparatorCharAsString : collapsedString;

            return result;
        }

        /// <summary>Gets whether the system is case-sensitive.</summary>
        internal static bool IsCaseSensitive
        {
            [ArduinoImplementation]
            get
            {
                return true;
            }
        }
    }
}

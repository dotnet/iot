using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Device.Ports.SerialPort.Tests.Utilities
{
    internal static class FileUtilities
    {
        private static string GetFallbackGuid()
            => Guid.NewGuid().ToString("N").Substring(0, 10);

        /// <summary>Gets a test file full path that is associated with the call site.</summary>
        /// <param name="index">An optional index value to use as a suffix on the file name.  Typically a loop index.</param>
        /// <param name="memberName">The member name of the function calling this method.</param>
        /// <param name="lineNumber">The line number of the function calling this method.</param>
        public static string GetTestFilePath(int? index = null, [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0) =>
            Path.Combine(TestDirectory, GetTestFileName(index, memberName, lineNumber));

        /// <summary>Gets a test file name that is associated with the call site.</summary>
        /// <param name="index">An optional index value to use as a suffix on the file name.  Typically a loop index.</param>
        /// <param name="memberName">The member name of the function calling this method.</param>
        /// <param name="lineNumber">The line number of the function calling this method.</param>
        public static string GetTestFileName(int? index = null, [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            string testFileName = GenerateTestFileName(index, memberName, lineNumber);
            string testFilePath = Path.Combine(TestDirectory, testFileName);

            const int maxLength = 260 - 5; // Windows MAX_PATH minus a bit

            int excessLength = testFilePath.Length - maxLength;

            if (excessLength > 0)
            {
                // The path will be too long for Windows -- can we
                // trim memberName to fix it?
                if (excessLength < memberName.Length + "...".Length)
                {
                    // Take a chunk out of the middle as perhaps it's the least interesting part of the name
                    int halfMemberNameLength = (int)Math.Floor((double)memberName.Length / 2);
                    int halfExcessLength = (int)Math.Ceiling((double)excessLength / 2);
                    memberName = memberName.Substring(0, halfMemberNameLength - halfExcessLength) + "..." + memberName.Substring(halfMemberNameLength + halfExcessLength);

                    testFileName = GenerateTestFileName(index, memberName, lineNumber);
                    testFilePath = Path.Combine(TestDirectory, testFileName);
                }
                else
                {
                    return GetFallbackGuid();
                }
            }

            Debug.Assert(testFilePath.Length <= maxLength + "...".Length, "Incorrect path length");

            return testFileName;
        }

        public static string GenerateTestFileName([CallerMemberName] string? memberName = null, [CallerLineNumber] int lineNumber = 0)
            => GenerateTestFileName(null, memberName, lineNumber);

        public static string GenerateTestFileName(int? index, string? memberName, int lineNumber) =>
            string.Format(
                index.HasValue ? "{0}_{1}_{2}_{3}" : "{0}_{1}_{3}",
                memberName ?? "TestBase",
                lineNumber,
                index.GetValueOrDefault(),
                Guid.NewGuid().ToString("N").Substring(0, 8)); // randomness to avoid collisions between derived test classes using same base method concurrently
    }
}

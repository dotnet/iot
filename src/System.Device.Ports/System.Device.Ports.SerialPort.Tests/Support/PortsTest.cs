// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Legacy.Support;
using Xunit;

namespace System.IO.PortsTests
{
    // : FileCleanupTestBase
    public class PortsTest
    {
        private string? _testDirectory = null;
        private string _fallbackGuid = Guid.NewGuid().ToString("N").Substring(0, 10);

        public static bool HasOneSerialPort => TCSupport.SufficientHardwareRequirements(TCSupport.SerialPortRequirements.OneSerialPort);

        public static bool HasTwoSerialPorts => TCSupport.SufficientHardwareRequirements(TCSupport.SerialPortRequirements.TwoSerialPorts);

        public static bool HasLoopback => TCSupport.SufficientHardwareRequirements(TCSupport.SerialPortRequirements.Loopback);

        public static bool HasNullModem => TCSupport.SufficientHardwareRequirements(TCSupport.SerialPortRequirements.NullModem);

        public static bool HasLoopbackOrNullModem => TCSupport.SufficientHardwareRequirements(TCSupport.SerialPortRequirements.LoopbackOrNullModem);

        /// <summary>
        /// Shows that we can retain a single byte in the transmit queue if flow control doesn't permit transmission
        /// This is true for traditional PC ports, but will be false if there is additional driver/hardware buffering in the system
        /// </summary>
        public static bool HasSingleByteTransmitBlocking => TCSupport.HardwareTransmitBufferSize == 0;

        /// <summary>
        /// Shows that we can inhibit transmission using hardware flow control
        /// Some kinds of virtual port or RS485 adapter can't do this
        /// </summary>
        public static bool HasHardwareFlowControl => TCSupport.HardwareWriteBlockingAvailable;

        public static void Fail(string format, params object[] args)
        {
            Assert.True(false, string.Format(format, args));
        }

#pragma warning disable SYSLIB0001 // Encoding.UTF7 property is obsolete
        protected static Encoding LegacyUTF7Encoding => Encoding.UTF7;
#pragma warning restore SYSLIB0001

        /// <summary>
        /// Returns a value stating whether <paramref name="encoding"/> is UTF-7.
        /// </summary>
        /// <remarks>
        /// This method checks only for the code page 65000.
        /// </remarks>
        internal static bool IsUTF7Encoding(Encoding encoding)
        {
            return (encoding.CodePage == LegacyUTF7Encoding.CodePage);
        }

        protected virtual string GetTestFilePath(int? index = null,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
            => Path.Combine(GetTestDirectory(), GetTestFileName(index, memberName, lineNumber));

        private string GetTestDirectory()
        {
            if (_testDirectory == null)
            {
                var tempDirectory = Path.GetTempPath();

                // Use a unique test directory per test class.  The test directory lives in the user's temp directory,
                // and includes both the name of the test class and a random string.  The test class name is included
                // so that it can be easily correlated if necessary, and the random string to helps avoid conflicts if
                // the same test should be run concurrently with itself (e.g. if a [Fact] method lives on a base class)
                // or if some stray files were left over from a previous run.

                // Make 3 attempts since we have seen this on rare occasions fail with access denied, perhaps due to machine
                // configuration, and it doesn't make sense to fail arbitrary tests for this reason.
                string failure = string.Empty;
                for (int i = 0; i <= 2; i++)
                {
                    _testDirectory = Path.Combine(tempDirectory, GetType().Name + "_" + Path.GetRandomFileName());
                    try
                    {
                        Directory.CreateDirectory(_testDirectory);
                        break;
                    }
                    catch (Exception ex)
                    {
                        failure += ex.ToString() + Environment.NewLine;
                        Thread.Sleep(10); // Give a transient condition like antivirus/indexing a chance to go away
                    }
                }
            }

            if (_testDirectory == null)
            {
                Fail($"Cannot create a valid Test Directory");
                return string.Empty;
            }

            return _testDirectory;
        }

        protected string GetTestFileName(int? index = null,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            string testFileName = PathGenerator.GenerateTestFileName(index, memberName, lineNumber);
            string testFilePath = Path.Combine(GetTestDirectory(), testFileName);

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

                    testFileName = PathGenerator.GenerateTestFileName(index, memberName, lineNumber);
                    testFilePath = Path.Combine(GetTestDirectory(), testFileName);
                }
                else
                {
                    return _fallbackGuid;
                }
            }

            Debug.Assert(testFilePath.Length <= maxLength + "...".Length, "The Test File Path is too long");

            return testFileName;
        }

        public static class PathGenerator
        {
            public static string GenerateTestFileName([CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0)
                => GenerateTestFileName(null, memberName, lineNumber);

            public static string GenerateTestFileName(int? index, string memberName, int lineNumber) =>
                string.Format(
                    index.HasValue ? "{0}_{1}_{2}_{3}" : "{0}_{1}_{3}",
                    memberName ?? "TestBase",
                    lineNumber,
                    index.GetValueOrDefault(),
                    Guid.NewGuid().ToString("N").Substring(0, 8)); // randomness to avoid collisions between derived test classes using same base method concurrently
        }

    }
}

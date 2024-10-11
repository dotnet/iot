// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.Arduino;
using Microsoft.Win32.SafeHandles;

#pragma warning disable CA1416 // Function is only available on Windows (Oh, well, what a coincidence that we're mimicking the windows kernel here...)
namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+Kernel32", "System.Private.CoreLib.dll", true, IncludingPrivates = true)]
        internal static partial class Kernel32
        {
            internal const uint LOCALE_ALLOW_NEUTRAL_NAMES = 0x08000000; // Flag to allow returning neutral names/lcids for name conversion
            internal const uint LOCALE_ILANGUAGE = 0x00000001;
            internal const uint LOCALE_SUPPLEMENTAL = 0x00000002;
            internal const uint LOCALE_REPLACEMENT = 0x00000008;
            internal const uint LOCALE_NEUTRALDATA = 0x00000010;
            internal const uint LOCALE_SPECIFICDATA = 0x00000020;
            internal const uint LOCALE_SISO3166CTRYNAME = 0x0000005A;
            internal const uint LOCALE_SNAME = 0x0000005C;
            internal const uint LOCALE_INEUTRAL = 0x00000071;
            internal const uint LOCALE_SSHORTTIME = 0x00000079;
            internal const uint LOCALE_ICONSTRUCTEDLOCALE = 0x0000007d;
            internal const uint LOCALE_STIMEFORMAT = 0x00001003;
            internal const uint LOCALE_IFIRSTDAYOFWEEK = 0x0000100C;
            internal const uint LOCALE_RETURN_NUMBER = 0x20000000;
            internal const uint LOCALE_NOUSEROVERRIDE = 0x80000000;

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static unsafe bool GetThreadIOPendingFlag(System.IntPtr hThread, out bool lpIOIsPending)
            {
                lpIOIsPending = false;
                return true;
            }

            [ArduinoImplementation("Interop_Kernel32GetCurrentThreadNative")]
            public static int GetCurrentThread()
            {
                return 1;
            }

            public static unsafe uint GetFullPathNameW(ref Char lpFileName, UInt32 nBufferLength, ref Char lpBuffer, IntPtr lpFilePart)
            {
                throw new NotImplementedException();
            }

            private static unsafe int AssignCharData(char* value, int valueLength, string data)
            {
                if (valueLength < data.Length + 1)
                {
                    return data.Length + 1;
                }

                int i = 0;
                for (i = 0; i < data.Length; i++)
                {
                    value[i] = data[i];
                }

                value[i] = '\0';

                return data.Length + 1;
            }

            private static unsafe int AssignNumber(char* value, int valueLength, ushort number)
            {
                if (valueLength < 2)
                {
                    return 2;
                }

                // This actually returns a DWORD in a place where a string would normally be. Don't ask me who designed an interface this way
                ushort* ptr = (ushort*)value;
                *ptr = number;
                return 2;
            }

            public static unsafe int GetLocaleInfoEx(string lpLocaleName, uint lcType, void* lpLCData, int cchData)
            {
                return GetLocaleInfoEx(lpLocaleName, lcType, (char*)lpLCData, cchData);
            }

            public static unsafe int GetLocaleInfoEx(string lpLocaleName, uint lcType, char* lpLCData, int cchData)
            {
                uint typeToQuery = lcType & 0xFFFF; // Ignore high-order bits
                bool returnNumber = (lcType & LOCALE_RETURN_NUMBER) != 0;
                switch (typeToQuery)
                {
                    case LOCALE_ICONSTRUCTEDLOCALE:
                        if (returnNumber)
                        {
                            return AssignNumber(lpLCData, cchData, 0);
                        }

                        return AssignCharData(lpLCData, cchData, "0");
                    case LOCALE_ILANGUAGE:
                    case LOCALE_IFIRSTDAYOFWEEK:
                        if (returnNumber)
                        {
                            return AssignNumber(lpLCData, cchData, 0);
                        }

                        return AssignCharData(lpLCData, cchData, "0");
                    case LOCALE_INEUTRAL:
                        if (returnNumber)
                        {
                            return AssignNumber(lpLCData, cchData, 1);
                        }

                        return AssignCharData(lpLCData, cchData, "1");
                    case LOCALE_SNAME:
                        return AssignCharData(lpLCData, cchData, "World");
                    case LOCALE_SISO3166CTRYNAME:
                        return AssignCharData(lpLCData, cchData, "WRL");

                    default:
                        throw new NotSupportedException();
                }
            }

            internal static unsafe int CompareStringEx(
                char* lpLocaleName,
                uint dwCmpFlags,
                char* lpString1,
                int cchCount1,
                char* lpString2,
                int cchCount2,
                void* lpVersionInformation,
                void* lpReserved,
                IntPtr lParam)
            {
                throw new NotImplementedException();
            }

            internal static unsafe int CompareStringOrdinal(
                char* lpString1,
                int cchCount1,
                char* lpString2,
                int cchCount2,
                bool bIgnoreCase)
            {
                throw new NotImplementedException();
            }

            internal static unsafe int LCMapStringEx(string lpLocaleName, uint dwMapFlags, char* lpSrcStr, int cchsrc, void* lpDestStr,
                int cchDest, void* lpVersionInformation, void* lpReserved, IntPtr sortHandle)
            {
                return 0; // Can apparently be null in InvariantCulture mode.
            }

            internal static int FindNLSString(
                int locale,
                uint flags,
                [MarshalAs(UnmanagedType.LPWStr)] string sourceString,
                int sourceCount,
                [MarshalAs(UnmanagedType.LPWStr)] string findString,
                int findCount,
                out int found)
            {
                // NLS is not active, we should never really get here.
                throw new NotImplementedException();
            }

            internal static unsafe int FindNLSStringEx(
                char* lpLocaleName,
                uint dwFindNLSStringFlags,
                char* lpStringSource,
                int cchSource,
                char* lpStringValue,
                int cchValue,
                int* pcchFound,
                void* lpVersionInformation,
                void* lpReserved,
                IntPtr sortHandle)
            {
                // NLS is not active, we should never really get here.
                throw new NotImplementedException();
            }

            [ArduinoImplementation("InteropQueryPerformanceFrequency", 0x200)]
            internal static unsafe bool QueryPerformanceFrequency(long* lpFrequency)
            {
                return true;
            }

            [ArduinoImplementation("InteropQueryPerformanceCounter", 0x201)]
            internal static unsafe bool QueryPerformanceCounter(long* lpCounter)
            {
                return true;
            }

            internal static unsafe uint GetTempPathW(int bufferLen, ref char buffer)
            {
                // We require 5 chars, including the terminating 0
                if (bufferLen < 5)
                {
                    return 5;
                }

                char* ptr = (char*)MiniUnsafe.AsPointer(ref buffer);
                ptr[0] = '/';
                ptr[1] = 't';
                ptr[2] = 'm';
                ptr[3] = 'p';
                ptr[4] = '\0';
                return 4; // the return value is the number of chars copied, not including the 0
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static unsafe Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(System.String lpFileName, System.Int32 dwDesiredAccess,
                System.IO.FileShare dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, System.IO.FileMode dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
            {
                IntPtr file = CreateFileInternal(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
                if (file == IntPtr.Zero)
                {
                    throw new IOException("IO Error", (int)GetLastError());
                }

                return new SafeFileHandle(file, true);
            }

            [ArduinoImplementation("Interop_Kernel32CreateFile", 0x202, CompareByParameterNames = true)]
            internal static unsafe IntPtr CreateFileInternal(System.String lpFileName, System.Int32 dwDesiredAccess,
                System.IO.FileShare dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, System.IO.FileMode dwCreationDisposition, System.Int32 dwFlagsAndAttributes, System.IntPtr hTemplateFile)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            internal static unsafe IntPtr CreateFile_IntPtr(
                string lpFileName,
                int dwDesiredAccess,
                FileShare dwShareMode,
                FileMode dwCreationDisposition,
                int dwFlagsAndAttributes)
            {
                return CreateFileInternal(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
            }

            [ArduinoImplementation("Interop_Kernel32SetFilePointerEx", 0x203)]
            internal static System.Boolean SetFilePointerEx(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, System.Int64 liDistanceToMove, ref System.Int64 lpNewFilePointer, System.UInt32 dwMoveMethod)
            {
                throw new NotImplementedException();
            }

            internal static System.Boolean SetThreadErrorMode(System.UInt32 dwNewMode, ref System.UInt32 lpOldMode)
            {
                return true;
            }

            [ArduinoImplementation("Interop_Kernel32CloseHandle", 0x204)]
            internal static System.Boolean CloseHandle(System.IntPtr handle)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static string GetMessage(int errorCode)
            {
                // We don't have the resources for the full messages available
                return string.Format("OS error (0x{0:x})", errorCode);
            }

            [ArduinoImplementation]
            public static string GetMessage(int errorCode, IntPtr moduleHandle)
            {
                return string.Format("OS error (0x{0:x})", errorCode);
            }

            [ArduinoImplementation("Interop_Kernel32SetLastError", 0x205)]
            internal static void SetLastError(uint errorCode)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32GetLastError", 0x206)]
            internal static uint GetLastError()
            {
                throw new NotImplementedException();
            }

            internal static int GetFileType(SafeHandle hFile)
            {
                SetLastError(0);
                return 1; // Only disk files supported
            }

            [ArduinoImplementation("Interop_Kernel32SetEvent")]
            internal static Boolean SetEventInternal(IntPtr handle)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static Boolean SetEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
            {
                return SetEventInternal(handle.DangerousGetHandle());
            }

            [ArduinoImplementation("Interop_Kernel32ResetEvent")]
            internal static Boolean ResetEventInternal(IntPtr handle)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static Boolean ResetEvent(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
            {
                return ResetEventInternal(handle.DangerousGetHandle());
            }

            [ArduinoImplementation("Interop_Kernel32SetEndOfFile", 0x207)]
            internal static Boolean SetEndOfFile(Microsoft.Win32.SafeHandles.SafeFileHandle hFile)
            {
                return true;
            }

            [ArduinoImplementation("Interop_Kernel32WriteFile", 0x208)]
            internal static unsafe Int32 WriteFileInternal(IntPtr fileHandle, Byte* bytes, Int32 numBytesToWrite)
            {
                return 0;
            }

            internal static unsafe Int32 WriteFile(System.IntPtr handle, System.Byte* bytes, System.Int32 numBytesToWrite, ref System.Int32 numBytesWritten, System.IntPtr mustBeZero)
            {
                numBytesWritten = WriteFileInternal(handle, bytes, numBytesToWrite);
                return numBytesWritten == numBytesToWrite ? 1 : 0; // Return type is int, but the value is actually a bool
            }

            internal static unsafe Int32 WriteFile(SafeHandle handle, Byte* bytes, Int32 numBytesToWrite, ref Int32 numBytesWritten, System.IntPtr mustBeZero)
            {
                numBytesWritten = WriteFileInternal(handle.DangerousGetHandle(), bytes, numBytesToWrite);
                if (numBytesWritten < 0)
                {
                    numBytesWritten = 0;
                    return 0;
                }

                // True
                return 1;
            }

            [ArduinoImplementation]
            internal static unsafe Int32 WriteFile(System.Runtime.InteropServices.SafeHandle handle, Byte* bytes, System.Int32 numBytesToWrite, ref System.Int32 numBytesWritten, NativeOverlapped* lpOverlapped)
            {
                return WriteFile(handle.DangerousGetHandle(), bytes, numBytesToWrite, ref numBytesWritten, lpOverlapped->OffsetLow);
            }

            [ArduinoImplementation("Interop_Kernel32WriteFileOverlapped2", 0x210)]
            internal static unsafe Int32 WriteFile(IntPtr handle, Byte* bytes, System.Int32 numBytesToWrite, ref System.Int32 numBytesWritten, Int32 offset)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32WriteFileOverlapped", 0x209)]
            internal static unsafe Int32 WriteFile(System.Runtime.InteropServices.SafeHandle handle, Byte* bytes, System.Int32 numBytesToWrite, System.IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32ReadFileOverlapped2", 0x211)]
            internal static unsafe Int32 ReadFile(System.Runtime.InteropServices.SafeHandle handle, Byte* bytes, System.Int32 numBytesToReade, ref Int32 numBytesRead, NativeOverlapped* lpOverlapped)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32GetOverlappedResult", 0x212)]
            internal static unsafe bool GetOverlappedResult(
                SafeFileHandle hFile,
                NativeOverlapped* lpOverlapped,
                ref int lpNumberOfBytesTransferred,
                bool bWait)
            {
                throw new NotImplementedException();
            }

            // TODO: Probably better rewrite managed
            [ArduinoImplementation("Interop_Kernel32CreateEventEx", 0x213)]
            internal static IntPtr CreateEventExInternal(string name, uint flags, uint desiredAccess)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            internal static SafeWaitHandle CreateEventEx(
                IntPtr lpSecurityAttributes,
                string name,
                uint flags,
                uint desiredAccess)
            {
                return new SafeWaitHandle(CreateEventExInternal(name, flags, desiredAccess), true);
            }

            [ArduinoImplementation("Interop_Kernel32CreateIoCompletionPort", 0x214)]
            internal static IntPtr CreateIoCompletionPort(
                IntPtr FileHandle,
                IntPtr ExistingCompletionPort,
                UIntPtr CompletionKey,
                int NumberOfConcurrentThreads)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            internal static SafeWaitHandle OpenMutex(
                uint desiredAccess,
                bool inheritHandle,
                string name)
            {
                throw new NotImplementedException();
            }

            internal static SafeWaitHandle CreateMutexEx(
                IntPtr lpMutexAttributes,
                string name,
                uint flags,
                uint desiredAccess)
            {
                throw new NotImplementedException();
            }

            internal static bool ReleaseMutex(SafeWaitHandle handle)
            {
                throw new NotImplementedException();
            }

            internal static IntPtr LoadLibraryEx(String libFileName, IntPtr reserved, Int32 flags)
            {
                return IntPtr.Zero;
            }

            internal static bool FreeLibrary(IntPtr hModule)
            {
                return true;
            }

            [ArduinoImplementation("Interop_Kernel32ReadFile", 0x20A)]
            internal static unsafe Int32 ReadFileInternal(IntPtr fileHandle, Byte* bytes, System.Int32 numBytesToRead)
            {
                return 0;
            }

            internal static unsafe Int32 ReadFile(IntPtr handle, Byte* bytes, System.Int32 numBytesToRead, ref System.Int32 numBytesRead, System.IntPtr mustBeZero)
            {
                numBytesRead = ReadFileInternal(handle, bytes, numBytesToRead);
                if (numBytesRead < 0)
                {
                    numBytesRead = 0;
                    return 0;
                }

                return 1;
            }

            internal static unsafe Int32 ReadFile(System.Runtime.InteropServices.SafeHandle handle, Byte* bytes, System.Int32 numBytesToRead, ref System.Int32 numBytesRead, System.IntPtr mustBeZero)
            {
                numBytesRead = ReadFileInternal(handle.DangerousGetHandle(), bytes, numBytesToRead);
                if (numBytesRead < 0)
                {
                    numBytesRead = 0;
                    return 0;
                }

                return 1;
            }

            internal static unsafe bool ReadConsole(System.IntPtr hConsoleInput, Byte* lpBuffer, Int32 nNumberOfCharsToRead, ref Int32 lpNumberOfCharsRead, System.IntPtr pInputControl)
            {
                lpNumberOfCharsRead = 0;
                return true;
            }

            [ArduinoImplementation("Interop_Kernel32CancelIoEx", 0x20B)]
            internal static unsafe Boolean CancelIoEx(System.Runtime.InteropServices.SafeHandle handle, System.Threading.NativeOverlapped* lpOverlapped)
            {
                return false;
            }

            [ArduinoImplementation("Interop_Kernel32ReadFileOverlapped", 0x20C)]
            internal static unsafe System.Int32 ReadFile(System.Runtime.InteropServices.SafeHandle handle, System.Byte* bytes, System.Int32 numBytesToRead, System.IntPtr numBytesRead_mustBeZero, System.Threading.NativeOverlapped* overlapped)
            {
                return 0;
            }

            [ArduinoImplementation("Interop_Kernel32FlushFileBuffers", 0x20D)]
            internal static Boolean FlushFileBuffers(System.Runtime.InteropServices.SafeHandle hHandle)
            {
                return false;
            }

            [ArduinoImplementation]
            internal static unsafe Boolean GetFileInformationByHandleEx(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, System.Int32 FileInformationClass, void* lpFileInformation, System.UInt32 dwBufferSize)
            {
                return GetFileInformationByHandleExInternal(hFile.DangerousGetHandle(), FileInformationClass, lpFileInformation, dwBufferSize);
            }

            [ArduinoImplementation("Interop_Kernel32GetFileInformationByHandleEx", 0x20E)]
            public static unsafe Boolean GetFileInformationByHandleExInternal(IntPtr hFile, System.Int32 FileInformationClass, void* lpFileInformation, System.UInt32 dwBufferSize)
            {
                return false;
            }

            [ArduinoImplementation("Interop_Kernel32QueryUnbiasedInterruptTime", 0x20F)]
            internal static System.Boolean QueryUnbiasedInterruptTime(ref System.UInt64 UnbiasedTime)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32FindStringOrdinal", 0x215, CompareByParameterNames = true)]
            internal static unsafe int FindStringOrdinal(
                uint dwFindStringOrdinalFlags,
                char* lpStringSource,
                int cchSource,
                char* lpStringValue,
                int cchValue,
                bool bIgnoreCase)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32SetFileInformationByHandle", 0x216)]
            internal static unsafe bool SetFileInformationByHandle(
                SafeFileHandle hFile,
                int FileInformationClass,
                void* lpFileInformation,
                uint dwBufferSize)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32DeleteFile", 0x217)]
            internal static bool DeleteFile(string path)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32InitializeCriticalSection", 0x218, CompareByParameterNames = true)]
            internal static unsafe void InitializeCriticalSection(
                CRITICAL_SECTION* lpCriticalSection)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32EnterCriticalSection", 0x219, CompareByParameterNames = true)]
            internal static unsafe void EnterCriticalSection(
                CRITICAL_SECTION* lpCriticalSection)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32LeaveCriticalSection", 0x21A, CompareByParameterNames = true)]
            internal static unsafe void LeaveCriticalSection(
                CRITICAL_SECTION* lpCriticalSection)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32DeleteCriticalSection", 0x21B, CompareByParameterNames = true)]
            internal static unsafe void DeleteCriticalSection(
                CRITICAL_SECTION* lpCriticalSection)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32SleepConditionVariableCS", 0x21C, CompareByParameterNames = true)]
            internal static unsafe bool SleepConditionVariableCS(
                CONDITION_VARIABLE* ConditionVariable,
                CRITICAL_SECTION* CriticalSection,
                int dwMilliseconds)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32InitializeConditionVariable", 0x21D, CompareByParameterNames = true)]
            internal static unsafe void InitializeConditionVariable(
                CONDITION_VARIABLE* ConditionVariable)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32WakeConditionVariable", 0x21E, CompareByParameterNames = true)]
            internal static unsafe void WakeConditionVariable(
                CONDITION_VARIABLE* ConditionVariable)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation("Interop_Kernel32GetFileType", 0x21F)]
            internal static uint GetFileType(System.IntPtr handle)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            internal static bool GetSystemTimes(out long idle, out long kernel, out long user)
            {
                idle = 0;
                kernel = 0;
                user = 0;
                return true;
            }

            [ArduinoImplementation]
            public static bool PostQueuedCompletionStatus(
                IntPtr CompletionPort,
                uint dwNumberOfBytesTransferred,
                UIntPtr CompletionKey,
                IntPtr lpOverlapped)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static bool GetQueuedCompletionStatus(
                IntPtr CompletionPort,
                out uint lpNumberOfBytesTransferred,
                out UIntPtr CompletionKey,
                out IntPtr lpOverlapped,
                int dwMilliseconds)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal static unsafe bool GetQueuedCompletionStatusEx(System.IntPtr CompletionPort, void* lpCompletionPortEntries,
                System.Int32 ulCount, ref System.Int32 ulNumEntriesRemoved, System.Int32 dwMilliseconds, System.Boolean fAlertable)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static unsafe SafeHandle CreateThreadpoolIo(SafeHandle fl, void* pfnio, IntPtr context, IntPtr pcbe)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static void CancelThreadpoolIo(SafeHandle pio)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static void StartThreadpoolIo(SafeHandle pio)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static unsafe System.Boolean DeviceIoControl(SafeHandle hDevice, UInt32 dwIoControlCode, void* lpInBuffer,
                UInt32 nInBufferSize, void* lpOutBuffer, UInt32 nOutBufferSize, ref UInt32 lpBytesReturned, System.IntPtr lpOverlapped)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            [ArduinoCompileTimeConstant(nameof(GetDynamicTimeZoneInformation))]
            public static unsafe uint GetDynamicTimeZoneInformation(out TIME_DYNAMIC_ZONE_INFORMATION pTimeZoneInformation)
            {
                pTimeZoneInformation = default;
                pTimeZoneInformation.Bias = 0;
                return 1;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            [ArduinoCompileTimeConstant(nameof(GetTimeZoneInformation))] // The compiler has special code to handle this method
            public static UInt32 GetTimeZoneInformation(out TIME_ZONE_INFORMATION lpTimeZoneInformation)
            {
                lpTimeZoneInformation = default;
                return 1;
            }

            private static unsafe int StrLen(byte* cstr)
            {
                int ret = 0;
                while (*cstr != 0)
                {
                    ret++;
                    cstr++;
                }

                return ret;
            }

            [ArduinoImplementation]
            internal static unsafe Int32 MultiByteToWideChar(System.UInt32 CodePage, System.UInt32 dwFlags, Byte* lpMultiByteStr, Int32 cbMultiByte,
                Char* lpWideCharStr, System.Int32 cchWideChar)
            {
                int bytesToConvert = cbMultiByte;
                if (cbMultiByte == -1)
                {
                    bytesToConvert = StrLen(lpMultiByteStr);
                }

                if (cchWideChar == 0)
                {
                    return bytesToConvert + 1;
                }

                int bytesRemaining = bytesToConvert;
                // May be -1 to run until *lpMultiByteStr is 0
                int idx = 0;
                while (bytesRemaining > 0)
                {
                    byte input = lpMultiByteStr[idx];
                    lpWideCharStr[idx] = (Char)input;
                    bytesRemaining--;
                    idx++;
                }

                return bytesToConvert;
            }

            [ArduinoImplementation]
            internal static int GetLeadByteRanges(int codePage, byte[] leadByteRanges)
            {
                /*
                int count = 0;
                CPINFOEXW cpInfo;
                if (GetCPInfoExW((uint)codePage, 0, &cpInfo) != false)
                {
                    // we don't care about the last 2 bytes as those are nulls
                    for (int i = 0; i < 10 && leadByteRanges[i] != 0; i += 2)
                    {
                        leadByteRanges[i] = cpInfo.LeadByte[i];
                        leadByteRanges[i + 1] = cpInfo.LeadByte[i + 1];
                        count++;
                    }
                }
                return count;
                */
                return 0;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static unsafe IntPtr CreateThreadpoolTimer(void* pfnti, IntPtr pv, IntPtr pcbe)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static unsafe System.IntPtr SetThreadpoolTimer(System.IntPtr pti, System.Int64* pftDueTime, System.UInt32 msPeriod, System.UInt32 msWindowLength)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static unsafe IntPtr CreateThreadpoolWork(void* pfnwk, IntPtr pv, IntPtr pcbe)
            {
                // Shouldn't be called, because we use the portable thread pool
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static void CloseThreadpoolWork(IntPtr pwk)
            {
                // Shouldn't be called, because we use the portable thread pool
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public static void SubmitThreadpoolWork(IntPtr pwk)
            {
                // Shouldn't be called, because we use the portable thread pool
                throw new NotImplementedException();
            }
        }

#pragma warning disable CS0169
#pragma warning disable SA1306
#pragma warning disable SX1309
#pragma warning disable CS0649
        internal struct CRITICAL_SECTION
        {
            public IntPtr DebugInfo;
            public int LockCount;
            public int RecursionCount;
            public IntPtr OwningThread;
            public IntPtr LockSemaphore;
            public UIntPtr SpinCount;
        }

        internal struct CONDITION_VARIABLE
        {
            public IntPtr Ptr;
        }

        internal struct SECURITY_ATTRIBUTES
        {
            public int DummyData;
        }

        internal struct SYSTEMTIME
        {
            internal ushort Year;
            internal ushort Month;
            internal ushort DayOfWeek;
            internal ushort Day;
            internal ushort Hour;
            internal ushort Minute;
            internal ushort Second;
            internal ushort Milliseconds;

            internal bool Equals(in SYSTEMTIME other) =>
                Year == other.Year &&
                Month == other.Month &&
                DayOfWeek == other.DayOfWeek &&
                Day == other.Day &&
                Hour == other.Hour &&
                Minute == other.Minute &&
                Second == other.Second &&
                Milliseconds == other.Milliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct TIME_ZONE_INFORMATION
        {
            internal int Bias;
            internal fixed char StandardName[32];
            internal SYSTEMTIME StandardDate;
            internal int StandardBias;
            internal fixed char DaylightName[32];
            internal SYSTEMTIME DaylightDate;
            internal int DaylightBias;

            internal TIME_ZONE_INFORMATION(in TIME_DYNAMIC_ZONE_INFORMATION dtzi)
            {
                // The start of TIME_DYNAMIC_ZONE_INFORMATION has identical layout as TIME_ZONE_INFORMATION
                fixed (TIME_ZONE_INFORMATION* pTo = &this)
                {
                    fixed (TIME_DYNAMIC_ZONE_INFORMATION* pFrom = &dtzi)
                    {
                        *pTo = *(TIME_ZONE_INFORMATION*)pFrom;
                    }
                }
            }

            internal string GetStandardName()
            {
                fixed (char* p = StandardName)
                {
                    return new string(p);
                }
            }

            internal string GetDaylightName()
            {
                fixed (char* p = DaylightName)
                {
                    return new string(p);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct TIME_DYNAMIC_ZONE_INFORMATION
        {
            internal int Bias;
            internal fixed char StandardName[32];
            internal SYSTEMTIME StandardDate;
            internal int StandardBias;
            internal fixed char DaylightName[32];
            internal SYSTEMTIME DaylightDate;
            internal int DaylightBias;
            internal fixed char TimeZoneKeyName[128];
            internal byte DynamicDaylightTimeDisabled;

            internal string GetTimeZoneKeyName()
            {
                fixed (char* p = TimeZoneKeyName)
                {
                    return new string(p);
                }
            }
        }
    }
}

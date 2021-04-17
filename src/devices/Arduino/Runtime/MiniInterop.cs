using System;
using System.Runtime.InteropServices;

namespace Iot.Device.Arduino.Runtime
{
    // We're not replacing the helper methods of this class itself
    [ArduinoReplacement("Interop", "System.Private.CoreLib.dll", false, IncludingPrivates = true)]
    internal class MiniInterop
    {
        internal enum LocaleNumberData
        {
            LocaleNumber_LanguageId = 0x01,
            LocaleNumber_MeasurementSystem = 0x0D,
            LocaleNumber_FractionalDigitsCount = 0x00000011,
            LocaleNumber_NegativeNumberFormat = 0x00001010,
            LocaleNumber_MonetaryFractionalDigitsCount = 0x00000019,
            LocaleNumber_PositiveMonetaryNumberFormat = 0x0000001B,
            LocaleNumber_NegativeMonetaryNumberFormat = 0x0000001C,
            LocaleNumber_FirstDayofWeek = 0x0000100C,
            LocaleNumber_FirstWeekOfYear = 0x0000100D,
            LocaleNumber_ReadingLayout = 0x00000070,
            LocaleNumber_NegativePercentFormat = 0x00000074,
            LocaleNumber_PositivePercentFormat = 0x00000075,
            LocaleNumber_Digit = 0x00000010,
            LocaleNumber_Monetary = 0x00000018
        }

        internal enum LocaleString
        {
            LocalizedDisplayName = 0x02,
            EnglishDisplayName = 0x00000072,
            NativeDisplayName = 0x00000073,
            LocalizedLanguageName = 0x0000006f,
            EnglishLanguageName = 0x00001001,
            NativeLanguageName = 0x04,
            EnglishCountryName = 0x00001002,
            NativeCountryName = 0x08,
            DecimalSeparator = 0x0E,
            ThousandSeparator = 0x0F,
            Digits = 0x00000013,
            MonetarySymbol = 0x00000014,
            CurrencyEnglishName = 0x00001007,
            CurrencyNativeName = 0x00001008,
            Iso4217MonetarySymbol = 0x00000015,
            MonetaryDecimalSeparator = 0x00000016,
            MonetaryThousandSeparator = 0x00000017,
            AMDesignator = 0x00000028,
            PMDesignator = 0x00000029,
            PositiveSign = 0x00000050,
            NegativeSign = 0x00000051,
            Iso639LanguageTwoLetterName = 0x00000059,
            Iso639LanguageThreeLetterName = 0x00000067,
            Iso3166CountryName = 0x0000005A,
            Iso3166CountryName2 = 0x00000068,
            NaNSymbol = 0x00000069,
            PositiveInfinitySymbol = 0x0000006a,
            ParentName = 0x0000006d,
            PercentSymbol = 0x00000076,
            PerMilleSymbol = 0x00000077
        }

        [ArduinoImplementation(NativeMethod.InteropGetRandomBytes)]
        public static unsafe void GetRandomBytes(byte* buffer, int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoReplacement("Interop+Globalization", "System.Private.CoreLib.dll", false, IncludingSubclasses = true, IncludingPrivates = true)]
        internal static class Globalization
        {
            [ArduinoImplementation(NativeMethod.None)]
            public static int LoadICU()
            {
                return 1; // returning a non-zero value means false, which results in UseNLS to become false, which is probably what we want
            }

            [ArduinoImplementation(NativeMethod.None)]
            internal static bool GetLocaleInfoInt(string localeName, uint localeNumberData, ref int value)
            {
                // See pal_localeNumberData.c for the original implementation of this. We just use some invariant defaults
                LocaleNumberData data = (LocaleNumberData)localeNumberData;
                switch (data)
                {
                    case LocaleNumberData.LocaleNumber_LanguageId:
                        value = 0;
                        break;
                    case LocaleNumberData.LocaleNumber_MeasurementSystem:
                        value = 0; // metric
                        break;
                    case LocaleNumberData.LocaleNumber_FractionalDigitsCount:
                        value = 2;
                        break;
                    case LocaleNumberData.LocaleNumber_NegativeNumberFormat:
                        value = 1; // -n
                        break;
                    case LocaleNumberData.LocaleNumber_MonetaryFractionalDigitsCount:
                        value = 2;
                        break;
                    case LocaleNumberData.LocaleNumber_PositiveMonetaryNumberFormat:
                        value = 2; // C n
                        break;
                    case LocaleNumberData.LocaleNumber_NegativeMonetaryNumberFormat:
                        value = 12; // C -n
                        break;
                    case LocaleNumberData.LocaleNumber_FirstWeekOfYear:
                        value = 0; // The settings in this group are not properly documented, it seems (and windows does not offer changing it manually)
                        break;
                    case LocaleNumberData.LocaleNumber_ReadingLayout:
                        value = 0; // left-to-right
                        break;
                    case LocaleNumberData.LocaleNumber_FirstDayofWeek:
                        value = 0; // Monday?
                        break;
                    case LocaleNumberData.LocaleNumber_NegativePercentFormat:
                        value = 1; // -n%
                        break;
                    case LocaleNumberData.LocaleNumber_PositivePercentFormat:
                        value = 1; // n%
                        break;
                    default:
                        value = 0;
                        return false;
                }

                return true;
            }

            private static unsafe bool AssignCharData(char* value, int valueLength, string data)
            {
                if (valueLength < data.Length + 1)
                {
                    return false;
                }

                int i = 0;
                for (i = 0; i < data.Length; i++)
                {
                    value[i] = data[i];
                }

                value[i] = '\0';

                return true;
            }

            [ArduinoImplementation(NativeMethod.None)]
            internal static unsafe bool GetLocaleInfoString(string localeName, uint localeStringData, char* value, int valueLength)
            {
                LocaleString data = (LocaleString)localeStringData;
                switch (data)
                {
                    case LocaleString.LocalizedDisplayName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.EnglishDisplayName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.NativeDisplayName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.LocalizedLanguageName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.EnglishLanguageName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.NativeLanguageName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.EnglishCountryName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.NativeCountryName:
                        return AssignCharData(value, valueLength, "Invariant");
                    case LocaleString.ThousandSeparator:
                        return AssignCharData(value, valueLength, "'");
                    case LocaleString.DecimalSeparator:
                        return AssignCharData(value, valueLength, ".");
                    case LocaleString.Digits:
                        return AssignCharData(value, valueLength, string.Empty);
                    case LocaleString.MonetarySymbol:
                        return AssignCharData(value, valueLength, "$");
                    case LocaleString.Iso4217MonetarySymbol:
                        return AssignCharData(value, valueLength, "$");
                    case LocaleString.CurrencyEnglishName:
                        return AssignCharData(value, valueLength, "Dollars");
                    case LocaleString.CurrencyNativeName:
                        return AssignCharData(value, valueLength, "Dollars");
                    case LocaleString.MonetaryDecimalSeparator:
                        return AssignCharData(value, valueLength, ".");
                    case LocaleString.MonetaryThousandSeparator:
                        return AssignCharData(value, valueLength, "'");
                    case LocaleString.AMDesignator:
                        return AssignCharData(value, valueLength, "AM");
                    case LocaleString.PMDesignator:
                        return AssignCharData(value, valueLength, "PM");
                    case LocaleString.PositiveSign:
                        return AssignCharData(value, valueLength, "+");
                    case LocaleString.NegativeSign:
                        return AssignCharData(value, valueLength, "-");
                    case LocaleString.Iso639LanguageTwoLetterName:
                        return AssignCharData(value, valueLength, "IN");
                    case LocaleString.Iso639LanguageThreeLetterName:
                        return AssignCharData(value, valueLength, "INV");
                    case LocaleString.Iso3166CountryName:
                        return AssignCharData(value, valueLength, "IN");
                    case LocaleString.Iso3166CountryName2:
                        return AssignCharData(value, valueLength, "IN");
                    case LocaleString.NaNSymbol:
                        return AssignCharData(value, valueLength, "NAN");
                    case LocaleString.PositiveInfinitySymbol:
                        return AssignCharData(value, valueLength, "infinity");
                    case LocaleString.ParentName:
                        return AssignCharData(value, valueLength, String.Empty);
                    case LocaleString.PercentSymbol:
                        return AssignCharData(value, valueLength, "%");
                    case LocaleString.PerMilleSymbol:
                        return AssignCharData(value, valueLength, "‰");
                }

                return false;
            }

            [ArduinoImplementation(NativeMethod.None)]
            internal static unsafe bool GetLocaleTimeFormat(string localeName, bool shortFormat, char* value, int valueLength)
            {
                if (shortFormat)
                {
                    return AssignCharData(value, valueLength, @"hh\:mm");
                }
                else
                {
                    return AssignCharData(value, valueLength, @"hh\:mm\:ss");
                }
            }

            [ArduinoImplementation(NativeMethod.Interop_GlobalizationGetCalendarInfo, CompareByParameterNames = true)]
            internal static unsafe int GetCalendarInfo(string localeName, int calendarId, int calendarDataType, char* result, int resultCapacity)
            {
                return 0;
            }

            [ArduinoImplementation]
            public static unsafe int ChangeCaseTurkish(char* src, int srcLen, char* dstBuffer, int dstBufferCapacity, bool bToUpper)
            {
                throw new NotImplementedException();
            }

        }

        [ArduinoReplacement("Interop+Kernel32", "System.Private.CoreLib.dll", true, IncludingSubclasses = true, IncludingPrivates = true)]
        internal class Kernel32
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
                throw new NotImplementedException();
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

            [ArduinoImplementation(NativeMethod.InteropQueryPerformanceFrequency)]
            internal static unsafe bool QueryPerformanceFrequency(long* lpFrequency)
            {
                return true;
            }

            [ArduinoImplementation(NativeMethod.InteropQueryPerformanceCounter)]
            internal static unsafe bool QueryPerformanceCounter(long* lpCounter)
            {
                return true;
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+Globalization", "System.Private.CoreLib.dll", false, IncludingSubclasses = true, IncludingPrivates = true)]
        internal static class Globalization
        {
            [ArduinoImplementation]
            public static int LoadICU()
            {
                return 1; // returning a non-zero value means false, which results in UseNLS to become false, which is probably what we want
            }

            [ArduinoImplementation]
            public static void InitICUFunctions(IntPtr icuuc, IntPtr icuin, String version, String suffix)
            {
                // Probably nothing to do
            }

            [ArduinoImplementation]
            public static unsafe void InitOrdinalCasingPage(Int32 pageNumber, char* pTarget)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
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

            [ArduinoImplementation]
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

            [ArduinoImplementation]
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

            [ArduinoImplementation("Interop_GlobalizationGetCalendarInfo", CompareByParameterNames = true)]
            internal static unsafe int GetCalendarInfo(string localeName, int calendarId, int calendarDataType, char* result, int resultCapacity)
            {
                return 0;
            }

            [ArduinoImplementation]
            public static unsafe int ChangeCaseTurkish(char* src, int srcLen, char* dstBuffer, int dstBufferCapacity, bool bToUpper)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation]
            public static unsafe bool StartsWith(IntPtr sortHandle, Char* target, Int32 cwTargetLength, Char* source, Int32 cwSourceLength, CompareOptions options, Int32* matchedLength)
            {
                // target is the element to search for, source the input string
                if (cwTargetLength == 0)
                {
                    return true;
                }

                if (cwTargetLength > cwSourceLength)
                {
                    return false;
                }

                *matchedLength = 0;

                for (int i = 0; i < cwTargetLength; i++)
                {
                    if (options == CompareOptions.IgnoreCase || options == CompareOptions.OrdinalIgnoreCase)
                    {
                        Char a = source[i];
                        Char b = target[i];
                        if (Char.ToUpper(a) != char.ToUpper(b))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (source[i] != target[i])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            [ArduinoImplementation]
            public static unsafe void ChangeCaseInvariant(System.Char* src, System.Int32 srcLen, System.Char* dstBuffer, System.Int32 dstBufferCapacity, System.Boolean bToUpper)
            {
                if (srcLen > dstBufferCapacity)
                {
                    throw new OutOfMemoryException();
                }

                for (int i = 0; i < srcLen; i++)
                {
                    Char c = src[i];
                    dstBuffer[i] = bToUpper ? Char.ToUpper(c) : Char.ToLower(c);
                }

                dstBuffer[dstBufferCapacity - 1] = '\0';
            }

            [ArduinoImplementation]
            public static unsafe void ChangeCase(Char* src, Int32 srcLen, Char* dstBuffer, Int32 dstBufferCapacity, System.Boolean bToUpper)
            {
                // We're always using invariant culture for comparisons
                ChangeCaseInvariant(src, srcLen, dstBuffer, dstBufferCapacity, bToUpper);
            }

            [ArduinoImplementation]
            public static unsafe Int32 IanaIdToWindowsId(String ianaId, Char* windowsId, Int32 windowsIdLength)
            {
                int charsToCopy = ianaId.Length;
                if (windowsIdLength - 1 < charsToCopy)
                {
                    charsToCopy = windowsIdLength - 1;
                }

                for (int i = 0; i < charsToCopy; i++)
                {
                    *windowsId = ianaId[i];
                }

                windowsId[charsToCopy] = '\0';
                return charsToCopy;
            }
        }
    }
}

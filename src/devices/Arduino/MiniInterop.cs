using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
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

        [ArduinoReplacement("Interop+Globalization", "System.Private.CoreLib.dll", false, true, IncludingPrivates = true)]
        internal static class Globalization
        {
            [ArduinoImplementation(ArduinoImplementation.None)]
            public static int LoadICU()
            {
                return 1; // returning a non-zero value means false, which results in UseNLS to become false, which is probably what we want
            }

            [ArduinoImplementation(ArduinoImplementation.None)]
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

            [ArduinoImplementation(ArduinoImplementation.None)]
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

            [ArduinoImplementation(ArduinoImplementation.None)]
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

            [ArduinoImplementation(ArduinoImplementation.Interop_GlobalizationGetCalendarInfo, CompareByParameterNames = true)]
            internal static unsafe int GetCalendarInfo(string localeName, int calendarId, int calendarDataType, char* result, int resultCapacity)
            {
                return 0;
            }

        }

        [ArduinoReplacement("Interop+Kernel32", "System.Private.CoreLib.dll", false, true, IncludingPrivates = true)]
        internal class Kernel32
        {
        }
    }
}

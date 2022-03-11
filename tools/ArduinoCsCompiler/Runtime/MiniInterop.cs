// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.Globalization;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    // We're not replacing the helper methods of this class itself
    [ArduinoReplacement("Interop", "System.Private.CoreLib.dll", false, IncludingPrivates = true)]
    internal partial class MiniInterop
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

        [ArduinoImplementation("InteropGetRandomBytes")]
        public static unsafe void GetRandomBytes(byte* buffer, int length)
        {
            throw new NotImplementedException();
        }

        public static IntPtr CheckIo(IntPtr input)
        {
            if (input == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return input;
        }
    }
}

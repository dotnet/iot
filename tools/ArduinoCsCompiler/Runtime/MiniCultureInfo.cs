// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    internal enum CalendarId : ushort
    {
        UNINITIALIZED_VALUE = 0,
        GREGORIAN = 1,     // Gregorian (localized) calendar
        GREGORIAN_US = 2,     // Gregorian (U.S.) calendar
        JAPAN = 3,     // Japanese Emperor Era calendar
        /* SSS_WARNINGS_OFF */
        TAIWAN = 4,     // Taiwan Era calendar /* SSS_WARNINGS_ON */
        KOREA = 5,     // Korean Tangun Era calendar
        HIJRI = 6,     // Hijri (Arabic Lunar) calendar
        THAI = 7,     // Thai calendar
        HEBREW = 8,     // Hebrew (Lunar) calendar
        GREGORIAN_ME_FRENCH = 9,     // Gregorian Middle East French calendar
        GREGORIAN_ARABIC = 10,     // Gregorian Arabic calendar
        GREGORIAN_XLIT_ENGLISH = 11,     // Gregorian Transliterated English calendar
        GREGORIAN_XLIT_FRENCH = 12,
        // Note that all calendars after this point are MANAGED ONLY for now.
        JULIAN = 13,
        JAPANESELUNISOLAR = 14,
        CHINESELUNISOLAR = 15,
        SAKA = 16,     // reserved to match Office but not implemented in our code
        LUNAR_ETO_CHN = 17,     // reserved to match Office but not implemented in our code
        LUNAR_ETO_KOR = 18,     // reserved to match Office but not implemented in our code
        LUNAR_ETO_ROKUYOU = 19,     // reserved to match Office but not implemented in our code
        KOREANLUNISOLAR = 20,
        TAIWANLUNISOLAR = 21,
        PERSIAN = 22,
        UMALQURA = 23,
        LAST_CALENDAR = 23 // Last calendar ID
    }

    ////////////////////////////////////////////////////////////////////////////
    //
    //
    //
    //  Purpose:  This class represents the software preferences of a particular
    //            culture or community.  It includes information such as the
    //            language, writing system, and a calendar used by the culture
    //            as well as methods for common operations such as printing
    //            dates and sorting strings.
    //
    //
    //
    //  !!!! NOTE WHEN CHANGING THIS CLASS !!!!
    //
    //  If adding or removing members to this class, please update CultureInfoBaseObject
    //  in ndp/clr/src/vm/object.h. Note, the "actual" layout of the class may be
    //  different than the order in which members are declared. For instance, all
    //  reference types will come first in the class before value types (like ints, bools, etc)
    //  regardless of the order in which they are declared. The best way to see the
    //  actual order of the class is to do a !dumpobj on an instance of the managed
    //  object inside of the debugger.
    //
    ////////////////////////////////////////////////////////////////////////////
    [ArduinoReplacement(typeof(CultureInfo), true, IncludingPrivates = true)]
    internal class MiniCultureInfo : IFormatProvider, ICloneable
    {
        // We use an RFC4646 type string to construct CultureInfo.
        // This string is stored in _name and is authoritative.
        // We use the _cultureData to get the data for our object
        // The Invariant culture;
        private static readonly MiniCultureInfo _invariantCultureInfo = new MiniCultureInfo(MiniCultureData.Invariant, isReadOnly: true);
        private static readonly MiniCultureInfo _localizedCultureInfo = new MiniCultureInfo(new MiniCultureData(false), isReadOnly: true);

        // Get the current user default culture. This one is almost always used, so we create it by default.
        private static volatile MiniCultureInfo? s_userDefaultCulture;

        // The culture used in the user interface. This is mostly used to load correct localized resources.
        private static volatile MiniCultureInfo? s_userDefaultUICulture;

        private static volatile TextInfo s_textInfo = MiniUnsafe.As<TextInfo>(new MiniTextInfo());

        private bool _isReadOnly;
        internal NumberFormatInfo? _numInfo;
        internal DateTimeFormatInfo? _dateTimeInfo;

        internal bool _isInherited;

        // Names are confusing.  Here are 3 names we have:
        //
        //  new CultureInfo()   _name          _nonSortName    _sortName
        //      en-US           en-US           en-US           en-US
        //      de-de_phoneb    de-DE_phoneb    de-DE           de-DE_phoneb
        //      fj-fj (custom)  fj-FJ           fj-FJ           en-US (if specified sort is en-US)
        //      en              en
        //
        // Note that in Silverlight we ask the OS for the text and sort behavior, so the
        // textinfo and compareinfo names are the same as the name

        // This has a de-DE, de-DE_phoneb or fj-FJ style name
        internal string _name;

        // This will hold the sorting name to be returned from CultureInfo.SortName property.
        // This might be completely unrelated to the culture name if a custom culture.  Ie en-US for fj-FJ.
        // Otherwise its the sort name, ie: de-DE or de-DE_phoneb
        private string? _sortName;

        // WARNING: We allow diagnostic tools to directly inspect these three members (s_InvariantCultureInfo, s_DefaultThreadCurrentUICulture and s_DefaultThreadCurrentCulture)
        // See https://github.com/dotnet/corert/blob/master/Documentation/design-docs/diagnostics/diagnostics-tools-contract.md for more details.
        // Please do not change the type, the name, or the semantic usage of this member without understanding the implication for tools.
        // Get in touch with the diagnostics team if you have questions.
        private MiniCultureData _cultureData;

        // LOCALE constants of interest to us internally and privately for LCID functions
        // (ie: avoid using these and use names if possible)
        internal const int LOCALE_NEUTRAL = 0x0000;
        private const int LOCALE_USER_DEFAULT = 0x0400;
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LOCALE_CUSTOM_UNSPECIFIED = 0x1000;
        internal const int LOCALE_CUSTOM_DEFAULT = 0x0c00;
        internal const int LOCALE_INVARIANT = 0x007F;

        private static MiniCultureInfo InitializeUserDefaultCulture()
        {
            s_userDefaultCulture = _localizedCultureInfo;
            return s_userDefaultCulture!;
        }

        private static MiniCultureInfo InitializeUserDefaultUICulture()
        {
            s_userDefaultUICulture = _localizedCultureInfo;
            return s_userDefaultUICulture!;
        }

        public MiniCultureInfo(string name)
            : this(name, true)
        {
        }

        public MiniCultureInfo(string name, bool useUserOverride)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Get our data providing record
            MiniCultureData cultureData = new MiniCultureData(string.IsNullOrWhiteSpace(name));

            _cultureData = cultureData;
            _name = _cultureData.CultureName;
            _isInherited = false;
        }

        private MiniCultureInfo(MiniCultureData cultureData, bool isReadOnly = false)
        {
            _cultureData = cultureData;
            _name = cultureData.CultureName;
            _isReadOnly = isReadOnly;
        }

        public MiniCultureInfo(int culture)
            : this(culture, true)
        {
        }

        public MiniCultureInfo(int culture, bool useUserOverride)
        {
            // We don't check for other invalid LCIDS here...
            if (culture < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(culture));
            }

            _cultureData = new MiniCultureData(culture == 0x1000);
            _isInherited = GetType() != typeof(CultureInfo);
            _name = _cultureData.CultureName;
        }

        /// <summary>
        /// Constructor called by SQL Server's special munged culture - creates a culture with
        /// a TextInfo and CompareInfo that come from a supplied alternate source. This object
        /// is ALWAYS read-only.
        /// Note that we really cannot use an LCID version of this override as the cached
        /// name we create for it has to include both names, and the logic for this is in
        /// the GetCultureInfo override *only*.
        /// </summary>
        internal MiniCultureInfo(string cultureName, string textAndCompareCultureName)
        {
            if (cultureName == null)
            {
                throw new ArgumentNullException(nameof(cultureName));
            }

            _cultureData = new MiniCultureData(string.IsNullOrWhiteSpace(cultureName));

            _name = _cultureData.CultureName;
        }

        /// <summary>
        /// Return a specific culture. A tad irrelevent now since we always
        /// return valid data for neutral locales.
        ///
        /// Note that there's interesting behavior that tries to find a
        /// smaller name, ala RFC4647, if we can't find a bigger name.
        /// That doesn't help with things like "zh" though, so the approach
        /// is of questionable value
        /// </summary>
        public static MiniCultureInfo CreateSpecificCulture(string name)
        {
            MiniCultureInfo? culture;

            try
            {
                culture = new MiniCultureInfo(name);
            }
            catch (ArgumentException)
            {
                // When CultureInfo throws this exception, it may be because someone passed the form
                // like "az-az" because it came out of an http accept lang. We should try a little
                // parsing to perhaps fall back to "az" here and use *it* to create the neutral.
                culture = null;
                for (int idx = 0; idx < name.Length; idx++)
                {
                    if ('-' == name[idx])
                    {
                        try
                        {
                            culture = new MiniCultureInfo(name.Substring(0, idx));
                            break;
                        }
                        catch (ArgumentException)
                        {
                            // throw the original exception so the name in the string will be right
                            throw;
                        }
                    }
                }

                if (culture == null)
                {
                    // nothing to save here; throw the original exception
                    throw;
                }
            }

            // In the most common case, they've given us a specific culture, so we'll just return that.
            if (!culture.IsNeutralCulture)
            {
                return culture;
            }

            return new MiniCultureInfo(culture._cultureData.CultureName);
        }

        internal static bool VerifyCultureName(string cultureName, bool throwException)
        {
            // This function is used by ResourceManager.GetResourceFileName().
            // ResourceManager searches for resource using CultureInfo.Name,
            // so we should check against CultureInfo.Name.
            for (int i = 0; i < cultureName.Length; i++)
            {
                char c = cultureName[i];
                // TODO: Names can only be RFC4646 names (ie: a-zA-Z0-9) while this allows any unicode letter/digit
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                {
                    continue;
                }

                if (throwException)
                {
                    throw new ArgumentException(cultureName, nameof(cultureName));
                }

                return false;
            }

            return true;
        }

        internal static bool VerifyCultureName(CultureInfo culture, bool throwException)
        {
            return true;
        }

        /// <summary>
        /// This instance provides methods based on the current user settings.
        /// These settings are volatile and may change over the lifetime of the
        /// thread.
        /// </summary>
        /// <remarks>
        /// We use the following order to return CurrentCulture and CurrentUICulture
        ///      o   Use WinRT to return the current user profile language
        ///      o   use current thread culture if the user already set one using CurrentCulture/CurrentUICulture
        ///      o   use thread culture if the user already set one using DefaultThreadCurrentCulture
        ///          or DefaultThreadCurrentUICulture
        ///      o   Use NLS default user culture
        ///      o   Use NLS default system culture
        ///      o   Use Invariant culture
        /// </remarks>
        public static MiniCultureInfo CurrentCulture
        {
            get
            {
                return s_userDefaultCulture ?? InitializeUserDefaultCulture();
            }

            // Required attribute, because the original property has a different type
            [ArduinoImplementation(CompareByParameterNames = true)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                s_userDefaultCulture = value;
            }
        }

        public static MiniCultureInfo CurrentUICulture
        {
            get
            {
                return s_userDefaultUICulture ?? InitializeUserDefaultUICulture();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                s_userDefaultUICulture = value;
            }
        }

        internal static string UserDefaultLocaleName => UserDefaultLocalNameGetter();

        [ArduinoCompileTimeConstant]
        internal static string UserDefaultLocalNameGetter()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        internal static MiniCultureInfo UserDefaultUICulture => s_userDefaultUICulture ?? InitializeUserDefaultUICulture();

        public static MiniCultureInfo InstalledUICulture => s_userDefaultCulture ?? InitializeUserDefaultCulture();

        public static MiniCultureInfo? DefaultThreadCurrentCulture
        {
            get => InvariantCulture;
            set
            // If you add pre-conditions to this method, check to see if you also need to
            // add them to Thread.CurrentCulture.set.
            {
            }
        }

        public static MiniCultureInfo? DefaultThreadCurrentUICulture
        {
            get => InvariantCulture;
            set
            {
            }
        }

        /// <summary>
        /// This instance provides methods, for example for casing and sorting,
        /// that are independent of the system and current user settings.  It
        /// should be used only by processes such as some system services that
        /// require such invariant results (eg. file systems).  In general,
        /// the results are not linguistically correct and do not match any
        /// culture info.
        /// </summary>
        public static MiniCultureInfo InvariantCulture
        {
            get
            {
                return _invariantCultureInfo;
            }
        }

        /// <summary>
        /// Return the parent CultureInfo for the current instance.
        /// </summary>
        public virtual MiniCultureInfo Parent
        {
            get
            {
                return null!;
            }
        }

        public virtual int LCID => 0;

        public virtual int KeyboardLayoutId => 0;

        public static MiniCultureInfo[] GetCultures(CultureTypes types)
        {
            return new MiniCultureInfo[] { InvariantCulture };
        }

        /// <summary>
        /// Returns the full name of the CultureInfo. The name is in format like
        /// "en-US" This version does NOT include sort information in the name.
        /// </summary>
        public virtual string Name => (_cultureData.Name ?? string.Empty);

        /// <summary>
        /// This one has the sort information (ie: de-DE_phoneb)
        /// </summary>
        internal string SortName => _sortName ??= _cultureData.SortName;

        public string IetfLanguageTag =>
            // special case the compatibility cultures
            Name switch
            {
                "zh-CHT" => "zh-Hant",
                "zh-CHS" => "zh-Hans",
                _ => Name,
            };

        /// <summary>
        /// Returns the full name of the CultureInfo in the localized language.
        /// For example, if the localized language of the runtime is Spanish and the CultureInfo is
        /// US English, "Ingles (Estados Unidos)" will be returned.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return _cultureData.DisplayName;
            }
        }

        /// <summary>
        /// Returns the full name of the CultureInfo in the native language.
        /// For example, if the CultureInfo is US English, "English
        /// (United States)" will be returned.
        /// </summary>
        public virtual string NativeName => _cultureData.NativeName;

        /// <summary>
        /// Returns the full name of the CultureInfo in English.
        /// For example, if the CultureInfo is US English, "English
        /// (United States)" will be returned.
        /// </summary>
        public virtual string EnglishName => _cultureData.EnglishName;

        /// <summary>
        /// ie: en
        /// </summary>
        public virtual string TwoLetterISOLanguageName => _cultureData.TwoLetterISOLanguageName;

        /// <summary>
        /// ie: eng
        /// </summary>
        public virtual string ThreeLetterISOLanguageName => _cultureData.ThreeLetterISOLanguageName;

        /// <summary>
        /// Returns the 3 letter windows language name for the current instance.  eg: "ENU"
        /// The ISO names are much preferred
        /// </summary>
        public virtual string ThreeLetterWindowsLanguageName => _cultureData.ThreeLetterWindowsLanguageName;

        /// <summary>
        /// Gets the CompareInfo for this culture.
        /// </summary>
        public virtual CompareInfo CompareInfo => MiniUnsafe.As<CompareInfo>(new MiniCompareInfo(this));

        /// <summary>
        /// Gets the TextInfo for this culture.
        /// </summary>
        public virtual TextInfo TextInfo
        {
            get
            {
                return s_textInfo;
            }
        }

        public override bool Equals(object? value)
        {
            if (object.ReferenceEquals(this, value))
            {
                return true;
            }

            if (value == null)
            {
                return false;
            }

            if (value is CultureInfo that)
            {
                // using CompareInfo to verify the data passed through the constructor
                // CultureInfo(String cultureName, String textAndCompareCultureName)
                bool ret = Name.Equals(that.Name);
                return ret;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + CompareInfo.GetHashCode();
        }

        /// <summary>
        /// Implements object.ToString(). Returns the name of the CultureInfo,
        /// eg. "de-DE_phoneb", "en-US", or "fj-FJ".
        /// </summary>
        public override string ToString() => _name;

        public virtual object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(NumberFormatInfo))
            {
                return NumberFormat;
            }

            if (formatType == typeof(DateTimeFormatInfo))
            {
                return DateTimeFormat;
            }

            return null;
        }

        public virtual bool IsNeutralCulture => true;

        public CultureTypes CultureTypes
        {
            get
            {
                return CultureTypes.NeutralCultures;
            }
        }

        public virtual NumberFormatInfo NumberFormat
        {
            get
            {
                if (_numInfo == null)
                {
                    _numInfo = new NumberFormatInfo();
                    _cultureData.GetNFIValues(_numInfo);
                }

                return _numInfo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                VerifyWritable();
                _numInfo = value;
            }
        }

        /// <summary>
        /// Create a DateTimeFormatInfo, and fill in the properties according to
        /// the CultureID.
        /// </summary>
        public virtual DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                if (_dateTimeInfo == null)
                {
                    _dateTimeInfo = new DateTimeFormatInfo();
                    _cultureData.GetDateTimeFormat(_dateTimeInfo);
                }

                return _dateTimeInfo;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                VerifyWritable();
                _dateTimeInfo = value;
            }
        }

        public void ClearCachedData()
        {
            // reset the default culture values
            s_userDefaultCulture = new MiniCultureInfo(0);
            s_userDefaultUICulture = new MiniCultureInfo(0);
        }

        /// <summary>
        /// Return/set the default calendar used by this culture.
        /// This value can be overridden by regional option if this is a current culture.
        /// </summary>
        public virtual Calendar Calendar
        {
            get
            {
                return null!;
            }
        }

        public bool UseUserOverride => false;

        public static MiniCultureInfo ReadOnly(MiniCultureInfo ci)
        {
            ci._isReadOnly = true;
            return ci;
        }

        public MiniCultureInfo GetConsoleFallbackUICulture()
        {
            return InvariantCulture;
        }

        public virtual object Clone()
        {
            return new MiniCultureInfo(0);
        }

        public bool IsReadOnly => _isReadOnly;

        private void VerifyWritable()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// For resource lookup, we consider a culture the invariant culture by name equality.
        /// We perform this check frequently during resource lookup, so adding a property for
        /// improved readability.
        /// </summary>
        internal bool HasInvariantCultureName => Name == InvariantCulture.Name;

        /// <summary>
        /// Gets a cached copy of the specified culture from an internal
        /// hashtable (or creates it if not found). (LCID version)
        /// </summary>
        public static MiniCultureInfo GetCultureInfo(int culture)
        {
            if (culture <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(culture));
            }

            return culture == 0x10000 ? InvariantCulture : CurrentCulture;
        }

        /// <summary>
        /// Gets a cached copy of the specified culture from an internal
        /// hashtable (or creates it if not found). (Named version)
        /// </summary>
        public static MiniCultureInfo GetCultureInfo(string name)
        {
            return CurrentCulture;
        }

        /// <summary>
        /// Gets a cached copy of the specified culture from an internal
        /// hashtable (or creates it if not found).
        /// </summary>
        public static MiniCultureInfo GetCultureInfo(string name, string altName)
        {
            return CurrentCulture;
        }

        public static MiniCultureInfo GetCultureInfo(string name, bool predefinedOnly)
        {
            return CurrentCulture;
        }

        public static MiniCultureInfo GetCultureInfoByIetfLanguageTag(string name)
        {
            // Disallow old zh-CHT/zh-CHS names
            return CurrentCulture;
        }

        [ArduinoReplacement("System.Globalization.CultureData", null, true, IncludingPrivates = true)]
        private class MiniCultureData
        {
            private static readonly string[] _saLongTimes;
            private static readonly string[] _saShortTimes = new string[] { "HH:mm", "hh:mm tt", "H:mm", "h:mm tt" }; // short time format
            private static readonly string[] _saDurationFormats = new string[] { "HH:mm:ss" };
            private static readonly string[] _saLongDates;

            private static readonly string[] _saShortDates = new string[]
            {
                "yyyy-MM-dd"
            };
            private static readonly string[] _saYearMonths = new string[] { "yyyy MMMM" };
            private static readonly CalendarId[] _saCalendars = new CalendarId[]
            {
                CalendarId.GREGORIAN
            };

            private static readonly string[] _saEraNames = new string[] { "AD", "BC" };
            private static readonly string[] _saAbbreviatedDayNames;
            private static readonly string[] _saDayNames;
            private static readonly string[] _saAbbreviatedMonthNames;
            private static readonly string[] _saMonthNames;

            private static readonly string _sPositiveSign = "+";
            private static readonly string _sNegativeSign = "-";
            private static readonly string _sThousandSeparatorInvariant = "'";
            private static readonly string _sDecimalSeparatorInvariant = ".";
            private static readonly string _sCurrencyInvariant = "$";

            private static readonly string _sCurrencyLocal;
            private static readonly string _sThousandSeparatorLocal;
            private static readonly string _sDecimalSeparatorLocal;

            private readonly bool _isInvariant;

            static MiniCultureData()
            {
                _sCurrencyLocal = GetCurrencySymbol();
                _sThousandSeparatorLocal = GetThousandSeparator();
                _sDecimalSeparatorLocal = GetDecimalSeparator();

                _saMonthNames = GetMonthNames();
                _saAbbreviatedMonthNames = GetAbbreviatedMonthNames();
                _saDayNames = GetDayNames();
                _saAbbreviatedDayNames = GetAbbreviatedDayNames();
                _saLongDates = new string[1] { GetLongDatePattern() };
                _saLongTimes = new string[1] { GetLongTimePattern() };
            }

            /// <summary>
            /// Required constructor for infrastructure
            /// Used to create the non-static property values marked with [ArduinoCompileTimeConstant]
            /// </summary>
            private MiniCultureData()
                : this(false)
            {
            }

            public MiniCultureData(bool isInvariant)
            {
                _isInvariant = isInvariant;
            }

            private static MiniCultureData? _sInvariant; // Initialized on usage, to prevent a static ctor dependency

            public static MiniCultureData Invariant
            {
                get
                {
                    if (_sInvariant == null)
                    {
                        return _sInvariant = new MiniCultureData(true);
                    }

                    return _sInvariant;
                }
            }

            public string CultureName
            {
                get
                {
                    if (_isInvariant)
                    {
                        return "Invariant";
                    }

                    return "Local";
                }
            }

            public string DisplayName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.DisplayName;
                }
            }

            public string Name
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.Name;
                }
            }

            public string SortName
            {
                get
                {
                    return Name;
                }
            }

            public string TextInfoName
            {
                get
                {
                    return SortName;
                }
            }

            public string NativeName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.NativeName;
                }
            }

            public string EnglishName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.EnglishName;
                }
            }

            public string ThreeLetterWindowsLanguageName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
                }
            }

            public string TwoLetterISOLanguageName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }
            }

            public string ThreeLetterISOLanguageName
            {
                [ArduinoCompileTimeConstant]
                get
                {
                    return CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
                }
            }

            public int FirstDayOfWeek
            {
                get
                {
                    return 0;
                }
            }

            public int CalendarWeekRule
            {
                get
                {
                    return 0;
                }
            }

            public string AMDesignator
            {
                get
                {
                    return "AM";
                }
            }

            public string PMDesignator
            {
                get
                {
                    return "PM";
                }
            }

            public string TimeSeparator
            {
                get
                {
                    return ":";
                }
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public string DateSeparator(int calendarId)
            {
                return "/";
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public string[] LongDates(CalendarId calendarId)
            {
                return _saLongDates;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public string[] ShortDates(CalendarId calendarId)
            {
                return _saShortDates;
            }

            public string[] LongTimes
            {
                get
                {
                    return _saLongTimes;
                }
            }

            public string[] ShortTimes
            {
                get
                {
                    return _saShortTimes;
                }
            }

            [SuppressMessage("Microsoft.Naming", "SA1300", Justification = "Runtime method name")]
            internal static string[]? nativeEnumTimeFormats(string localeName, uint dwFlags, bool useUserOverride)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            public string[] YearMonths(CalendarId calendarId)
            {
                return _saYearMonths;
            }

            /// <summary>
            /// All available calendar type(s). The first one is the default calendar.
            /// </summary>
            internal CalendarId[] CalendarIds
            {
                get
                {
                    return _saCalendars;
                }
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal string MonthDay(CalendarId calendarId)
            {
                return "MMMM dd";
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal MiniCultureData GetCalendar(CalendarId calendarId)
            {
                return this;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] EraNames(CalendarId calendarId)
            {
                return _saEraNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] AbbrevEraNames(CalendarId calendarId)
            {
                return _saEraNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] AbbreviatedEnglishEraNames(CalendarId calendarId)
            {
                return _saEraNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] AbbreviatedDayNames(CalendarId calendarId)
            {
                return _saAbbreviatedDayNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] DayNames(CalendarId calendarId)
            {
                return _saDayNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] AbbreviatedGenitiveMonthNames(CalendarId calendarId)
            {
                return _saAbbreviatedMonthNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] AbbreviatedMonthNames(CalendarId calendarId)
            {
                return _saAbbreviatedMonthNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] MonthNames(CalendarId calendarId)
            {
                return _saMonthNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal String[] GenitiveMonthNames(CalendarId calendarId)
            {
                return _saMonthNames;
            }

            [ArduinoImplementation(CompareByParameterNames = true)]
            internal string[] LeapYearMonthNames(CalendarId calendarId)
            {
                // Only used in hebrew
                return _saMonthNames;
            }

            internal bool UseUserOverride
            {
                get
                {
                    return false;
                }
            }

            internal static MiniCultureData? GetCultureData(string? cultureName, bool useUserOverride)
            {
                return new MiniCultureData(false);
            }

            internal void GetNFIValues(NumberFormatInfo nfi)
            {
                nfi.PositiveSign = _sPositiveSign;
                nfi.NegativeSign = _sNegativeSign;

                nfi.NumberGroupSeparator = _sThousandSeparatorInvariant;
                nfi.NumberDecimalSeparator = _sDecimalSeparatorInvariant;
                nfi.NumberDecimalDigits = 2;
                nfi.NumberNegativePattern = 1;

                nfi.CurrencySymbol = _sCurrencyInvariant!;
                nfi.CurrencyGroupSeparator = _sThousandSeparatorInvariant;
                nfi.CurrencyDecimalSeparator = _sThousandSeparatorInvariant;
                nfi.CurrencyDecimalDigits = 2;
                nfi.CurrencyNegativePattern = 12;
                nfi.CurrencyPositivePattern = 2;
                if (!_isInvariant)
                {
                    nfi.CurrencySymbol = _sCurrencyLocal;
                    nfi.NumberGroupSeparator = _sThousandSeparatorLocal;
                    nfi.NumberDecimalSeparator = _sDecimalSeparatorLocal;
                }
            }

            [ArduinoCompileTimeConstant]
            private static string GetCurrencySymbol()
            {
                return CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            }

            [ArduinoCompileTimeConstant]
            private static string GetThousandSeparator()
            {
                return CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            }

            [ArduinoCompileTimeConstant]
            private static string GetDecimalSeparator()
            {
                return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            }

            [ArduinoCompileTimeConstant]
            private static string[] GetMonthNames()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            }

            [ArduinoCompileTimeConstant]
            private static string[] GetDayNames()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
            }

            [ArduinoCompileTimeConstant]
            private static string[] GetAbbreviatedMonthNames()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
            }

            [ArduinoCompileTimeConstant]
            private static string[] GetAbbreviatedDayNames()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            }

            [ArduinoCompileTimeConstant]
            private static string GetFullDateTimePattern()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern;
            }

            [ArduinoCompileTimeConstant]
            private static string GetLongDatePattern()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            }

            [ArduinoCompileTimeConstant]
            private static string GetLongTimePattern()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            }

            [ArduinoCompileTimeConstant]
            private static string GetShortTimePattern()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            }

            [ArduinoCompileTimeConstant]
            private static string GetShortDatePattern()
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            }

            public void GetDateTimeFormat(DateTimeFormatInfo dateTimeInfo)
            {
                dateTimeInfo.MonthNames = _saMonthNames;
                dateTimeInfo.DayNames = _saDayNames;
                dateTimeInfo.AbbreviatedMonthNames = _saAbbreviatedMonthNames;
                dateTimeInfo.AbbreviatedDayNames = _saAbbreviatedDayNames;
                dateTimeInfo.FullDateTimePattern = GetFullDateTimePattern();
                dateTimeInfo.LongTimePattern = GetLongTimePattern();
                dateTimeInfo.ShortTimePattern = GetShortTimePattern();
                dateTimeInfo.LongDatePattern = GetLongDatePattern();
                dateTimeInfo.ShortDatePattern = GetShortDatePattern();
            }
        }

        public class MiniCompareInfo
        {
            public MiniCompareInfo(MiniCultureInfo culture)
            {
            }
        }
    }
}

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Globalization.CompareInfo), true)]
    internal sealed class MiniCompareInfo
    {
        // Mask used to check if IndexOf()/LastIndexOf()/IsPrefix()/IsPostfix() has the right flags.
        private const CompareOptions ValidIndexMaskOffFlags =
            ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace |
              CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType);

        // Mask used to check if Compare() / GetHashCode(string) / GetSortKey has the right flags.
        private const CompareOptions ValidCompareMaskOffFlags =
            ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace |
              CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.StringSort);

        // Cache the invariant CompareInfo
        internal static readonly CompareInfo Invariant = CultureInfo.InvariantCulture.CompareInfo;

        // CompareInfos have an interesting identity.  They are attached to the locale that created them,
        // ie: en-US would have an en-US sort.  For haw-US (custom), then we serialize it as haw-US.
        // The interesting part is that since haw-US doesn't have its own sort, it has to point at another
        // locale, which is what SCOMPAREINFO does.
        [OptionalField(VersionAdded = 2)]
        private string _name; // The name used to construct this CompareInfo. Do not rename (binary serialization)

        internal MiniCompareInfo(CultureInfo culture)
        {
            _name = culture.Name;
            InitSort(culture);
        }

        /// <summary>
        /// Get the CompareInfo constructed from the data table in the specified
        /// assembly for the specified culture.
        /// Warning: The assembly versioning mechanism is dead!
        /// </summary>
        public static CompareInfo GetCompareInfo(int culture, Assembly assembly)
        {
            // Parameter checking.
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return GetCompareInfo(culture);
        }

        /// <summary>
        /// Get the CompareInfo constructed from the data table in the specified
        /// assembly for the specified culture.
        /// The purpose of this method is to provide version for CompareInfo tables.
        /// </summary>
        public static CompareInfo GetCompareInfo(string name, Assembly assembly)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return GetCompareInfo(name);
        }

        /// <summary>
        /// Get the CompareInfo for the specified culture.
        /// This method is provided for ease of integration with NLS-based software.
        /// </summary>
        public static CompareInfo GetCompareInfo(int culture)
        {
            return CultureInfo.GetCultureInfo(culture).CompareInfo;
        }

        /// <summary>
        /// Get the CompareInfo for the specified culture.
        /// </summary>
        public static CompareInfo GetCompareInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return CultureInfo.GetCultureInfo(name).CompareInfo;
        }

        public static bool IsSortable(char ch)
        {
            return IsSortable(MemoryMarshal.CreateReadOnlySpan(ref ch, 1));
        }

        public static bool IsSortable(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return IsSortable(text.AsSpan());
        }

        /// <summary>
        /// Indicates whether a specified Unicode string is sortable.
        /// </summary>
        /// <param name="text">A string of zero or more Unicode characters.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="text"/> is non-empty and contains
        /// only sortable Unicode characters; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSortable(ReadOnlySpan<char> text)
        {
            if (text.Length == 0)
            {
                return false;
            }

            return true; // Invariant culture is always sortable
        }

#if NET5_0
        /// <summary>
        /// Indicates whether a specified <see cref="Rune"/> is sortable.
        /// </summary>
        /// <param name="value">A Unicode scalar value.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value"/> is a sortable Unicode scalar
        /// value; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSortable(Rune value)
        {
            Span<char> valueAsUtf16 = stackalloc char[4];
            int charCount = value.EncodeToUtf16(valueAsUtf16);
            return IsSortable(valueAsUtf16.Slice(0, charCount));
        }
#endif

        private void InitSort(CultureInfo culture)
        {
        }

        /// <summary>
        ///  Returns the name of the culture (well actually, of the sort).
        ///  Very important for providing a non-LCID way of identifying
        ///  what the sort is.
        ///
        ///  Note that this name isn't dereferenced in case the CompareInfo is a different locale
        ///  which is consistent with the behaviors of earlier versions.  (so if you ask for a sort
        ///  and the locale's changed behavior, then you'll get changed behavior, which is like
        ///  what happens for a version update)
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Compares the two strings with the given options.  Returns 0 if the
        /// two strings are equal, a number less than 0 if string1 is less
        /// than string2, and a number greater than 0 if string1 is greater
        /// than string2.
        /// </summary>
        public int Compare(string? string1, string? string2)
        {
            return Compare(string1, string2, CompareOptions.None);
        }

        public int Compare(string? string1, string? string2, CompareOptions options)
        {
            int retVal;

            // Our paradigm is that null sorts less than any other string and
            // that two nulls sort as equal.
            if (string1 == null)
            {
                retVal = (string2 == null) ? 0 : -1;
                goto CheckOptionsAndReturn;
            }

            if (string2 == null)
            {
                retVal = 1;
                goto CheckOptionsAndReturn;
            }

            return Compare(string1.AsSpan(), string2.AsSpan(), options);

        CheckOptionsAndReturn:

            // If we're short-circuiting the globalization logic, we still need to check that
            // the provided options were valid.
            CheckCompareOptionsForCompare(options);
            return retVal;
        }

        internal int CompareOptionIgnoreCase(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2)
        {
            return Compare(string1, string2, CompareOptions.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares the specified regions of the two strings with the given
        /// options.
        /// Returns 0 if the two strings are equal, a number less than 0 if
        /// string1 is less than string2, and a number greater than 0 if
        /// string1 is greater than string2.
        /// </summary>
        public int Compare(string? string1, int offset1, int length1, string? string2, int offset2, int length2)
        {
            return Compare(string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
        }

        public int Compare(string? string1, int offset1, string? string2, int offset2, CompareOptions options)
        {
            return Compare(string1, offset1, string1 == null ? 0 : string1.Length - offset1,
                           string2, offset2, string2 == null ? 0 : string2.Length - offset2, options);
        }

        public int Compare(string? string1, int offset1, string? string2, int offset2)
        {
            return Compare(string1, offset1, string2, offset2, CompareOptions.None);
        }

        public int Compare(string? string1, int offset1, int length1, string? string2, int offset2, int length2, CompareOptions options)
        {
            ReadOnlySpan<char> span1 = default;
            ReadOnlySpan<char> span2 = default;

            if (string1 == null)
            {
                if (offset1 != 0 || length1 != 0)
                {
                    goto BoundsCheckError;
                }
            }
            else if (!MiniString.TryGetSpan(string1, offset1, length1, out span1))
            {
                goto BoundsCheckError;
            }

            if (string2 == null)
            {
                if (offset2 != 0 || length2 != 0)
                {
                    goto BoundsCheckError;
                }
            }
            else if (!MiniString.TryGetSpan(string2, offset2, length2, out span2))
            {
                goto BoundsCheckError;
            }

            // At this point both string1 and string2 have been bounds-checked.
            int retVal;

            // Our paradigm is that null sorts less than any other string and
            // that two nulls sort as equal.
            if (string1 == null)
            {
                retVal = (string2 == null) ? 0 : -1;
                goto CheckOptionsAndReturn;
            }

            if (string2 == null)
            {
                retVal = 1;
                goto CheckOptionsAndReturn;
            }

            return Compare(span1, span2, options);

        CheckOptionsAndReturn:

            // If we're short-circuiting the globalization logic, we still need to check that
            // the provided options were valid.
            CheckCompareOptionsForCompare(options);
            return retVal;

        BoundsCheckError:

            // We know a bounds check error occurred. Now we just need to figure
            // out the correct error message to surface.
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="string1">The first string to compare.</param>
        /// <param name="string2">The second string to compare.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the comparison.</param>
        /// <returns>
        /// Zero if <paramref name="string1"/> and <paramref name="string2"/> are equal;
        /// or a negative value if <paramref name="string1"/> sorts before <paramref name="string2"/>;
        /// or a positive value if <paramref name="string1"/> sorts after <paramref name="string2"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public int Compare(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options = CompareOptions.None)
        {
            if (string1 == string2) // referential equality + length
            {
                CheckCompareOptionsForCompare(options);
                return 0;
            }

            // The actual comparison is a simple ordinal comparison
            return string1.SequenceCompareTo(string2);
        }

        // Checks that 'CompareOptions' is valid for a call to Compare, throwing the appropriate
        // exception if the check fails.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCompareOptionsForCompare(CompareOptions options)
        {
            // Any combination of defined CompareOptions flags is valid, except for
            // Ordinal and OrdinalIgnoreCase, which may only be used in isolation.
            if ((options & ValidCompareMaskOffFlags) != 0)
            {
                if (options != CompareOptions.Ordinal && options != CompareOptions.OrdinalIgnoreCase)
                {
                    ThrowCompareOptionsCheckFailed(options);
                }
            }
        }

        private static void ThrowCompareOptionsCheckFailed(CompareOptions options)
        {
            throw new ArgumentException(
                paramName: nameof(options),
                message: "Invalid Arguments");
        }

        /// <summary>
        /// Determines whether prefix is a prefix of string.  If prefix equals
        /// string.Empty, true is returned.
        /// </summary>
        public bool IsPrefix(string source, string prefix, CompareOptions options)
        {
            return IsPrefix(source.AsSpan(), prefix.AsSpan(), options);
        }

        /// <summary>
        /// Determines whether a string starts with a specific prefix.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="prefix">The prefix to attempt to match at the start of <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="prefix"/> occurs at the start of <paramref name="source"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public unsafe bool IsPrefix(ReadOnlySpan<char> source, ReadOnlySpan<char> prefix, CompareOptions options = CompareOptions.None)
        {
            // The empty string is trivially a prefix of every other string. For compat with
            // earlier versions of the Framework we'll early-exit here before validating the
            // 'options' argument.
            if (prefix.IsEmpty)
            {
                return true;
            }

            return source.StartsWith(prefix);
        }

        /// <summary>
        /// Determines whether a string starts with a specific prefix.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="prefix">The prefix to attempt to match at the start of <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the match.</param>
        /// <param name="matchLength">When this method returns, contains the number of characters of
        /// <paramref name="source"/> that matched the desired prefix. This may be different than the
        /// length of <paramref name="prefix"/> if a linguistic comparison is performed. Set to 0
        /// if the prefix did not match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="prefix"/> occurs at the start of <paramref name="source"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        /// <remarks>
        /// This method has greater overhead than other IsPrefix overloads which don't
        /// take a <paramref name="matchLength"/> argument. Call this overload only if you require
        /// the match length information.
        /// </remarks>
        public unsafe bool IsPrefix(ReadOnlySpan<char> source, ReadOnlySpan<char> prefix, CompareOptions options, out int matchLength)
        {
            bool matched;

            matched = IsPrefix(source, prefix, options);
            matchLength = (matched) ? prefix.Length : 0;
            return matched;
        }

        public bool IsPrefix(string source, string prefix)
        {
            return IsPrefix(source, prefix, CompareOptions.None);
        }

        /// <summary>
        /// Determines whether suffix is a suffix of string.  If suffix equals
        /// string.Empty, true is returned.
        /// </summary>
        public bool IsSuffix(string source, string suffix, CompareOptions options)
        {
            return IsSuffix(source.AsSpan(), suffix.AsSpan(), options);
        }

        /// <summary>
        /// Determines whether a string ends with a specific suffix.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="suffix">The suffix to attempt to match at the end of <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="suffix"/> occurs at the end of <paramref name="source"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public unsafe bool IsSuffix(ReadOnlySpan<char> source, ReadOnlySpan<char> suffix, CompareOptions options = CompareOptions.None)
        {
            // The empty string is trivially a suffix of every other string. For compat with
            // earlier versions of the Framework we'll early-exit here before validating the
            // 'options' argument.
            if (suffix.IsEmpty)
            {
                return true;
            }

            return source.EndsWith(suffix);
        }

        /// <summary>
        /// Determines whether a string ends with a specific suffix.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="suffix">The suffix to attempt to match at the end of <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the match.</param>
        /// <param name="matchLength">When this method returns, contains the number of characters of
        /// <paramref name="source"/> that matched the desired suffix. This may be different than the
        /// length of <paramref name="suffix"/> if a linguistic comparison is performed. Set to 0
        /// if the suffix did not match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="suffix"/> occurs at the end of <paramref name="source"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        /// <remarks>
        /// This method has greater overhead than other IsSuffix overloads which don't
        /// take a <paramref name="matchLength"/> argument. Call this overload only if you require
        /// the match length information.
        /// </remarks>
        public unsafe bool IsSuffix(ReadOnlySpan<char> source, ReadOnlySpan<char> suffix, CompareOptions options, out int matchLength)
        {
            bool matched;

            matched = IsSuffix(source, suffix, options);
            matchLength = (matched) ? suffix.Length : 0;

            return matched;
        }

        public bool IsSuffix(string source, string suffix)
        {
            return IsSuffix(source, suffix, CompareOptions.None);
        }

        /// <summary>
        /// Returns the first index where value is found in string.  The
        /// search starts from startIndex and ends at endIndex.  Returns -1 if
        /// the specified value is not found.  If value equals string.Empty,
        /// startIndex is returned.  Throws IndexOutOfRange if startIndex or
        /// endIndex is less than zero or greater than the length of string.
        /// Throws ArgumentException if value (as a string) is null.
        /// </summary>
        public int IndexOf(string source, char value)
        {
            return IndexOf(source, value, CompareOptions.None);
        }

        public int IndexOf(string source, string value)
        {
            return IndexOf(source, value, CompareOptions.None);
        }

        public int IndexOf(string source, char value, CompareOptions options)
        {
            return IndexOf(source, MemoryMarshal.CreateReadOnlySpan(ref value, 1), options);
        }

        public int IndexOf(string source, string value, CompareOptions options)
        {
            return IndexOf(source.AsSpan(), value.AsSpan(), options);
        }

        public int IndexOf(string source, char value, int startIndex)
        {
            return IndexOf(source, value, startIndex, CompareOptions.None);
        }

        public int IndexOf(string source, string value, int startIndex)
        {
            return IndexOf(source, value, startIndex, CompareOptions.None);
        }

        public int IndexOf(string source, char value, int startIndex, CompareOptions options)
        {
            return IndexOf(source, value, startIndex, source.Length - startIndex, options);

        }

        public int IndexOf(string source, string value, int startIndex, CompareOptions options)
        {
            return IndexOf(source, value, startIndex, source.Length - startIndex, options);
        }

        public int IndexOf(string source, char value, int startIndex, int count)
        {
            return IndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public int IndexOf(string source, string value, int startIndex, int count)
        {
            return IndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public unsafe int IndexOf(string source, char value, int startIndex, int count, CompareOptions options)
        {
            if (!MiniString.TryGetSpan(source, startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                // Bounds check failed - figure out exactly what went wrong so that we can
                // surface the correct argument exception.
                throw new ArgumentOutOfRangeException();
            }

            int result = IndexOf(sourceSpan, MemoryMarshal.CreateReadOnlySpan(ref value, 1), options);
            if (result >= 0)
            {
                result += startIndex;
            }

            return result;
        }

        public unsafe int IndexOf(string source, string value, int startIndex, int count, CompareOptions options)
        {
            if (!MiniString.TryGetSpan(source, startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                // Bounds check failed - figure out exactly what went wrong so that we can
                // surface the correct argument exception.
                throw new ArgumentOutOfRangeException();
            }

            int result = IndexOf(sourceSpan, value, options);
            if (result >= 0)
            {
                result += startIndex;
            }

            return result;
        }

        /// <summary>
        /// Searches for the first occurrence of a substring within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The substring to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where the substring <paramref name="value"/>
        /// first appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public unsafe int IndexOf(ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options = CompareOptions.None)
        {
            return source.IndexOf(value);
        }

        /// <summary>
        /// Searches for the first occurrence of a substring within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The substring to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <param name="matchLength">When this method returns, contains the number of characters of
        /// <paramref name="source"/> that matched the desired value. This may be different than the
        /// length of <paramref name="value"/> if a linguistic comparison is performed. Set to 0
        /// if <paramref name="value"/> is not found within <paramref name="source"/>.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where the substring <paramref name="value"/>
        /// first appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        /// <remarks>
        /// This method has greater overhead than other IndexOf overloads which don't
        /// take a <paramref name="matchLength"/> argument. Call this overload only if you require
        /// the match length information.
        /// </remarks>
        public unsafe int IndexOf(ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options, out int matchLength)
        {
            int tempMatchLength;
            int retVal = IndexOf(source, value, &tempMatchLength, options, fromBeginning: true);
            matchLength = tempMatchLength;
            return retVal;
        }

#if NET5_0
        /// <summary>
        /// Searches for the first occurrence of a <see cref="Rune"/> within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The <see cref="Rune"/> to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where <paramref name="value"/>
        /// first appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public int IndexOf(ReadOnlySpan<char> source, Rune value, CompareOptions options = CompareOptions.None)
        {
            Span<char> valueAsUtf16 = stackalloc char[4];
            int charCount = value.EncodeToUtf16(valueAsUtf16);
            return IndexOf(source, valueAsUtf16.Slice(0, charCount), options);
        }
#endif

        /// <summary>
        /// IndexOf overload used when the caller needs the length of the matching substring.
        /// Caller needs to ensure <paramref name="matchLengthPtr"/> is non-null and points
        /// to a valid address. This method will validate <paramref name="options"/>.
        /// </summary>
        private unsafe int IndexOf(ReadOnlySpan<char> source, ReadOnlySpan<char> value, int* matchLengthPtr, CompareOptions options, bool fromBeginning)
        {
            *matchLengthPtr = 0;

            int retVal = 0;

            retVal = (fromBeginning) ? source.IndexOf(value) : source.LastIndexOf(value);

            if (retVal >= 0)
            {
                *matchLengthPtr = value.Length;
            }

            return retVal;
        }

        /// <summary>
        /// Returns the last index where value is found in string.  The
        /// search starts from startIndex and ends at endIndex.  Returns -1 if
        /// the specified value is not found.  If value equals string.Empty,
        /// endIndex is returned.  Throws IndexOutOfRange if startIndex or
        /// endIndex is less than zero or greater than the length of string.
        /// Throws ArgumentException if value (as a string) is null.
        /// </summary>
        public int LastIndexOf(string source, char value)
        {
            return LastIndexOf(source, value, CompareOptions.None);
        }

        public int LastIndexOf(string source, string value)
        {
            return LastIndexOf(source, value, CompareOptions.None);
        }

        public int LastIndexOf(string source, char value, CompareOptions options)
        {
            return LastIndexOf(source, MemoryMarshal.CreateReadOnlySpan(ref value, 1), options);
        }

        public int LastIndexOf(string source, string value, CompareOptions options)
        {
            return LastIndexOf(source.AsSpan(), value.AsSpan(), options);
        }

        public int LastIndexOf(string source, char value, int startIndex)
        {
            return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
        }

        public int LastIndexOf(string source, string value, int startIndex)
        {
            return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
        }

        public int LastIndexOf(string source, char value, int startIndex, CompareOptions options)
        {
            return LastIndexOf(source, value, startIndex, startIndex + 1, options);
        }

        public int LastIndexOf(string source, string value, int startIndex, CompareOptions options)
        {
            return LastIndexOf(source, value, startIndex, startIndex + 1, options);
        }

        public int LastIndexOf(string source, char value, int startIndex, int count)
        {
            return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public int LastIndexOf(string source, string value, int startIndex, int count)
        {
            return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public int LastIndexOf(string source, char value, int startIndex, int count, CompareOptions options)
        {
            TryAgain:

            // Previous versions of the Framework special-cased empty 'source' to allow startIndex = -1 or startIndex = 0,
            // ignoring 'count' and short-circuiting the entire operation. We'll silently fix up the 'count' parameter
            // if this occurs.
            //
            // See the comments just before string.IndexOf(string) for more information on how these computations are
            // performed.
            if ((uint)startIndex >= (uint)source.Length)
            {
                if (startIndex == -1 && source.Length == 0)
                {
                    count = 0; // normalize
                }
                else if (startIndex == source.Length)
                {
                    // The caller likely had an off-by-one error when invoking the API. The Framework has historically
                    // allowed for this and tried to fix up the parameters, so we'll continue to do so for compat.
                    startIndex--;
                    if (count > 0)
                    {
                        count--;
                    }

                    goto TryAgain; // guaranteed never to loop more than once
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            startIndex = startIndex - count + 1; // this will be the actual index where we begin our search

            if (!MiniString.TryGetSpan(source, startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                throw new ArgumentOutOfRangeException();
            }

            int retVal = LastIndexOf(sourceSpan, MemoryMarshal.CreateReadOnlySpan(ref value, 1), options);
            if (retVal >= 0)
            {
                retVal += startIndex;
            }

            return retVal;
        }

        public int LastIndexOf(string source, string value, int startIndex, int count, CompareOptions options)
        {
        TryAgain:

            // Previous versions of the Framework special-cased empty 'source' to allow startIndex = -1 or startIndex = 0,
            // ignoring 'count' and short-circuiting the entire operation. We'll silently fix up the 'count' parameter
            // if this occurs.
            //
            // See the comments just before string.IndexOf(string) for more information on how these computations are
            // performed.
            if ((uint)startIndex >= (uint)source.Length)
            {
                if (startIndex == -1 && source.Length == 0)
                {
                    count = 0; // normalize
                }
                else if (startIndex == source.Length)
                {
                    // The caller likely had an off-by-one error when invoking the API. The Framework has historically
                    // allowed for this and tried to fix up the parameters, so we'll continue to do so for compat.
                    startIndex--;
                    if (count > 0)
                    {
                        count--;
                    }

                    goto TryAgain; // guaranteed never to loop more than once
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            startIndex = startIndex - count + 1; // this will be the actual index where we begin our search

            if (!MiniString.TryGetSpan(source, startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                throw new ArgumentOutOfRangeException();
            }

            int retVal = LastIndexOf(sourceSpan, value, options);
            if (retVal >= 0)
            {
                retVal += startIndex;
            }

            return retVal;
        }

        /// <summary>
        /// Searches for the last occurrence of a substring within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The substring to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where the substring <paramref name="value"/>
        /// last appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public unsafe int LastIndexOf(ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options = CompareOptions.None)
        {
            return source.LastIndexOf(value);
        }

        /// <summary>
        /// Searches for the last occurrence of a substring within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The substring to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <param name="matchLength">When this method returns, contains the number of characters of
        /// <paramref name="source"/> that matched the desired value. This may be different than the
        /// length of <paramref name="value"/> if a linguistic comparison is performed. Set to 0
        /// if <paramref name="value"/> is not found within <paramref name="source"/>.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where the substring <paramref name="value"/>
        /// last appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        /// <remarks>
        /// This method has greater overhead than other IndexOf overloads which don't
        /// take a <paramref name="matchLength"/> argument. Call this overload only if you require
        /// the match length information.
        /// </remarks>
        public unsafe int LastIndexOf(ReadOnlySpan<char> source, ReadOnlySpan<char> value, CompareOptions options, out int matchLength)
        {
            int tempMatchLength;
            int retVal = IndexOf(source, value, &tempMatchLength, options, fromBeginning: false);
            matchLength = tempMatchLength;
            return retVal;
        }

#if NET5_0
        /// <summary>
        /// Searches for the last occurrence of a <see cref="Rune"/> within a source string.
        /// </summary>
        /// <param name="source">The string to search within.</param>
        /// <param name="value">The <see cref="Rune"/> to locate within <paramref name="source"/>.</param>
        /// <param name="options">The <see cref="CompareOptions"/> to use during the search.</param>
        /// <returns>
        /// The zero-based index into <paramref name="source"/> where <paramref name="value"/>
        /// last appears; or -1 if <paramref name="value"/> cannot be found within <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> contains an unsupported combination of flags.
        /// </exception>
        public unsafe int LastIndexOf(ReadOnlySpan<char> source, Rune value, CompareOptions options = CompareOptions.None)
        {
            Span<char> valueAsUtf16 = stackalloc char[4];
            int charCount = value.EncodeToUtf16(valueAsUtf16);
            return LastIndexOf(source, valueAsUtf16.Slice(0, charCount), options);
        }
#endif

        public override bool Equals(object? value)
        {
            return value is CompareInfo otherCompareInfo
                && Name == otherCompareInfo.Name;
        }

        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// This method performs the equivalent of of creating a Sortkey for a string from CompareInfo,
        /// then generates a randomized hashcode value from the sort key.
        ///
        /// The hash code is guaranteed to be the same for string A and B where A.Equals(B) is true and both
        /// the CompareInfo and the CompareOptions are the same. If two different CompareInfo objects
        /// treat the string the same way, this implementation will treat them differently (the same way that
        /// Sortkey does at the moment).
        /// </summary>
        public int GetHashCode(string source, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }

            return GetHashCode(source.AsSpan(), options);
        }

        public int GetHashCode(ReadOnlySpan<char> source, CompareOptions options)
        {
            int result = 4711;
            for (int i = 0; i < source.Length; i++)
            {
                result ^= source[i];
                result <<= 2;
            }

            return result;
        }

        public override string ToString() => "CompareInfo - " + Name;

        public SortVersion Version
        {
            get
            {
                const int LOCALE_INVARIANT = 0x007F;
                return new SortVersion(0, new Guid(0, 0, 0, 0, 0, 0, 0,
                                                                        (byte)(LOCALE_INVARIANT >> 24),
                                                                        (byte)((LOCALE_INVARIANT & 0x00FF0000) >> 16),
                                                                        (byte)((LOCALE_INVARIANT & 0x0000FF00) >> 8),
                                                                        (byte)(LOCALE_INVARIANT & 0xFF)));
            }
        }

        public int LCID => CultureInfo.GetCultureInfo(Name).LCID;

        internal static unsafe IntPtr NlsGetSortHandle(string cultureName)
        {
            return IntPtr.Zero;
        }
    }
}

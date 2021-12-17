// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This file is copied and adapted from https://github.com/zanders3/json. MIT license.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Iot.Device.Common
{
    /// <summary>
    /// Really simple JSON parser
    /// </summary>
    // - Attempts to parse JSON files with minimal GC allocation
    // - Nice and simple "[1,2,3]".FromJson<List<int>>() API
    // - Classes and structs can be parsed too!
    //      class Foo { public int Value; }
    //      "{\"Value\":10}".FromJson<Foo>()
    // - Can parse JSON without type information into Dictionary<string,object> and List<object> e.g.
    //      "[1,2,3]".FromJson<object>().GetType() == typeof(List<object>)
    //      "{\"Value\":10}".FromJson<object>().GetType() == typeof(Dictionary<string,object>)
    // - No JIT Emit support to support AOT compilation on iOS
    // - Attempts are made to NOT throw an exception if the JSON is corrupted or invalid: returns null instead.
    // - Only public fields and property setters on classes/structs will be written to
    //
    // Limitations:
    // - No JIT Emit support to parse structures quickly
    // - Limited to parsing <2GB JSON files (due to int.MaxValue)
    // - Parsing of abstract classes or interfaces is NOT supported and will throw an exception.
    public class JsonParser
    {
        private readonly Stack<List<string>> _splitArrayPool;
        private readonly StringBuilder _stringBuilder;
        private readonly Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoCache;
        private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyInfoCache;

        /// <summary>
        /// Creates an instance of the parser. The parser is reusable.
        /// </summary>
        public JsonParser()
        {
            _splitArrayPool = new();
            _stringBuilder = new();
            _fieldInfoCache = new();
            _propertyInfoCache = new();
        }

        /// <summary>
        /// Decode a string into an object of type T
        /// </summary>
        /// <typeparam name="T">The object type to decode</typeparam>
        /// <param name="json">The input string</param>
        /// <returns>An instance of T or null in case of an error</returns>
        /// <exception cref="Exception">In case there's a problem with constructing instances of T</exception>
        public T? FromJson<T>(string json)
        {
            // Remove all whitespace not within strings to make parsing simpler
            _stringBuilder.Length = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"')
                {
                    i = AppendUntilStringEnd(true, i, json);
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                _stringBuilder.Append(c);
            }

            // Parse the thing!
            return (T?)ParseValue(typeof(T), _stringBuilder.ToString());
        }

        private int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            _stringBuilder.Append(json[startIdx]);
            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    if (appendEscapeCharacter)
                    {
                        _stringBuilder.Append(json[i]);
                    }

                    _stringBuilder.Append(json[i + 1]);
                    i++; // Skip next character as it is escaped
                }
                else if (json[i] == '"')
                {
                    _stringBuilder.Append(json[i]);
                    return i;
                }
                else
                {
                    _stringBuilder.Append(json[i]);
                }
            }

            return json.Length - 1;
        }

        // Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        private List<string> Split(string json)
        {
            List<string> splitArray = _splitArrayPool.Count > 0 ? _splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();
            if (json.Length == 2)
            {
                return splitArray;
            }

            int parseDepth = 0;
            _stringBuilder.Length = 0;
            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '"':
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case ',':
                    case ':':
                        if (parseDepth == 0)
                        {
                            splitArray.Add(_stringBuilder.ToString());
                            _stringBuilder.Length = 0;
                            continue;
                        }

                        break;
                }

                _stringBuilder.Append(json[i]);
            }

            splitArray.Add(_stringBuilder.ToString());

            return splitArray;
        }

        internal object? ParseValue(Type type, string json)
        {
            if (type == typeof(string))
            {
                if (json.Length <= 2)
                {
                    return string.Empty;
                }

                StringBuilder parseStringBuilder = new StringBuilder(json.Length);
                for (int i = 1; i < json.Length - 1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
                    {
                        int j = "\"\\nrtbf/".IndexOf(json[i + 1]);
                        if (j >= 0)
                        {
                            parseStringBuilder.Append("\"\\\n\r\t\b\f/"[j]);
                            ++i;
                            continue;
                        }

                        if (json[i + 1] == 'u' && i + 5 < json.Length - 1)
                        {
                            UInt32 c = 0;
                            if (UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c))
                            {
                                parseStringBuilder.Append((char)c);
                                i += 5;
                                continue;
                            }
                        }
                    }

                    parseStringBuilder.Append(json[i]);
                }

                return parseStringBuilder.ToString();
            }

            if (type == typeof(Int32))
            {
                int result;
                Int32.TryParse(json, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }

            if (type == typeof(UInt32))
            {
                UInt32 result;
                UInt32.TryParse(json, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }

            if (type == typeof(double))
            {
                double result;
                double.TryParse(json, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }

            if (type == typeof(decimal))
            {
                decimal result;
                decimal.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }

            if (type == typeof(DateTime))
            {
                DateTime result;
                DateTime.TryParse(json.Replace("\"", string.Empty), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);
                return result;
            }

            if (json == "null")
            {
                return null;
            }

            if (type.IsEnum)
            {
                if (json[0] == '"')
                {
                    json = json.Substring(1, json.Length - 2);
                }

                try
                {
                    return Enum.Parse(type, json, false);
                }
                catch
                {
                    return 0;
                }
            }

            if (type.IsArray)
            {
                Type arrayType = type.GetElementType()!;
                if (json[0] != '[' || json[json.Length - 1] != ']')
                {
                    return null;
                }

                List<string> elems = Split(json);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);
                for (int i = 0; i < elems.Count; i++)
                {
                    newArray.SetValue(ParseValue(arrayType, elems[i]), i);
                }

                _splitArrayPool.Push(elems);
                return newArray;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = type.GetGenericArguments()[0];
                if (json[0] != '[' || json[json.Length - 1] != ']')
                {
                    return null;
                }

                List<string> elems = Split(json);
                var list = (IList)type.GetConstructor(new Type[] { typeof(int) })!.Invoke(new object[] { elems.Count });
                for (int i = 0; i < elems.Count; i++)
                {
                    list.Add(ParseValue(listType, elems[i]));
                }

                _splitArrayPool.Push(elems);
                return list;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType, valueType;
                {
                    Type[] args = type.GetGenericArguments();
                    keyType = args[0];
                    valueType = args[1];
                }

                // Refuse to parse dictionary keys that aren't of type string
                if (keyType != typeof(string))
                {
                    return null;
                }

                // Must be a valid dictionary element
                if (json[0] != '{' || json[json.Length - 1] != '}')
                {
                    return null;
                }

                // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                {
                    return null;
                }

                var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) })!.Invoke(new object[] { elems.Count / 2 });
                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                    {
                        continue;
                    }

                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object? val = ParseValue(valueType, elems[i + 1]);
                    dictionary[keyValue] = val;
                }

                return dictionary;
            }

            if (type == typeof(object))
            {
                return ParseAnonymousValue(json);
            }

            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                return ParseObject(type, json);
            }

            return null;
        }

        private object? ParseAnonymousValue(string json)
        {
            if (json.Length == 0)
            {
                return null;
            }

            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                {
                    return null;
                }

                var dict = new Dictionary<string, object?>(elems.Count / 2);
                for (int i = 0; i < elems.Count; i += 2)
                {
                    dict[elems[i].Substring(1, elems[i].Length - 2)] = ParseAnonymousValue(elems[i + 1]);
                }

                return dict;
            }

            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                List<string> items = Split(json);
                var finalList = new List<object?>(items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    finalList.Add(ParseAnonymousValue(items[i]));
                }

                return finalList;
            }

            if (json[0] == '"' && json[json.Length - 1] == '"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }

            if (char.IsDigit(json[0]) || json[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }

            if (json == "true")
            {
                return true;
            }

            if (json == "false")
            {
                return false;
            }

            // handles json == "null" as well as invalid JSON
            return null;
        }

        private Dictionary<string, T> CreateMemberNameDictionary<T>(T[] members)
            where T : MemberInfo
        {
            Dictionary<string, T> nameToMember = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < members.Length; i++)
            {
                T member = members[i];
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                {
                    continue;
                }

                string name = member.Name;
                if (member.IsDefined(typeof(DataMemberAttribute), true))
                {
                    DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true)!;
                    if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                    {
                        name = dataMemberAttribute.Name;
                    }
                }

                nameToMember.Add(name, member);
            }

            return nameToMember;
        }

        private object? ParseObject(Type type, string json)
        {
            // This may throw, but only in case of a programming error (e.g. Type has no default ctor)
            object? instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                return null;
            }

            // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json);
            if (elems.Count % 2 != 0)
            {
                return instance;
            }

            Dictionary<string, FieldInfo>? nameToField;
            Dictionary<string, PropertyInfo>? nameToProperty;
            if (!_fieldInfoCache.TryGetValue(type, out nameToField))
            {
                nameToField = CreateMemberNameDictionary(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                _fieldInfoCache.Add(type, nameToField);
            }

            if (!_propertyInfoCache.TryGetValue(type, out nameToProperty))
            {
                nameToProperty = CreateMemberNameDictionary(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                _propertyInfoCache.Add(type, nameToProperty);
            }

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                {
                    continue;
                }

                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                FieldInfo? fieldInfo;
                PropertyInfo? propertyInfo;
                if (nameToField.TryGetValue(key, out fieldInfo))
                {
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                }
                else if (nameToProperty.TryGetValue(key, out propertyInfo))
                {
                    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
                }
            }

            return instance;
        }
    }
}

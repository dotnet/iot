// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Reflection;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Helper to get the enumeration description instead of enumeration name
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Returns the description attribute or the name of the enum
        /// </summary>
        /// <typeparam name="T">A valid enumeration</typeparam>
        /// <param name="enumerationValue">The type of enumeration</param>
        /// <returns>The description attribute or name if not existing</returns>
        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            if (!enumerationValue.GetType().IsEnum)
            {
                throw new ArgumentException($"EnumerationValue {nameof(enumerationValue)} must be of Enum type");
            }

            var enumVal = enumerationValue.ToString();
            enumVal = enumVal ?? string.Empty;

            MemberInfo[]? memberInfo = enumerationValue.GetType().GetMember(enumVal);
            if (memberInfo != null && memberInfo?.Length > 0)
            {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes?.Length > 0)
                {
                    // return the description
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            // Return just the name if attribute can't be found
            return enumVal;
        }
    }
}

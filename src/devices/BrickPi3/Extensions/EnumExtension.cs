// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BrickPi3.Extensions
{
    /// <summary>
    /// Extensions to get next or previous enum
    /// </summary>
    internal static class EnumExtensions
    {
        public static T Next<T>(this T src)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argumnent {typeof(T).FullName} is not an Enum");
            }

            T[] arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(arr, src) + 1;
            return (arr.Length == j) ? arr[0] : arr[j];
        }

        public static T Previous<T>(this T src)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argumnent {typeof(T).FullName} is not an Enum");
            }

            T[] arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(arr, src) - 1;
            return (j < 0) ? arr[arr.Length - 1] : arr[j];
        }
    }
}

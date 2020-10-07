//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class FlagsHelper
    {
        /// <summary>
        /// Sets the provided flag TODO
        /// </summary>
        public static bool IsSet<T>(T flags, T flag)
            where T : Enum
        {
            return flags.HasFlag(flag);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static T SetValue<T>(T flags, T flag, bool enabled)
            where T : struct
        {
            if (enabled)
            {
                Set(ref flags, flag);
            }
            else
            {
                Unset(ref flags, flag);
            }

            return flags;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void Set<T>(ref T flags, T flag)
            where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void Unset<T>(ref T flags, T flag)
            where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }
}

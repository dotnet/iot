//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Common
{
    /// <summary>
    /// Contains functionality to operate on a bit field <see cref="Enum"/>,
    /// which is an <see cref="Enum"/> decorated with the <see cref="FlagsAttribute"/>
    /// and which represents a set of flags.
    /// </summary>
    public static class FlagsHelper
    {
        /// <summary>
        /// Determines whether one or more bit fields are set in the provided <paramref name="flags"/> bit field.
        /// </summary>
        /// <typeparam name="T"><see cref="Enum"/> decorated with the <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="flags">Bit field to use as basis for the lookup.</param>
        /// <param name="flag">One or more bit fields to lookup.</param>
        public static bool IsSet<T>(T flags, T flag)
            where T : Enum
        {
            return flags.HasFlag(flag);
        }

        /// <summary>
        /// Sets or unsets one or more bit fields in the provided <paramref name="flags"/> bit field.
        /// </summary>
        /// <typeparam name="T"><see cref="Enum"/> decorated with the <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="flags">Bit field to apply the <paramref name="flag"/> to.</param>
        /// <param name="flag">One or more bit fields to set or unset.</param>
        /// <param name="enabled"><see langword="true"/> if <paramref name="flag"/> should be set; <see langword="false"/> if it should be unset.</param>
        public static void SetValue<T>(ref T flags, T flag, bool enabled)
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
        }

        private static void Set<T>(ref T flags, T flag)
            where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        private static void Unset<T>(ref T flags, T flag)
            where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }
}

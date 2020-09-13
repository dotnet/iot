//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    internal static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag)
            where T : struct
        {
            // Cast to object necessary due to C#'s restriction against a where T : Enum constraint
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

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

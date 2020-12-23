// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Interface attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InterfaceAttribute : Attribute
    {
        /// <summary>
        /// Display name of the interface
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Constructs InterfaceAttrbute
        /// </summary>
        /// <param name="displayName">Display name of the interface</param>
        public InterfaceAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}

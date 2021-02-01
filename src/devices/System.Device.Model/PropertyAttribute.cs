// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Property of the interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PropertyAttribute : Attribute
    {
        /// <summary>
        /// Name of the property in the interface
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Display name of the property
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Constructs PropertyAttribute
        /// </summary>
        /// <param name="name">Optional name of the property in the interface. If not provided property name will be used.</param>
        /// <param name="displayName">Optional name of the property in the interface. If not provided it may be infered from the type.</param>
        public PropertyAttribute(string? name = null, string? displayName = null)
        {
            Name = name;
            DisplayName = displayName;
        }
    }
}

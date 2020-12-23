// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Component attribute class referencing to an interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Name of the component
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Constructs ComponentAttribute
        /// </summary>
        /// <param name="name">Optional name of the component. If not provided property name will be used.</param>
        public ComponentAttribute(string? name = null)
        {
            Name = name;
        }
    }
}

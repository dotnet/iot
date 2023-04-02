// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Device
{
    /// <summary>
    /// A class to provide informational data about system components and active drivers and bindings.
    /// An instance of this class is typically obtained by calling QueryComponentInformation on any supported object.
    /// The structure represents a tree of connected devices, e.g. a controller and its associated drivers.
    /// </summary>
    /// <remarks>
    /// This class is currently reserved for debugging purposes. Its behavior its and signature are subject to change.
    /// </remarks>
    public record ComponentInformation
    {
        private const int MaximumRecursionDepth = 50;
        private readonly List<ComponentInformation> _subComponents;

        /// <summary>
        /// Create a new instance of <see cref="ComponentInformation"/>
        /// </summary>
        /// <param name="instance">The instance this information belongs to</param>
        /// <param name="description">An user-readable name for the object</param>
        public ComponentInformation(object instance, string description)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Instance = instance;

            Name = instance.GetType().FullName!;
            Description = description ?? throw new ArgumentNullException(nameof(description));

            _subComponents = new List<ComponentInformation>();
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// The name of the type this instance represents.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// The actual instance represented.
        /// This should only be used for querying information, not for operations on the object.
        /// </summary>
        protected object Instance
        {
            get;
        }

        /// <summary>
        /// The user-friendly name of the object
        /// </summary>
        public string Description
        {
            get; init;
        }

        /// <summary>
        /// A list of additional properties that belong to this object.
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get;
            init;
        }

        /// <summary>
        /// The list of subcomponents.
        /// </summary>
        /// <remarks>
        /// Note to implementors: Be careful not to generate cycles in the tree!
        /// </remarks>
        public IReadOnlyList<ComponentInformation> SubComponents => _subComponents.AsReadOnly();

        /// <summary>
        /// Adds another component as subcomponent of this one
        /// </summary>
        /// <param name="subComponent">Component to add</param>
        /// <exception cref="ArgumentNullException">The component to add is null.</exception>
        public virtual void AddSubComponent(ComponentInformation subComponent)
        {
            if (_subComponents == null)
            {
                throw new ArgumentNullException(nameof(subComponent));
            }

            // Don't add the same element twice
            if (_subComponents.All(x => x.Instance != subComponent.Instance))
            {
                _subComponents.Add(subComponent);
            }
        }

        private static string PrintComponentDescription(ComponentInformation component)
        {
            return $"{{{component.Name}}} {component.Description}{Environment.NewLine}";
        }

        /// <summary>
        /// Provides the string representation of all sub-components
        /// </summary>
        /// <param name="output">The stringbuilder to extend</param>
        /// <param name="ident">The level of current ident</param>
        protected virtual void SubComponentsToString(StringBuilder output, int ident)
        {
            if (ident > MaximumRecursionDepth)
            {
                output.Append(new string(' ', ident));
                output.AppendLine($"... (further levels skipped, possible loop in device tree)");
                return;
            }

            foreach (var component in _subComponents)
            {
                output.Append(new string(' ', ident));
                output.Append('\u2514'); // border element
                output.Append(PrintComponentDescription(component));
                component.SubComponentsToString(output, ident + 1);
            }
        }

        /// <summary>
        /// Creates a string representation of this object.
        /// </summary>
        /// <returns>A multi-line string in form of a tree.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder(PrintComponentDescription(this));
            SubComponentsToString(b, 1);

            return b.ToString().TrimEnd();
        }
    }
}

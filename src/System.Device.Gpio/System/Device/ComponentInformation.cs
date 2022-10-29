// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable CS1591
namespace System.Device
{
    public record ComponentInformation
    {
        private readonly List<ComponentInformation> _subComponents;

        public ComponentInformation(object instance, string name)
            : this(instance, name, string.Empty)
        {
        }

        public ComponentInformation(object instance, string name, string version)
            : this(instance, name, version, ComponentState.Unknown)
        {
        }

        public ComponentInformation(object instance, string name, ComponentState state)
            : this(instance, name, string.Empty, state)
        {
        }

        public ComponentInformation(object instance, string name, string version, ComponentState componentState)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Type = instance.GetType().FullName!;
            Name = name;

            Version = version;
            State = componentState;

            if (string.IsNullOrWhiteSpace(Version))
            {
                var assembly = instance.GetType().Assembly;
                var v = (AssemblyInformationalVersionAttribute?)assembly.GetCustomAttributes().FirstOrDefault(x => x is AssemblyInformationalVersionAttribute);

                if (v != null)
                {
                    Version = v.InformationalVersion;
                }
            }

            _subComponents = new List<ComponentInformation>();
        }

        public string Type
        {
            get;
        }

        public string Name
        {
            get; init;
        }

        public string Version
        {
            get; init;
        }

        public ComponentState State
        {
            get; init;
        }

        public IReadOnlyList<ComponentInformation> SubComponents => _subComponents.AsReadOnly();

        public void AddSubComponent(ComponentInformation subComponent)
        {
            if (_subComponents == null)
            {
                throw new ArgumentNullException(nameof(subComponent));
            }

            _subComponents.Add(subComponent);
        }

        private static string PrintComponentDescription(ComponentInformation component)
        {
            string v = string.Empty;
            if (!string.IsNullOrWhiteSpace(component.Version))
            {
                v = $" Version {component.Version}";
            }

            return $"{{{component.Type}}} {component.Name}{v}{Environment.NewLine}";
        }

        protected virtual void SubComponentToString(StringBuilder output, int ident)
        {
            foreach (var component in _subComponents)
            {
                output.Append(new string(' ', ident));
                output.Append('\u2514'); // border element
                output.Append(PrintComponentDescription(component));
                component.SubComponentToString(output, ident + 1);
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder(PrintComponentDescription(this));
            SubComponentToString(b, 1);

            return b.ToString().TrimEnd();
        }
    }
}

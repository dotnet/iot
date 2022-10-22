// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
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
            _subComponents = new List<ComponentInformation>();
        }

        public string Type
        {
            get;
        }

        public string Name
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }

        public ComponentState State
        {
            get;
            set;
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

        protected virtual void SubComponentToString(StringBuilder output, int ident)
        {
            foreach (var component in _subComponents)
            {
                output.Append(new string(' ', ident));
                output.Append('\u2514'); // border element
                output.Append($"{{{component.Type}}} {component.Name}{Environment.NewLine}");
                component.SubComponentToString(output, ident + 1);
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder($"{{{Type}}} {Name}{Environment.NewLine}");
            SubComponentToString(b, 1);

            return b.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class IlCode
    {
        public IlCode(MethodBase method, byte[]? ilBytes)
        {
            Method = method;
            IlBytes = ilBytes;
            DependentMethods = new List<MethodBase>();
            DependentFields = new List<FieldInfo>();
            DependentTypes = new List<TypeInfo>();
            Name = $"{method.DeclaringType} - {method}";
        }

        public IlCode(MethodBase method, byte[]? ilBytes, List<MethodBase> methods, List<FieldInfo> fields, List<TypeInfo> types)
        {
            Method = method;
            IlBytes = ilBytes;
            DependentMethods = methods;
            DependentFields = fields;
            DependentTypes = types;
            Name = $"{method.DeclaringType} - {method}";
        }

        public MethodBase Method
        {
            get;
        }

        public byte[]? IlBytes
        {
            get;
        }

        public string Name
        {
            get;
        }

        /// <summary>
        /// Methods (and constructors) used by this method
        /// </summary>
        public List<MethodBase> DependentMethods
        {
            get;
        }

        /// <summary>
        /// Fields (static and instance, own and from other types) used by this method
        /// </summary>
        public List<FieldInfo> DependentFields
        {
            get;
        }

        /// <summary>
        /// Types used by this method
        /// </summary>
        public List<TypeInfo> DependentTypes
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

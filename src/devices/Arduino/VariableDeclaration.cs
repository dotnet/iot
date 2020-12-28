using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal struct VariableDeclaration
    {
        public VariableDeclaration(VariableKind type, ushort size)
        {
            Type = type;
            Size = size;
        }

        public VariableKind Type
        {
            get;
            set;
        }

        public ushort Size
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class ExecutionSet
    {
        private List<ArduinoMethodDeclaration> _methods;

        public ExecutionSet()
        {
            _methods = new List<ArduinoMethodDeclaration>();
        }

        public List<ArduinoMethodDeclaration> Methods
        {
            get
            {
                return _methods;
            }
        }
    }
}

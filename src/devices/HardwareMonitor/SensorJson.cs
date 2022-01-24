using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareMonitor
{
    internal class SensorJson
    {
        public SensorJson()
        {
            NodeId = string.Empty;
            Name = string.Empty;
            Type = string.Empty;
            Unit = string.Empty;
            Parent = string.Empty;
        }

        public string NodeId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Unit
        {
            get;
            set;
        }

        public double Value
        {
            get;
            set;
        }

        public string Parent
        {
            get; set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

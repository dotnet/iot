using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareMonitor
{
    internal class ComputerJson
    {
        public ComputerJson()
        {
            ComputerName = string.Empty;
            Hardware = Array.Empty<HardwareJson>();
        }

        public string ComputerName
        {
            get;
            set;
        }

        public int LogicalProcessorCount
        {
            get;
            set;
        }

        public HardwareJson[] Hardware
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ComputerName;
        }
    }

    internal class HardwareJson
    {
        public HardwareJson()
        {
            NodeId = String.Empty;
            Name = String.Empty;
            Sensors = Array.Empty<SensorJson>();
            Parent = string.Empty;
            HardwareType = string.Empty;
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

        public string Parent
        {
            get;
            set;
        }

        public SensorJson[] Sensors
        {
            get;
            set;
        }

        public string HardwareType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }

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

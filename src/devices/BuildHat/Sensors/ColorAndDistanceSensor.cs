using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Color and distance sensor.
    /// </summary>
    public class ColorAndDistanceSensor : ColorSensor
    {
        /// <summary>
        /// Creates a color and distance sensor.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        public ColorAndDistanceSensor(Brick brick, SensorPort port)
            : base(brick, port, SensorType.ColourAndDistanceSensor)
        {
        }

        /// <inheritdoc/>
        public override string GetSensorName() => "Color and distance sensor";
    }
}

using System;
using System.Linq;
using System.IO.Ports;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represents and arrow object
    /// </summary>
    public class Arrow : HuskyObject
    {
        /// <summary>
        /// Origin of the arrow
        /// </summary>
        public Point Origin { get; private set; }

        /// <summary>
        /// Trget of the arrow
        /// </summary>
        public Point Target { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin of the arrow</param>
        /// <param name="target">Target of the arrow</param>
        /// <param name="id">Learnt Id</param>
        public Arrow(Point origin, Point target, int id = 0)
        : base(id)
        {
            Origin = origin;
            Target = target;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Arrow: o={Origin}, t={Target}, id={Id}";
        }

        /// <summary>
        /// Creates a new object from the wire protocol data
        /// </summary>
        /// <param name="data">the bytes from the wire</param>
        /// <returns>initialized object</returns>
        public static Arrow FromData(ReadOnlySpan<byte> data)
        {
            var buffer = data.ToArray();
            var ox = buffer[0] + buffer[1] * 0x100;
            var oy = buffer[2] + buffer[3] * 0x100;
            var tx = buffer[4] + buffer[5] * 0x100;
            var ty = buffer[6] + buffer[7] * 0x100;
            var id = buffer[8] + buffer[9] * 0x100;
            return new Arrow(new Point(ox, oy), new Point(tx, ty), id);
        }
    }
}
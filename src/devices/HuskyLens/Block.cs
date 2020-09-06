using System;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Representa block
    /// </summary>
    public class Block : HuskyObject
    {
        /// <summary>
        /// Center of the block
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// Width of the block , range:0-319
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the block, range:0-239
        /// </summary>
        /// <value></value>
        public int Height { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">The center</param>
        /// <param name="width">The width</param>
        /// <param name="height">The Height</param>
        /// <param name="id">The learned id</param>
        public Block(Point center, int width, int height, int id = 0)
        : base(id)
        {
            Center = center;
            Width = width;
            Height = height;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Block: c={Center}, w={Width}, h={Height}, id={Id}";
        }

        /// <summary>
        /// Creates a new object from the wire protocol data
        /// </summary>
        /// <param name="data">the bytes from the wire</param>
        /// <returns>initialized object</returns>
        public static Block FromData(ReadOnlySpan<byte> data)
        {
            var buffer = data.ToArray();
            var x = buffer[0] + buffer[1] * 0x100;
            var y = buffer[2] + buffer[3] * 0x100;
            var width = buffer[4] + buffer[5] * 0x100;
            var height = buffer[6] + buffer[7] * 0x100;
            var id = buffer[8] + buffer[9] * 0x100;
            return new Block(new Point(x, y), width, height, id);
        }
    }
}

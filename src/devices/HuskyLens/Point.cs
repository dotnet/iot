namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represents apoint in camera coordinates
    /// </summary>
    public class Point
    {
        /// <summary>
        /// The x coordinate, range:0-319
        /// </summary>
        public int X { get; }

        /// <summary>
        /// the y coordinate, range:-239
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="x">x coordinate, range:0-319</param>
        /// <param name="y">y coordinate range:0-239</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}

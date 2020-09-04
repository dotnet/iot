using System;
using System.Linq;
using System.IO.Ports;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// todo
    /// </summary>
    public class HuskyLens
    {
        private IBinaryConnection _connection;

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="connection">guess</param>
        public HuskyLens(SerialPort connection)
        {
            _connection = new SerialPortConnection(connection);
        }

        /// <summary>
        /// Ping the thing
        /// </summary>
        /// <returns>true for success, guess the rest</returns>
        public bool Ping()
        {
            _connection.Write(new byte[] { 0x55, 0xAA, 0x11, 0x00, 0x2C, 0x3C });
            WaitForOK();
            return true;
        }

        /// <summary>
        /// Sets the currently active algorithm
        /// </summary>
        /// <param name="algorithm">the algorithm</param>
        public void SetAlgorithm(Algorithm algorithm)
        {
            var command = new byte[] { 0x55, 0xAA, 0x11, 0x02, 0x2D, (byte)algorithm, 0x00, 0x00 };
            command[7] = (byte)(command.Aggregate(0x00, (a, b) => a + b) & 0xFF);
            _connection.Write(command);
            WaitForOK();
        }

        private void WaitForOK()
        {
            // COMMAND_RETURN_OK(0x2E):
            // HUSKYLENS will return OK, if HUSKYLENS receives COMMAND_REQUEST_ALGORITHM, COMMAND_REQUEST_KNOCK.
            var expected = new byte[] { 0x55, 0xAA, 0x11, 0x00, 0x2E, 0x3E };
            var response = _connection.Read(6);

            if (!response.ToArray().Zip(expected, (a, b) => a == b).All(a => a))
            {
                throw new Exception();
            }
        }
    }

    /// <summary>
    /// Algorithms for HuskyLens
    /// </summary>
    public enum Algorithm : byte
    {
        /// FACE_RECOGNITION
        FACE_RECOGNITION = 0x00,

        /// OBJECT_TRACKING
        OBJECT_TRACKING = 0x01,

        /// OBJECT_RECOGNITION
        OBJECT_RECOGNITION = 0x02,

        /// LINE_TRACKING
        LINE_TRACKING = 0x03,

        /// COLOR_RECOGNITION
        COLOR_RECOGNITION = 0x04,

        /// TAG_RECOGNITION
        TAG_RECOGNITION = 0x05,

        /// OBJECT_CLASSIFICATION
        OBJECT_CLASSIFICATION = 0x06,
    }
}

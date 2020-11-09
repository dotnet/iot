// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// todo
    /// </summary>
    public class HuskyLens
    {
        private readonly IBinaryConnection _connection;

        /// <summary>
        /// Algorithm currently used.
        /// </summary>
        public Algorithm Algorithm { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">guess</param>
        public HuskyLens(IBinaryConnection connection)
        {
            _connection = connection;
            Algorithm = Algorithm.Undefined;
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
            if (algorithm == Algorithm.Undefined)
            {
                throw new ArgumentException("Not supported", nameof(algorithm));
            }

            Algorithm = algorithm;
            var command = new byte[] { 0x55, 0xAA, 0x11, 0x02, 0x2D, (byte)algorithm, 0x00, 0x00 };
            command[7] = (byte)(command.Aggregate(0x00, (a, b) => a + b) & 0xFF);
            _connection.Write(command);
            WaitForOK();
        }

        /// <summary>
        /// Gets all recognized objects
        /// </summary>
        public IReadOnlyCollection<HuskyObject> GetAllObjects()
        {
            var command = new byte[] { 0x55, 0xAA, 0x11, 0x00, 0x20, 0x30 };
            _connection.Write(command);

            // wait for COMMAND_RETURN_INFO
            var response = new DataFrame(_connection.Read(0x0A + 6));
            if (!response.IsValid() || response.Command != Command.COMMAND_RETURN_INFO)
            {
                // error
                throw new NotImplementedException();
            }

            // read # of blocks&arrows
            var count = BinaryPrimitives.ReadInt16LittleEndian(response.Data.Slice(0, 2));
            Console.WriteLine($"Reading {count} objects");
            var huskyObjects = new List<HuskyObject>();
            for (int i = 0; i < count; i++)
            {
                response = new DataFrame(_connection.Read(0x0A + 6));
                if (!response.IsValid())
                {
                    // error
                    throw new NotImplementedException();
                }

                switch (response.Command)
                {
                    case Command.COMMAND_RETURN_BLOCK:
                        huskyObjects.Add(Block.FromData(response.Data));
                        break;
                    case Command.COMMAND_RETURN_ARROW:
                        huskyObjects.Add(Arrow.FromData(response.Data));
                        break;
                    default:
                        // error
                        throw new NotImplementedException();
                }
            }

            return huskyObjects;
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
}

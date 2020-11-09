// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// HuskyLens is an easy-to-use AI machine vision sensor with 6 built-in functions: face recognition, object tracking, object recognition, line following, color detection and tag detection.
    /// </summary>
    public class HuskyLens
    {
        private readonly IBinaryConnection _connection;
        private Algorithm _algorithm;

        /// <summary>
        /// Algorithm currently used.
        /// </summary>
        public Algorithm Algorithm
        {
            get => _algorithm;
            set
            {
                if (value == Algorithm.Undefined)
                {
                    throw new ArgumentException("Not supported", nameof(value));
                }

                // COMMAND_REQUEST_ALGORITHM(0x2D):
                // When HUSKYLENS receives this command, HUSKYLENS will change the algorithm by the Data. And will return COMMAND_RETURN_OK.
                // See: https://github.com/HuskyLens/HUSKYLENSArduino/blob/master/HUSKYLENS%20Protocol.md#command_request_algorithm0x2d
                var command = new byte[] { 0x55, 0xAA, 0x11, 0x02, 0x2D, (byte)value, 0x00, 0x00 };
                command[7] = command.CalculateChecksum();
                _connection.Write(command);
                WaitForOK();

                _algorithm = value;
            }
        }

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
        /// Test connection with HuskyLens.
        /// </summary>
        /// <returns>true for success, guess the rest</returns>
        public bool Ping()
        {
            // COMMAND_REQUEST_KNOCK(0x2C):
            // Used for test connection with HUSKYLENS.When HUSKYLENS received this command, HUSKYLENS will return COMMAND_RETURN_OK
            // See: https://github.com/HuskyLens/HUSKYLENSArduino/blob/master/HUSKYLENS%20Protocol.md#command_request_knock0x2c
            _connection.Write(new byte[] { 0x55, 0xAA, 0x11, 0x00, 0x2C, 0x3C });
            WaitForOK();
            return true;
        }

        /// <summary>
        /// Gets all recognized objects
        /// </summary>
        public IReadOnlyCollection<HuskyObject> GetAllObjects()
        {
            // COMMAND_REQUEST (0x20):
            // Request all blocks and arrows from HUSKYLENS.
            // See: https://github.com/HuskyLens/HUSKYLENSArduino/blob/master/HUSKYLENS%20Protocol.md#command_request-0x20
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
            var response = _connection.Read(6);

            if (!IsOk(response))
            {
                throw new Exception();
            }
        }

        private bool IsOk(ReadOnlySpan<byte> response)
        {
            // COMMAND_RETURN_OK(0x2E):
            // HUSKYLENS will return OK, if HUSKYLENS receives COMMAND_REQUEST_ALGORITHM, COMMAND_REQUEST_KNOCK.
            // See: https://github.com/HuskyLens/HUSKYLENSArduino/blob/master/HUSKYLENS%20Protocol.md#command_return_ok0x2e
            var expected = new byte[] { 0x55, 0xAA, 0x11, 0x00, 0x2E, 0x3E };

            if (response.Length != expected.Length)
            {
                return false;
            }

            for (int i = 0; i < response.Length; i++)
            {
                if (response[i] != expected[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

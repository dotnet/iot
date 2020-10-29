// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;

using RelayBoard;

namespace Iot.Device.RelayBoard.Samples
{
    /// <summary>
    /// Samples for RelayBoard
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            // Create a relay board, using default values
            var board = new RelayBoard();

            // Add a relay to the board
            board.CreateRelay(pin: 1);

            // Add a group of relays to the board
            board.CreateRelays(2, 3, 4);

            // Go through all the relays and set them to on
            foreach (var relay in board)
            {
                relay.On = true;
            }

            // Get a specific relay
            var r = board.GetRelay(1);
            r.On = false;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
            using var board = new RelayBoard();

            // Add a relay to the board
            board.CreateRelay(pin: 1);

            // Add a group of relays to the board
            board.CreateRelays(2, 3, 4);

            // Go through all the relays and set them to on
            foreach (Relay relay in board)
            {
                relay.On = true;
            }

            // Get a specific relay
            Relay? r = board.GetRelay(1);

            if (r != null)
            {
                r.On = false;
            }

            // Or, set it using the Set method
            board.Set(1, true);
        }
    }
}

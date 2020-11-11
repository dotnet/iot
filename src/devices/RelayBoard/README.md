# RelayBoard

## Summary
This is a basic class for controlling Relays. It also provides a convenient ``RelayBoard`` class that allows you to easily manage the different relay boards on your system.

## Device Family
This library should work with any basic relay that takes one input pin, including both normally closed and normally open relays.

## Binding Notes

You can either use the ``RelayBoard`` class to help organize multiple relays (especially if they are of the same type), or use the individual ``Relay`` class directly.

If you use ``Relay`` directly, you *must* manage the lifetime of the GPIO controller.

An example of how you would use a ``RelayBoard``:

```csharp
// Create a relay board, using default values
using var board = new RelayBoard();

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
```

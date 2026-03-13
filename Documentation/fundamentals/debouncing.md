# Signal Debouncing

When working with physical buttons, switches, and mechanical sensors, you'll encounter "bouncing" - rapid, unwanted signal transitions that can cause your code to register multiple events from a single button press. This guide explains the problem and various solutions.

## The Bouncing Problem

### What is Bouncing?

When you press or release a mechanical button/switch, the contacts physically bounce (vibrate) before settling into a stable state. This creates multiple HIGH/LOW transitions in a very short time (typically 5-50ms).

**What you expect:**

```
Button Press:   LOW ────┐
                        └──────── HIGH
```

**What actually happens:**

```
Button Press:   LOW ─┐─┐┐──┐
                     └┘└┘└──└─── HIGH (with bounces)
```

### Why It Matters

Without debouncing, a single button press can register as multiple presses:

```csharp
// Problematic code - counts multiple presses from one button push
int pressCount = 0;
controller.RegisterCallbackForPinValueChangedEvent(
    buttonPin,
    PinEventTypes.Falling,
    (sender, args) => pressCount++);

// User presses button once → pressCount might be 5, 10, or more!
```

## Software Debouncing Solutions

### 1. Simple Time-based Debouncing (Most Common)

Ignore subsequent events within a debounce period after the first event.

```csharp
using System.Device.Gpio;

const int buttonPin = 17;
const int debounceMs = 50; // 50ms debounce period

using GpioController controller = new();
controller.OpenPin(buttonPin, PinMode.InputPullUp);

DateTime lastPress = DateTime.MinValue;

controller.RegisterCallbackForPinValueChangedEvent(
    buttonPin,
    PinEventTypes.Falling,
    (sender, args) =>
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSinceLastPress = now - lastPress;

        if (timeSinceLastPress.TotalMilliseconds >= debounceMs)
        {
            lastPress = now;
            Console.WriteLine($"Button pressed at {now}");
            // Handle button press here
        }
        // Else: ignore bounce
    });
```

**Pros:** Simple, effective for most cases
**Cons:** May miss rapid intentional presses, requires tuning debounce time

### 2. State-based Debouncing with Confirmation

Wait for stable state confirmation before registering the event.

```csharp
using System.Device.Gpio;

const int buttonPin = 17;
const int stableMs = 20; // State must be stable for 20ms

using GpioController controller = new();
controller.OpenPin(buttonPin, PinMode.InputPullUp);

PinValue lastStableState = PinValue.High;
DateTime stateChangeTime = DateTime.UtcNow;

// Poll every 5ms
using Timer timer = new Timer(_ =>
{
    PinValue currentState = controller.Read(buttonPin);
    DateTime now = DateTime.UtcNow;

    if (currentState != lastStableState)
    {
        // State changed, reset timer
        stateChangeTime = now;
        lastStableState = currentState;
    }
    else
    {
        // State unchanged, check if stable long enough
        TimeSpan stableDuration = now - stateChangeTime;
        if (stableDuration.TotalMilliseconds >= stableMs)
        {
            // State is confirmed stable
            if (currentState == PinValue.Low) // Button pressed
            {
                Console.WriteLine("Button press confirmed");
                // Reset to prevent multiple triggers
                stateChangeTime = now.AddSeconds(1);
            }
        }
    }
}, null, 0, 5);

Console.ReadLine();
```

**Pros:** Most reliable, confirms stable state
**Cons:** More complex, requires periodic polling

### 3. Asynchronous Debouncing (Task-based)

Use async delays to debounce events.

```csharp
using System.Device.Gpio;

const int buttonPin = 17;
const int debounceMs = 50;

using GpioController controller = new();
controller.OpenPin(buttonPin, PinMode.InputPullUp);

bool isDebouncing = false;

controller.RegisterCallbackForPinValueChangedEvent(
    buttonPin,
    PinEventTypes.Falling,
    async (sender, args) =>
    {
        if (isDebouncing) return;

        isDebouncing = true;
        Console.WriteLine("Button pressed");

        // Handle button press here

        await Task.Delay(debounceMs);
        isDebouncing = false;
    });
```

**Pros:** Clean code, non-blocking
**Cons:** Requires async context, potential for timing issues under load

### 4. Debouncing with State Tracking

Track both press and release with proper debouncing.

```csharp
using System.Device.Gpio;

public class DebouncedButton
{
    private readonly GpioController _controller;
    private readonly int _pin;
    private readonly int _debounceMs;
    private DateTime _lastEventTime = DateTime.MinValue;
    private PinValue _lastStableValue;

    public event EventHandler<PinValue>? ValueChanged;

    public DebouncedButton(GpioController controller, int pin, int debounceMs = 50)
    {
        _controller = controller;
        _pin = pin;
        _debounceMs = debounceMs;

        _controller.OpenPin(_pin, PinMode.InputPullUp);
        _lastStableValue = _controller.Read(_pin);

        _controller.RegisterCallbackForPinValueChangedEvent(
            _pin,
            PinEventTypes.Rising | PinEventTypes.Falling,
            OnPinChanged);
    }

    private void OnPinChanged(object sender, PinValueChangedEventArgs args)
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSinceLastEvent = now - _lastEventTime;

        if (timeSinceLastEvent.TotalMilliseconds >= _debounceMs)
        {
            _lastEventTime = now;
            PinValue newValue = args.ChangeType == PinEventTypes.Rising 
                ? PinValue.High 
                : PinValue.Low;

            if (newValue != _lastStableValue)
            {
                _lastStableValue = newValue;
                ValueChanged?.Invoke(this, newValue);
            }
        }
    }

    public void Dispose()
    {
        _controller.ClosePin(_pin);
    }
}

// Usage:
using GpioController controller = new();
var button = new DebouncedButton(controller, 17, debounceMs: 50);

button.ValueChanged += (sender, value) =>
{
    if (value == PinValue.Low)
        Console.WriteLine("Button pressed");
    else
        Console.WriteLine("Button released");
};
```

**Pros:** Reusable, handles both press and release, encapsulated
**Cons:** More code, requires class instantiation

## Hardware Debouncing

Software debouncing is usually sufficient, but hardware solutions can provide better reliability.

### 1. RC Filter (Resistor-Capacitor)

Add a capacitor in parallel with the button:

```
          Button
GPIO ─────┐  ┌───── GND
          └──┘
           │
          ═╧═  Capacitor (0.1µF - 1µF)
           │
          GND
```

**Typical values:**

- 10kΩ pull-up resistor (built-in on Raspberry Pi)
- 0.1µF to 1µF capacitor
- Debounce time ≈ R × C (e.g., 10kΩ × 0.1µF = 1ms)

**Pros:** No software needed, works at hardware level
**Cons:** Requires physical components, may slow response time

### 2. Schmitt Trigger IC

Use a Schmitt trigger (like 74HC14) between button and GPIO:

```
          Button    74HC14
GPIO ─────┐  ┌────┐      ┌──── Input (clean)
          └──┘    │ ──── │
                  └──────┘
```

**Pros:** Very reliable, fast response
**Cons:** Requires additional IC, more complex circuit

### 3. Dedicated Debounce IC

Use specialized debounce ICs like MAX6816/MAX6817:

**Pros:** Professional solution, adjustable debounce time
**Cons:** Additional cost and complexity

## Choosing Debounce Time

### General Guidelines

| Application | Debounce Time | Reason |
|-------------|---------------|---------|
| Tactile buttons | 20-50ms | Typical bounce duration |
| Mechanical switches | 50-100ms | Longer bounce |
| Rotary encoders | 1-5ms | Fast transitions needed |
| Limit switches | 50-100ms | Heavy contacts, longer bounce |
| Reed switches | 10-20ms | Light contacts |

### Tuning Method

1. Start with 50ms (safe default)
2. If you miss fast button presses, decrease by 10ms
3. If you still get multiple triggers, increase by 10ms
4. Test with rapid button presses to find minimum reliable value

### Testing Code

```csharp
// Test your debounce timing
int eventCount = 0;
DateTime firstEvent = DateTime.MinValue;

controller.RegisterCallbackForPinValueChangedEvent(
    buttonPin,
    PinEventTypes.Falling,
    (sender, args) =>
    {
        if (eventCount == 0)
            firstEvent = DateTime.UtcNow;

        eventCount++;
        Console.WriteLine($"Event #{eventCount} at +{(DateTime.UtcNow - firstEvent).TotalMilliseconds:F1}ms");
    });

Console.WriteLine("Press button once and observe multiple events");
// Observe the time between events to set debounce time appropriately
```

## Advanced: Debouncing Multiple Buttons

```csharp
public class MultiButtonDebouncer
{
    private readonly GpioController _controller;
    private readonly Dictionary<int, ButtonState> _buttons = new();
    private readonly int _debounceMs;

    private class ButtonState
    {
        public DateTime LastEventTime { get; set; } = DateTime.MinValue;
        public PinValue LastValue { get; set; }
        public Action<PinValue>? Callback { get; set; }
    }

    public MultiButtonDebouncer(GpioController controller, int debounceMs = 50)
    {
        _controller = controller;
        _debounceMs = debounceMs;
    }

    public void RegisterButton(int pin, Action<PinValue> callback)
    {
        _controller.OpenPin(pin, PinMode.InputPullUp);
        var state = new ButtonState
        {
            LastValue = _controller.Read(pin),
            Callback = callback
        };
        _buttons[pin] = state;

        _controller.RegisterCallbackForPinValueChangedEvent(
            pin,
            PinEventTypes.Rising | PinEventTypes.Falling,
            (sender, args) => OnPinChanged(pin, args));
    }

    private void OnPinChanged(int pin, PinValueChangedEventArgs args)
    {
        if (!_buttons.TryGetValue(pin, out var state)) return;

        DateTime now = DateTime.UtcNow;
        TimeSpan timeSinceLast = now - state.LastEventTime;

        if (timeSinceLast.TotalMilliseconds >= _debounceMs)
        {
            state.LastEventTime = now;
            PinValue newValue = args.ChangeType == PinEventTypes.Rising 
                ? PinValue.High 
                : PinValue.Low;

            if (newValue != state.LastValue)
            {
                state.LastValue = newValue;
                state.Callback?.Invoke(newValue);
            }
        }
    }
}

// Usage:
using GpioController controller = new();
var debouncer = new MultiButtonDebouncer(controller, debounceMs: 50);

debouncer.RegisterButton(17, value => 
    Console.WriteLine($"Button 1: {value}"));
debouncer.RegisterButton(27, value => 
    Console.WriteLine($"Button 2: {value}"));
debouncer.RegisterButton(22, value => 
    Console.WriteLine($"Button 3: {value}"));
```

## Rotary Encoder Debouncing

Rotary encoders require special handling due to fast state changes:

```csharp
public class DebouncedRotaryEncoder
{
    private readonly GpioController _controller;
    private readonly int _pinA;
    private readonly int _pinB;
    private int _lastEncoded = 0;
    private long _encoderValue = 0;
    private DateTime _lastChangeTime = DateTime.MinValue;
    private const int DebounceUs = 1000; // 1ms for encoders

    public event EventHandler<int>? ValueChanged;

    public DebouncedRotaryEncoder(GpioController controller, int pinA, int pinB)
    {
        _controller = controller;
        _pinA = pinA;
        _pinB = pinB;

        _controller.OpenPin(_pinA, PinMode.InputPullUp);
        _controller.OpenPin(_pinB, PinMode.InputPullUp);

        _controller.RegisterCallbackForPinValueChangedEvent(_pinA, 
            PinEventTypes.Rising | PinEventTypes.Falling, OnPinChanged);
        _controller.RegisterCallbackForPinValueChangedEvent(_pinB, 
            PinEventTypes.Rising | PinEventTypes.Falling, OnPinChanged);
    }

    private void OnPinChanged(object sender, PinValueChangedEventArgs args)
    {
        DateTime now = DateTime.UtcNow;
        if ((now - _lastChangeTime).TotalMilliseconds < DebounceUs / 1000.0)
            return;

        _lastChangeTime = now;

        int MSB = _controller.Read(_pinA) == PinValue.High ? 1 : 0;
        int LSB = _controller.Read(_pinB) == PinValue.High ? 1 : 0;
        int encoded = (MSB << 1) | LSB;
        int sum = (_lastEncoded << 2) | encoded;

        if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011)
        {
            _encoderValue++;
            ValueChanged?.Invoke(this, 1);
        }
        else if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000)
        {
            _encoderValue--;
            ValueChanged?.Invoke(this, -1);
        }

        _lastEncoded = encoded;
    }
}
```

## Best Practices

1. **Start with software debouncing** - It's flexible and works for most cases
2. **Use 50ms as default** - Works for most buttons and switches
3. **Test with real hardware** - Simulated bouncing doesn't match reality
4. **Consider hardware debouncing** - For production or critical applications
5. **Log events during development** - Helps tune debounce timing
6. **Handle both edges** - Track press and release if needed
7. **Encapsulate debouncing logic** - Makes code cleaner and reusable

## Common Mistakes to Avoid

**No debouncing at all**

```csharp
// BAD: Will trigger multiple times per press
controller.RegisterCallbackForPinValueChangedEvent(pin, 
    PinEventTypes.Falling, (s, e) => HandlePress());
```

**Debounce time too short**

```csharp
// BAD: 5ms may be too short for mechanical buttons
const int debounceMs = 5; // Still getting bounces!
```

**Blocking delays in callback**

```csharp
// BAD: Blocks callback thread
controller.RegisterCallbackForPinValueChangedEvent(pin, 
    PinEventTypes.Falling, (s, e) => 
    {
        Thread.Sleep(50); // DON'T DO THIS
        HandlePress();
    });
```

**Ignoring edge types**

```csharp
// BAD: Might need both Rising and Falling
controller.RegisterCallbackForPinValueChangedEvent(pin, 
    PinEventTypes.Falling, callback); // What about release?
```

## Next Steps

- [GPIO Basics](gpio-basics.md) - Learn more about GPIO input/output
- [Understanding Protocols](understanding-protocols.md) - Explore other communication methods
- [Troubleshooting](../troubleshooting.md) - Solve common GPIO issues

## Additional Resources

- [Button Debouncing Tutorial - Ganssle](http://www.ganssle.com/debouncing.htm)
- [Debouncing Tutorial - Arduino](https://www.arduino.cc/en/Tutorial/BuiltInExamples/Debounce)
- [Understanding Switch Bounce](https://my.eng.utah.edu/~cs5780/debouncing.pdf)

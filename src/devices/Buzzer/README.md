# Buzzer - Piezo Buzzer Controller

## Summary

This device binding allows playing certain tones using piezo buzzer. It uses PWM with 50% duty cycle and various frequencies.

Piezo buzzers with three pins supported as well as piezo buzzers with two pins.

## Device Family

This binding was tested on two types of piezo buzzers. First type of buzzer has two pins *vcc* and *gnd*. Second type of buzzers has addition *signal* pin.

## Binding Notes

The  `Buzzer`  class can use either software or hardware PWM. This is done fully transparently by the initialization.

If you want to use the software PWM, you have to specify the GPIO pin you want to use as the first parameter in the constructor. Use the value -1 for the second one. This will force usage of the software PWM as it is not a valid value for hardware PWM.

To use the hardware PWM, make sure you reference correctly the chip and channel you want to use. The  `Buzzer`  class will always try first to open a hardware PWM then a software PWM.

Also you could explicitly pass PWM controller as third parameter of an appropriate constructor.

Here's an example how you could use `Buzzer`.
```csharp
using (Buzzer buzzer = new Buzzer(21, -1)); // Initialize buzzer with software PWM connected to pin 21.
{
	buzzer.PlayTone(440, 1000); // Play tone with frequency 440 hertz for one second.
}
```
`Buzzer` allows to play tone for certain duration like in example above.
Or you could start tone playing, perform some operation and then stop tone playing like in a following example.
```csharp
using (Buzzer buzzer = new Buzzer(21, -1));
{
	buzzer.SetFrequency(440);
	Thread.Sleep(1000);
	buzzer.StopPlaying();
}
```
The result will be the same as in previous example.

`Buzzer` allows you to play only single tone at a single moment. If you will call `SetFrequency` sequentially with a different frequencies then the last call will override previous calls. Following example explains it.
```csharp
using (Buzzer buzzer = new Buzzer(21, -1)); // Initialize buzzer with software PWM connected to pin 21.
{
	buzzer.SetFrequency(440);
	Thread.Sleep(1000);
	buzzer.SetFrequency(880);
	Thread.Sleep(1000);
	buzzer.StopPlaying();
}
```
This example will play tone with frequency 440 for a second and then will play tone with a frequency 880 for a second.
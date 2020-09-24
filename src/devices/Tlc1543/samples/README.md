# Simple Line finding algorithm with ADC usage

This sample shows how to calculate position of line under matrix of 5 IR sensors with the help of TLC1543 ADC. In this case it's AlphaBot2 Pi from WaveShare with ITR-20001 IR sensors.

## Initialization

```c#
Tlc1543 adc = new Tlc1543(24, 5, 23, 25);
```

- 24 is our address pin, 

- 5 chip select, 

- 23 data out pin 

- and 25 is input output clock.

## Changing Charge Channel

```c#
adc.ChargeChannel = Channel.SelfTest512;
```

You can set ChargeChannel to one of the Self Test channels (if you really know what you're doing you can also set it to one of the normal channels - but remember that this may amplify noise going from one channel to the other) if you know exactly what range of values you're expecting on polled channels (less interference on channels with fast changing but weak signals). Default set for SelfTest512 as that's the middle of 10 bit range.

## Getting data

```c#
List<int> values = adc.ReadChannels(channelList);
```

Simple way of reading values into just made list. 

## Calculating position

```c#
for (int i = 0; i < values.Count; i++)
{
    if (values[i] < 300)
    {
        lineAverage += (i - 2);
        onLine++;
    }
}
```

To find where under our matrix is line we need to check values given out from sensors depending on where they are pointing. 

- Pointing at sky gives us values ranging from 0 to 50

- Keeping it close to white paper gives values bigger than 600

- Placing it pointed at black tape - taped on white paper(for contrast) gives values ranging from 150 to 250 on sensor exactly above line

So to simplfy things we assume that when we run this program we won't be pointing our sensor matrix at the sky and we will keep it at a flat surface.

Next thing is to calculate average from sensors for when there are more than one sensors seeing line.

For example when first and second sensor sees line values will be

- on first loop `lineAverage = -2` and `onLine = 1`;

- on second loop `lineAverage = -3` and `onLine = 2`;

And now with this data we can calculate linePosition

```c#
double linePosition = ((double)lineAverage / (double)onLine);
```

`linePosition = -1.5`

and we now know that the line is somewhere between first and second sensor. By using `(i - 2)` in our loop we moved values that the third sensor now indicates middle of the matrix, values less than zero are left from the middle and values above are on the right side. If we would have seven sensor matrix we would move those values by 3. Nine sensors? Move it by 4 - simple as that.

- 

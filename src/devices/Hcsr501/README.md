# HC-SR501
HC-SR501 is used to detect motion based on the infrared heat in the surrounding area. 

## Sensor Image
![](sensor.jpg)

## Usage
```C#
using(Hcsr501 sensor = new Hcsr501(hcsr501Pin, PinNumberingScheme.Logical))
{
    bool isDetected = sensor.IsMotionDetected;
    // TODO
}
```

## References
In Chinese : http://wenku.baidu.com/view/26ef5a9c49649b6648d747b2.html

In English : https://cdn.datasheetspdf.com/pdf-down/H/C/-/HC-SR501-1-ETC.pdf

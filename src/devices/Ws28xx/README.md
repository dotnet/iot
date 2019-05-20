# Ws28xx LED drivers

## Summary

This binding allows you to update the RGB LEDs on Ws28xx and based strips and matrices.

To see how to use the binding in code, see the [sample](samples/README.md).

## Device Family

* WS2812B: [Datasheet](https://cdn-shop.adafruit.com/datasheets/WS2812B.pdf)
* WS2808: [Datasheet](https://datasheetspdf.com/pdf-file/806051/Worldsemi/WS2801/1)

## Binding Notes

### Raspberry Pi setup (/boot/config.txt)

* Make sure spi is enabled<br>
  `dtparam=spi=on`

* Make sure SPI don't change speed fix the core clock<br>
  `core_freq=250`<br>
  `core_freq_min=250`

## References 

* [Neo pixels guide](https://learn.adafruit.com/adafruit-neopixel-uberguide)
* [Neo pixels x8 stick](https://www.adafruit.com/product/1426)

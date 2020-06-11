# Memory LCD

The Memory LCD combines matrix technology with a one-bit memory circuit embedded into every pixel, so information is retained once it's written.

The Memory LCD features simple 3-wire Serial I/F connectivity (SI, SCS, SCLK).

Model       | Size  | Resolution | FPS | VDD           | SPI
------------|-------|------------|-----|---------------|-------
LS013B7DH03 | 1.28" | 128x128    | 65  | 3V (3.3V max) | 1.1MHz
LS013B7DH05 | 1.26" | 144x168    | 60  | 3V (3.3V max) | 1.1MHz
LS027B7DH01 | 2.7"  | 400x240    | 20  | 5V (5.5V max) | 2MHz

* Interface is all 3V Serial

# Notice

Due to the capacity limitation of the SPI output buffer, a buffer overflow exception may occur when update the screen with a high pixel count (eg. LS027B7DH01 with Raspberry Pi). To avoid this problem, you should limit the amount of lines and split one update into multiple updates.

# Datasheet

[LS013B7DH03](https://media.digikey.com/pdf/Data%20Sheets/Sharp%20PDFs/LS013B7DH03_Spec.pdf)

[LS013B7DH05](https://media.digikey.com/pdf/Data%20Sheets/Sharp%20PDFs/LS013B7DH05.pdf)

[LS027B7DH01](https://media.digikey.com/pdf/Data%20Sheets/Sharp%20PDFs/LS027B7DH01_Rev_Jun_2010.pdf)

# Reference

https://www.sharpsma.com/sharp-memory-lcd-technology

https://www.sharpsma.com/products?sharpCategory=Memory%20LCD

https://www.sharpsma.com/documents/1468207/1565928/Sharp+Memory+LCD+2016+Brochure

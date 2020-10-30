namespace Iot.Device.Arduino
{
    internal enum FirmataSpiCommand
    {
        SPI_BEGIN = 0x0,
        SPI_DEVICE_CONFIG = 0x01,
        SPI_TRANSFER = 0x02,
        SPI_WRITE = 0x03,
        SPI_READ = 0x04,
        SPI_REPLY = 0x05,
        SPI_END = 0x06,
    }
}

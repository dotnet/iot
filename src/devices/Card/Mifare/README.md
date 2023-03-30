# Mifare card - RFID Card

This class supports Mifare cards. They are RFID cards responding to ISO 14443 type A. You need a specific card reader like PN532, PN5180 to read, write those kind of cards.

## Usage

You'll need first to get the card from an RFID reader. The example below shows how to do it with a PN532 and read all the sectors and print all the sector data information.

```csharp
using System.Device.I2c;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Iot.Device.Card.Mifare;

const int I2cBus = 1;

I2cConnectionSettings i2cSettings = new(I2cBus, Pn532.I2cDefaultAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Pn532 pn532 = new(i2cDevice, true);

byte[]? retData = null;
while (!Console.KeyAvailable)
{
    retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
    if (retData is object)
        break;
    // Give time to PN532 to process
    Thread.Sleep(200);
}

if (retData is null)
{
    return;
}

var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
if (decrypted is object)
{
    Console.Write($"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
    if (decrypted.Ats is object)
    {
        Console.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
    }
    else
    {
        Console.WriteLine();
    }

    MifareCard mifareCard = new MifareCard(pn532, decrypted.TargetNumber) { ReselectAfterError = true };
    mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
    mifareCard.SerialNumber = decrypted.NfcId;
    mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    for (byte block = 0; block < 64; block++)
    {
        mifareCard.BlockNumber = block;
        mifareCard.Command = MifareCardCommand.AuthenticationB;
        var ret = mifareCard.RunMifareCardCommand();
        if (ret < 0)
        {
            // Try another one
            mifareCard.Command = MifareCardCommand.AuthenticationA;
            ret = mifareCard.RunMifareCardCommand();
        }

        if (ret >= 0)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.Read16Bytes;
            ret = mifareCard.RunMifareCardCommand();
            if (ret >= 0)
            {
                Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
            }
            else
            {
                Console.WriteLine($"Error reading bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
            }

            if (block % 4 == 3)
            {
                // Check what are the permissions
                for (byte j = 3; j > 0; j--)
                {
                    var access = mifareCard.BlockAccess((byte)(block - j), mifareCard.Data);
                    Console.WriteLine($"Bloc: {block - j}, Access: {access}");
                }
                var sector = mifareCard.SectorTailerAccess(block, mifareCard.Data);
                Console.WriteLine($"Bloc: {block}, Access: {sector}");
            }
        }
        else
        {
            Console.WriteLine($"Authentication error");
        }
    }
}
```

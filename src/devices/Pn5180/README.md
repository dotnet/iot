# PN5180 - RFID and NFC reader

PN5180 is a RFID and NFC reader. It does supports various standards: ISO/IEC 14443 TypeA, ISO/IEC 14443 TypeB, ISO/IEC 15693 and ISO/IEC 18000-3 Mode 3. It does supports up to 848 kBit/s communication with 14443 type A cards.

## Documentation

Official documentation can be fond [here](https://www.nxp.com/docs/en/data-sheet/PN5180A0XX_C3_C4.pdf)

Application note on how to operate PN5180 without a [library](https://www.nxp.com/docs/en/application-note/AN12650.pdf)

## Board

You will find different implementation of this board. All boards should have full SPI pins plus the reset and busy ones and additionnaly 5V and or 3.3V plus ground. This pictures shows an example of one of the implementation connected with a [FT4222](../FT4222/README.md) chipset to provide the necessary GPIO and SPI features:

![PN5180](./pn5180_ft4222.png)

## Usage

You will find a full example in the [samples directory](./samples/Program.cs). This example covers the usage of most of the public functions and properties. This example shows as well how to use the [FT4222](../FT4222/README.md) as a SPI and GPIO controller. Note that the development for the PN5180 has been done fully on a Windows 10 64 bit machine using this FT42222 to add the IoT capabilities.

PN5180 is operated thru SPI and GPIO. GPIO is used to control the SPI behavior as the PN5180 is using SPI in specific way. This does then require to manually manage the pin selection for SPI. And another pin called pin busy is used to understand when the PN5180 is available to receive and send information.

The following code shows how to create a SPI driver, reset the PN5180 and create the class.

```csharp
var spi = SpiDevice.Create(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.SpiClockFrequency, Mode = Pn5180.SpiMode, DataFlow = DataFlow.MsbFirst });

// Reset the device
var gpioController = new GpioController();
gpioController.OpenPin(4, PinMode.Output);
gpioController.Write(4, PinValue.Low);
Thread.Sleep(10);
gpioController.Write(4, PinValue.High);
Thread.Sleep(10);

var pn5180 = new Pn5180(spi, 2, 3);
```

You will note that the SPI maximum clock frenquency is preset with ```Pn5180.MaximumSpiClockFrequency```, the maximum operation frequency is 7MHz. Same for the mode thru ```Pn5180.DefaultSpiMode```. Data Flow has to be ```DataFlow.MsbFirst```.

In the previous example the pin 2 is used for busy and the pin 3 for the SPI selection. Note that you have to use a specific pin selection and cannot use the one which is associate with the SPI channel you create.

Reset is done thru pin 4. It is recommended to reset the board before creating the class.

Once created, you then need to select a card before you can actually exchange data with the card. Here is how to do it for an ISO 14443 Type A card:

```csharp
Data106kbpsTypeA cardTypeA;
do
{
    // This will try to select the card for 1 second and will wait 300 milliseconds before trying again if none is found
    var retok = _pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 1000);
    if (retok)
    {
        Console.WriteLine($"ISO 14443 Type A found:");
        Console.WriteLine($"  ATQA: {cardTypeA.Atqa}");
        Console.WriteLine($"  SAK: {cardTypeA.Sak}");
        Console.WriteLine($"  UID: {BitConverter.ToString(cardTypeA.NfcId)}");
        // This is where you do something with the card
    }
    else
    {
        Thread.Sleep(300);
    }
}
while (!Console.KeyAvailable);
```

And for an ISO 14443 Type B card:

```csharp
Data106kbpsTypeB card;

do
{
    // This will try to select the card for 1 second, if no card detected wait for 300 milliseconds and try again
    retok = _pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out card, 1000);
    if (!retok)
    {
        Thread.Sleep(300);
        continue;
    }

    Console.WriteLine($"ISO 14443 Type B found:");
    Console.WriteLine($"  Target number: {card.TargetNumber}");
    Console.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
    Console.WriteLine($"  App type: {card.ApplicationType}");
    Console.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
    Console.WriteLine($"  Bit rates: {card.BitRates}");
    Console.WriteLine($"  Cid support: {card.CidSupported}");
    Console.WriteLine($"  Command: {card.Command}");
    Console.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
    Console.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
    Console.WriteLine($"  Max frame size: {card.MaxFrameSize}");
    Console.WriteLine($"  Nad support: {card.NadSupported}");

    // Do something else, all operations you want with the card
    // Halt card
    if (_pn5180.DeselecCardTypeB(card))
    {
        Console.WriteLine($"Card unselected properly");
    }
    else
    {
        Console.WriteLine($"ERROR: Card can't be unselected");
    }
}
while (!Console.KeyAvailable);
```

And for an ISO 15693 card:

```csharp
if (pn5180.ListenToCardIso15693(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26, out IList<Data26_53kbps>? cards, 20000))
{
    pn5180.ResetPN5180Configuration(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26);
    foreach (Data26_53kbps card in cards)
    {
        Console.WriteLine($"Target number: {card.TargetNumber}");
        Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
        Console.WriteLine($"DSFID: {card.Dsfid}");
    }
}
```

Please note that the ```ListenToCardIso14443TypeA```, ```ListenToCardIso14443TypeB``` and ```ListenToCardIso15693``` can be configured with different transceiver and receiver configurations. Usually the configuration need to match but you can adjust and change them. See the section with Radio Frequency configuration for more information.

A card will be continuously tried to be detected during the duration on your polling. If nothing is detected or if any issue, the function will return false.

Specific for type B cards, they have a target number. This target number is needed to transceive any information with the card. The PN5180 can support up to 14 cards at the same time. But you can only select 1 card at a time, so if you have a need for multiple card selected at the same time, it is recommended to chain this card detection with the number of cards you need to select and operate at the same time. Note that depending on the card, they may not been seen as still selected by the reader.

You should deselect the Type B card at the end to release the target number. If not done, during the next poll, this implementation will test if the card is still present, keep it in this case.

For 15693 cards, the PN5180 can support up to 16 cards at the same time. ```ListenToCardIso15693``` can inventory multiple card at a time.

## EEPROM

You can fully access the PN5180 EEPROM. Here is an example on how to do it:

```csharp
// Maximum size of the EEPROM
Span<byte> eeprom = stackalloc byte[255];
// This will read fully the EEPROM
var ret = _pn5180.ReadAllEeprom(eeprom);
Console.WriteLine($"EEPROM dump: success: {ret}, Data: {BitConverter.ToString(eeprom.ToArray())}");
// This reads only the unique Identifier
ret = _pn5180.ReadEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 16));
Console.WriteLine($"EEPROM read, unique identifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
// Same as above
ret = _pn5180.GetIdentifier(eeprom.Slice(0, 16));
// So you should see the exact same result than from reading manully the 16 bytes of the unique identifier
Console.WriteLine($"GetIdentifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
// This tries to write in a read only part of the EEPROM
ret = _pn5180.WriteEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 1));
// So you'll receive false as an answer from the PN5180
Console.WriteLine($"Trying to write a read only EEPROM, this should return false: {ret}");
// This is important to understand, if you write in the EEPROM and then try to read right after,
// in most of the cases, the value won't change. After a reboot, you'll get the new value
Console.WriteLine($"EEPROM writing will not be immediate. Some are only active after a reboot");
Console.WriteLine($"changing second byte of UUID when acting as a card (first is always fix to 0x08)");
ret = _pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
eeprom[0]++;
Console.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
Console.WriteLine($"New value to write: {BitConverter.ToString(eeprom.Slice(0, 1).ToArray())}");
ret = _pn5180.WriteEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
Console.WriteLine($"Wrote IRQ_PIN_CONFIG: {ret}");
ret = _pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
Console.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
```

Functions has been implemented to read and write part or all the EEPROM. You need to be careful of the size of the buffer, it can't exceed 255 bytes and can't be larger than the base address you want to write and total size. So if you write at position 250, your buffer size and only be 5 maximum.

## PN5180 versions

You can retreive the PN5180 version thru the ```GetVersion``` function. 3 versions will be returned, the product, firmware and EEPROM ones.

```csharp
var (product, firmware, eeprom) = _pn5180.GetVersion();
Console.WriteLine($"Product: {product.ToString()}, Firmware: {firmware.ToString()}, EEPROM: {eeprom.ToString()}");
```

You should see something like this:

```text
Product: 3.5, Firmware: 3.5, EEPROM: 145.0
```

Current firmware versions are 3.12 (3.C) and 4.0. That said, this implementation supports older firmware. Newer firmware have better support for auto calibration, fixes bugs and added specific EMVco (payment) low level features. Note that the product version is the original firmware version installed. so if you've done firmware upgrade, the product version will always remain the one from the original firmware.

Note that this implementation does not support firmware update. You should use NXP tools if you want to update the firmware

## Radio Frequency Configuration

The PN5180 offers the possibility to set a lot of configurations. The good news is that those configurations are stored and can be loaded. You can adjust them as well. The following code shows an example on how to load, extract the configuration and with the same way, you can write back a configuration if you need. Please refer to the documentation in this case to understand the changes you want to make:

```csharp
// Number of configuration
var sizeConfig = _pn5180.GetRadioFrequencyConfigSize(TransmitterRadioFrequencyConfiguration.Iso14443B_106);
// The RadioFrequencyConfiguraitonSize is 5, 1 for the register and 4 for the register data
Span<byte> configBuff = stackalloc byte[Pn5180.RadioFrequencyConfiguraitonSize * sizeConfig];
var ret = _pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
for (int i = 0; i < sizeConfig; i++)
{
    Console.WriteLine($"Register: {configBuff[Pn5180.RadioFrequencyConfiguraitonSize * i]}, Data: {BitConverter.ToString(configBuff.Slice(Pn5180.RadioFrequencyConfiguraitonSize * i + 1, Pn5180.RadioFrequencyConfiguraitonSize - 1).ToArray())}");
}
```

Every configuration has the size of 5 bytes, first byte is the register number, and the next 4 are the data them selves.

## Transceive data with a card

Once the card is selected properly, you can use the CardTranscive class to exchange data with the card. See [Mifare](../Card/Mifare/README.md) and [Credit Card](../Card/CreditCard/README.md) for detailed examples.

This shows how to dump a Mifare (ISO 14443 type A) card fully:

```csharp
Data106kbpsTypeA cardTypeA;

// Let's pull for 20 seconds and see the result
var retok = _pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 20000);
Console.WriteLine();

if (!retok)
{
    Console.WriteLine("Can't read properly the card");
}
else
{
    Console.WriteLine($"ATQA: {cardTypeA.Atqa}");
    Console.WriteLine($"SAK: {cardTypeA.Sak}");
    Console.WriteLine($"UID: {BitConverter.ToString(cardTypeA.NfcId)}");

    MifareCard mifareCard = new MifareCard(_pn5180, cardTypeA.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
    mifareCard.SetCapacity(cardTypeA.Atqa, cardTypeA.Sak);
    mifareCard.SerialNumber = cardTypeA.NfcId;
    mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    for (byte block = 0; block < 64; block++)
    {
        mifareCard.BlockNumber = block;
        mifareCard.Command = MifareCardCommand.AuthenticationB;
        var ret = mifareCard.RunMifiCardCommand();
        if (ret < 0)
        {
            // Try another one
            mifareCard.Command = MifareCardCommand.AuthenticationA;
            ret = mifareCard.RunMifiCardCommand();
        }

        if (ret >= 0)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.Read16Bytes;
            ret = mifareCard.RunMifiCardCommand();
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

This shows how to read and write ICODE SLIX (ISO 15693) card fully:

```csharp
if (pn5180.ListenToCardIso15693(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26, out IList<Data26_53kbps>? cards, 20000))
{
    pn5180.ResetPN5180Configuration(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26);
    foreach (Data26_53kbps card in cards)
    {
        Console.WriteLine($"Target number: {card.TargetNumber}");
        Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
        Console.WriteLine($"DSFID: {card.Dsfid}");
        if (card.NfcId[6] == 0x04)
        {
            IcodeCard icodeCard = new IcodeCard(pn5180, card.TargetNumber)
            {
                Uid = card.NfcId,
                Capacity = IcodeCardCapacity.IcodeSlix,
            };

            icodeCard.GetSystemInformation();
            Console.WriteLine($"SystemInfo data is :{BitConverter.ToString(icodeCard.Data)}");
            icodeCard.Data = new byte[] { 0x1c, 0x1b, 0x1b, 0x1b };
            // icodeCard.LockBlock(27);
            icodeCard.WriteSingleBlock(2);
            Console.WriteLine($"write data response is :{BitConverter.ToString(icodeCard.Data)}");
            icodeCard.ReadMultipleBlocks(0, 3);
            Console.WriteLine($"block 0~3 data is :{BitConverter.ToString(icodeCard.Data)}");
            for (byte i = 0; i < 28; i++)
            {
                if (icodeCard.ReadSingleBlock(i))
                {
                    Console.WriteLine($"Block {i} data is :{BitConverter.ToString(icodeCard.Data)}");
                }
                else
                {
                    icodeCard.Data = new byte[] { };
                }
            }
        }
        else
        {
            Console.WriteLine("Only Icode cards are supported");
        }
    }
}
```

The [example](./samples/Program.cs) contains as well an implementation to fully dump the content of a credit card.

## Current implementation

Communication support:

- [X] Hardware SPI Controller fully supported
- [X] GPIO Controller fully supported

Miscellaneous

- [X] Read fully EEPROM
- [X] Write fully EEPROM
- [X] Read any part of EEPROM
- [X] Write any part of EEPROM
- [X] Get product, hardware and firmware versions
- [X] CardTransceive support to reuse existing [Mifare](../Card/Mifare/README.md) and [Credit Card](../Card/CreditCard/README.md), ISO 14443 support Type A or Type B protocol
- [ ] Secure firmware update
- [ ] Own board GPIO access

RF communication commands:

- [X] Load a specific configuration
- [X] Read a specific configuration
- [X] Write a specific configuration

PN5180 as an initiator (reader) commands:

- [X] Auto poll ISO 14443 type A cards
- [X] Auto poll ISO 14443 type B cards
- [X] Auto poll ISO 15693 cards
- [X] Deselect ISO 14443 type B cards
- [X] Multi card support at the same time: partial, depending on the card, CID mandatory in all 14443 type B communications
- [X] ISO 14443-4 communication protocol
- [ ] Auto poll ISO/IEC 18000-3 cards
- [ ] Communication support for ISO/IEC 18000-3 cards
- [ ] Low power card detection
- [X] Mifare specific authentication
- [ ] Fast 212, 424, 848 kbtis communication: partial

PN5180 as a Target (acting like a card)

- [ ] Initialization as target
- [ ] Handling communication with another reader as a target
- [ ] Support for transceive data

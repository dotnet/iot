# PN532 - RFID and NFC reader

PN532 is a RFID and NFC reader. It does supports various standards: IsoIec14443TypeA, IsoIec14443TypeB, Iso18092. This implementation should support as well PN533 which is a full ASB serial only implementation and have few more registers and functions but looks retro compatible with this implentation.

## Documentation

Official documentation can be fond here: https://www.nxp.com/docs/en/user-guide/141520.pdf

## USage

You first need to create the class thru I2C, SPI or Serial.

```csharp
// Embedded serial port on Raspberry Pi
string device = "/dev/ttyS0";
// or like this on Windows:
// string device = "COM7";
pn532 = new Pn532(device);
```

To act as a crad reader, the PN532 has to be in listening mode. 2 options are available, either thru using the ```ListPassiveTarget``` either the ```AutoPoll``` functions.

Example with polling a simple passive 14443 type A card like a Mifare:

```csharp
byte[] retData = null;
while ((!Console.KeyAvailable))
{
	retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
	if (retData != null)
		break;
	// Give time to PN532 to process
	Thread.Sleep(200);
}
if (retData == null)
	return;
var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
```

Example pooling a 14443 type B card like a credit card:

```csharp
 byte[] retData = null;
while ((!Console.KeyAvailable))
{
	// PollingType.InnovisionJewel, PollingType.DepActive106kbps, PollingType.DepPassive106kbps,PollingType.Passive106kbpsISO144443_4A
	retData = pn532.AutoPoll(2, 300, new PollingType[] { PollingType.GenericPassive106kbps, PollingType.MifareCard, PollingType.Passive106kbps, PollingType.Passive106kbpsISO144443_4B });
	if (retData != null)
		break;
	// Give time to PN532 to process
	Thread.Sleep(200);
}
if (retData == null)
	return;
//Check how many tags and the type
Console.WriteLine($"Num tags: {retData[0]}, Type: {(PollingType)retData[1]}");
var decrypted = pn532.TryDecodeData106kbpsTypeB(retData.AsSpan().Slice(3));
```

## Reading or writing to cards

PN532 implement a ReadWrite function that allows to use a high level Mifare card class. This implementation abstract the reader which is used.

Once detected and selected like in the previous example, this fully dump the content of a classical Mifare 1K card:

```csharp
if (decrypted != null)
{
	Console.WriteLine($"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
	if (decrypted.Ats != null)
		Console.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
	MifareCard mifareCard = new MifareCard(pn532, decrypted.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
	mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
	mifareCard.SerialNumber = decrypted.NfcId;
	mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
	mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };/
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
				Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
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
			Console.WriteLine($"Autentication error");

		}
	}
```

## Current implementation

Communication support:
- [X] HSU serial port: fully supported
- [X] I2C: supported
- [ ] SPI: experimental, using a specific chip select pin as well as using LSB with reverse bytes function rather than built in function. This is due to current limitation in the SPI implementations
- [ ] Hardware reset pin

Miscellaneous commands:
- [X] Diagnose. Note: partial implementation, basics tests implemented only
- [X] GetFirmwareVersion 
- [X] GetGeneralStatus 
- [X] ReadRegister 
- [X] WriteRegister
- [X] ReadGPIO
- [X] WriteGPIO
- [X] SetSerialBaudRate
- [X] SetParameters
- [X] SAMConfiguration
- [X] PowerDown

RF communication commands:
- [X] RFConfiguration
- [ ] RFRegulationTest

PN532 as an initiator (reader) commands:
- [ ] InJumpForDEP
- [ ] InJumpForPSL
- [X] InListPassiveTarget
- [ ] InATR
- [ ] InPSL
- [X] InDataExchange
- [X] InCommunicateThru
- [X] InDeselect
- [X] InRelease
- [X] InSelect
- [X] InAutoPoll 
  
PN532 as a Target (acting like a card)
- [ ] TgInitAsTarget
- [ ] TgSetGeneralBytes
- [ ] TgGetData
- [ ] TgSetData
- [ ] TgSetMetaData
- [ ] TgGetInitiatorCommand
- [ ] TgResponseToInitiator
- [ ] TgGetTargetStatus 
 
 
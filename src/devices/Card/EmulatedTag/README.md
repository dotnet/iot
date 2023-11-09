# Emulated card NDEF Tag

This class supports NDEF card emulation. So far, the only reader supported is the PN532.

## Documentation

You can find useful documentation on:

* [NFC Forum](https://nfc-forum.org/uploads/specifications/97-NFCForum-TS-T4T-1.2.pdf) for the high level communication protocol and payloads.
* [ST25TA64K](https://www.st.com/resource/en/datasheet/st25ta64k.pdf) as a good implementation detail from the card perspective.

## Usage

When you have created a PN52, you can use directly 

```csharp
EmulatedNdefTag ndef = new(pn532, new byte[] { 0x12, 0x34, 0x45 });
ndef.CardStatusChanged += NdefCardStatusChanged;
ndef.NdefReceived += NdefNdefReceived;
ndef.InitializeAndListen(cts.Token);
```

The card will automatically place itself in the listen mode and will listen up to a cancellation token is set to cancel.

This implies in this mode that once the card is deselected and read already, placing it in listen more again can trigger the listener to select it again.

So you can use the `CardStatusChanged` event to adjust the needed behavior. Both `Initialize` and `Listen` functions can be used in a more granular way.

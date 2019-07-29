# Credit Card - Credit Card

This CreditCard class allows to read thru a standard Near Field Communiction (NFC) or Smart Card (PC/SC) reader. Be aware that to read the data stored in the card, you need a compatible reader:
- An ISO 14443-4 Type B for NFC. Keep in lind as well that the antena and the gain of your NFC reader need to be strong enough to read all kind of cards. Most of the popular PN532 NFC readers that you can find online does not allow to read all kind of card due to this issue
- A smart card reader compatible to read secured card

Your credit card needs to be compatible with wireless payments for NFC reading. It needs to have a chip to be read by a smart card reader.

You will need a reader with an implemetation of a WriteRead function from the class CardWriteRead. You will find an example with the PN532 below.

## Usage

Here is a full example on how to read a Credit Card using a PN532. As mention above, the quality of your antena and gain of the antena may not allow you to read your card even if it's compatible.

```csharp
static void Main(string[] args)
{
    Pn532 pn532 = new Pn532("COM7", LogLevel.None);
    ReadCreditCard(pn532);
}

static void ReadCreditCard(Pn532 pn532)
{
    byte[] retData = null;
    while ((!Console.KeyAvailable))
    {
        retData = pn532.AutoPoll(5, 300, new PollingType[] { PollingType.Passive106kbpsISO144443_4B });
        if (retData != null)
            if (retData.Length >= 3)
                break;
        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData == null)
        return;

    // Check how many cards and the type
    Console.WriteLine($"Num tags: {retData[0]}, Type: {(PollingType)retData[1]}");
    var decrypted = pn532.TryDecodeData106kbpsTypeB(retData.AsSpan().Slice(3));
    if (decrypted != null)
    {
        Console.WriteLine($"{decrypted.TargetNumber}, Serial: {BitConverter.ToString(decrypted.NfcId)}, App Data: {BitConverter.ToString(decrypted.ApplicationData)}, " +
            $"{decrypted.ApplicationType}, Bit Rates: {decrypted.BitRates}, CID {decrypted.CidSupported}, Command: {decrypted.Command}, FWT: {decrypted.FrameWaitingTime}, " +
            $"ISO144443 compliance: {decrypted.ISO14443_4Compliance}, Max Frame size: {decrypted.MaxFrameSize}, NAD: {decrypted.NadSupported}");

        CreditCard creditCard = new CreditCard(pn532, decrypted.TargetNumber);
        creditCard.FillCreditCardInfo();

        Console.WriteLine("All Tags for the Credit Card:");
        DisplayTags(creditCard.Tags, 0);

    }
}

static string AddSpace(int level)
{
    string space = "";
    for (int i = 0; i < level; i++)
        space += "  ";

    return space;
}

static void DisplayTags(List<Tag> tagToDisplay, int levels)
{
    foreach (var tagparent in tagToDisplay)
    {
        Console.Write(AddSpace(levels) + $"{tagparent.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault()?.Description}");
        var isTemplate = TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault();
        if ((isTemplate?.IsTemplate == true) || (isTemplate?.IsConstructed == true))
        {
            Console.WriteLine();
            DisplayTags(tagparent.Tags, levels + 1);
        }
        else if (isTemplate?.IsDol == true)
        {
            //In this case, all the data inside are 1 byte only
            Console.WriteLine(", Data Object Length elements:");
            foreach (var dt in tagparent.Tags)
            {
                Console.Write(AddSpace(levels + 1) + $"{dt.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == dt.TagNumber).FirstOrDefault()?.Description}");
                Console.WriteLine($", data length: {dt.Data[0]}");
            }
        }
        else
        {
            TagDetails tg = new TagDetails(tagparent);
            Console.WriteLine($": {tg.ToString()}");
        }
    }
}
```

### Creating a Credit Card class

You just need to pass a compatible CardWriteRead class and a target number. Note that some readers don't have this nothing, in this case, just ignore this setting.

```csharp
CreditCard creditCard = new CreditCard(pn532, decrypted.TargetNumber);        
```

### Getting all the card information

The ```FillCreditCardInformation``` function gets all the public data from your card.

```csharp
creditCard.FillCreditCardInformation();
```

### Accessing the information

The Credit Card stores data in Tags. Those Tags have a specific number and data. The data format, the decription of those tags are available in the TagList class. You can use it to display those data.

Note as well that a ```ToString()``` converter has been implented. The function in the example above ```DisplayTags``` gives an example on how you can display all the Tags and their data.


## Limitations

This Credit Card class allows you to gather all the public information present into your credit card. The class does not implement fully all what is necessary to initate a payment. Nothing prevent it as all the primary elements are present but you'll need to implement the Authentication.
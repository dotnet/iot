# NMEA 0183 Protocol

## Summary

NMEA stands for `National Marine Electronics Associations`.

NMEA 0183 is an industry standard first released in 1983 and has been updated several times since then.

In NMEA 0183 device is either a talker or a listener. In most common scenario there is only one talker.

There are multiple types of sentences (or messages) which can be sent:
- talker sentence (`TalkerSentence` class) - most common message
- query sentence (`QuerySentence` class)
- propertiary sentence (not available here)

Each message has a talker identifier (see `TalkerIdentifier`), sentence identifier (see `SentenceId`), fields and optional checksum.

Sentence identifier decides on what format the fields will be in.
There is a whole variety of sentences device can send and currently only [one](Sentences/RecommendedMinimumNavigationInformation.cs) is fully supported.
Fields for all types of sentences can be inspected.

## Samples

Please refer to [NEO-M8 sample](../Neo-M8/samples/README.md).

## Guidelines for adding new sentence identifiers

- Base a new sentence identifier on [RMC sentence](Sentences/RecommendedMinimumNavigationInformation.cs)
- Modify `GetKnownSentences` in [TalkerSentence.cs](TalkerSentence.cs) or call `TalkerSentence.RegisterSentence` in the beginning of your `Main` method

## References 

- https://www.nmea.org/
- http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf

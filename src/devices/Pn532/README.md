
# contact less

https://www.emvco.com/emv-technologies/contactless/

# mode émulation

https://stackoverflow.com/questions/21051315/nfc-acr122-tginitastarget-initiator-releasing-target/21107403#21107403

# EMV
Advanced EMV parsing: https://iso8583.info/lib/EMV/

Main documentation for EMV: https://www.emvco.com/emv-technologies/contact/

Return InAutoPoll

		[0]	1	byte => Nb tags
		[1]	35	byte => Type
		[2]	15	byte => Length of data, next part calls AutoPoll
		[3]	1	byte   => Target
		[4]	80	byte   => ????
		[5]	0	byte   => NFCID1
		[6]	186	byte   => NFCID2
		[7]	227	byte   => NFCID3
		[8]	42	byte   => NFCID4
		[9]	0	byte   => AppData1
		[10]	0	byte   => AppData2
		[11]	0	byte   => AppData3
		[12]	0	byte   => AppData4
		[13]	128	byte   =>Bit Rate Capacity
		[14]	129	byte   => Max Frame Size/-4 Info
		[15]	113	byte   =>FWI/Coding Options
		[16]	1	byte ===> ATTRIB_RES  length / CRC1 ?
		[17]	1	byte ===> ATTRIB_RES  / CRC2?

		ATQB Description: http://ww1.microchip.com/downloads/en/AppNotes/doc2056.pdf page 16

Buffer

6F-62-84-0E-11-22-33-44-55-66-11-77-66-55-44-33-22-11-A5-50-BF-0C-4D-61-26-4F-07-A0-00-00-00-42-10-10-50-02-43-42-87-01-01-9F-2A-08-02-00-00-00-00-00-00-00-9F-0A-08-00-01-05-02-00-00-00-00-61-23-4F-07-A0-00-00-00-04-10-10-50-0A-4D-41-53-54-45-52-43-41-52-44-87-01-02-9F-0A-08-00-01-05-02

Détails pour la structure de paiement: https://salmg.net/2017/09/12/intro-to-analyze-nfc-contactless-cards/
Liste des AID: https://fr.wikipedia.org/wiki/Europay_Mastercard_Visa, https://en.wikipedia.org/wiki/EMV
Pour décoder les TLV: https://www.emvlab.org/tlvutils/?data=6F+6A+84+0E+32+50+41+59+2E+53+59+53+2E+44+44+46+30+31+A5+58+BF+0C+55+61+26+4F+07+A0+00+00+00+03+10+10+50+0A+56+69+73+61+20+44+65+62+69+74+87+01+01+9F+2A+01+03+5F+55+02+55+53+42+03+46+50+98+61+2B+4F+07+A0+00+00+00+98+08+40+50+0F+55+53+20+43+6F+6D+6D+6F+6E+20+44+65+62+69+74+87+01+02+9F+2A+01+03+5F+55+02+55+53+42+03+46+50+98+90+00

requête opour numéro de carte: 
Pour numéro de carte:
6F-4F-84-07-A0-00-00-00-42-10-10-A5-44-50-0B-43-42-20-43-4F-4D-50-54-41-4E-54-87-01-01-9F-38-18-9F-66-04-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-5F-2D-04-66-72-65-6E-BF-0C-0F-DF-61-01-03-9F-0A-08-00-01-05-02-00-00-00-00-90-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-
Card number:	9F-66-04-
				9F-02-06- 
				9F-03-06- 
				9F-1A-02-
				95-05-
				5F-2A-02
				9A-03
				9C-01
				9F-37-04

{0x40,0x01,
						0x80,0xCA, // GET DATA
						0x9F,0x4F, // 9F4F asks for the log format
						0x00};

All AID: https://www.eftlab.co.uk/knowledge-base/211-emv-aid-rid-pix/

Track 1 equivalent data: 33-34-38-37-37-37-34-37-38-30-30-30-30-30-30-34-37-38-30-30-30-30-30-30

APPID: A0-00-00-00-42-10-10, Priority: 1, CB COMPTANT
SFI 1, Record 1
70-3F-5F-20-02-20-2F-9F-1F-18-33-34-38-37-37-37-34-37-38-30-30-30-30-30-30-34-37-38-30-30-30-30-30-30-9F-0B-1C-20-2F-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-90-00-00
SFI 2, Record 1
70-81-B3-90-81-B0-63-7F-FF-E7-01-4C-54-8E-5C-B2-8F-8A-D0-73-B8-D3-55-1A-66-A0-C1-58-06-C5-D8-E1-93-B8-BE-B8-26-C4-6D-F3-9F-04-CC-65-A9-FE-46-C8-7D-2C-BF-FD-51-43-1B-CF-32-1F-3F-18-25-26-0C-A6-51-F8-AE-39-6C-D0-DA-13-4B-B8-3C-37-12-2F-2B-69-97-4D-12-25-FF-CC-3C-51-F6-12-F7-A9-86-15-E9-F7-83-E0-40-19-28-1D-A3-A0-DE-A4-99-14-CD-C2-C2-F7-EE-F8-06-8D-39-3B-21-FE-FB-9E-0A-79-0D-69-BC-4D-6D-30-4D-06-0B-46-2E-99-F6-9A-04-3B-06-85-3C-5F-FA-F5-46-06-C2-08-B1-46-28-8E-CC-0F-BF-92-FB-C1-7A-B5-0B-A5-8E-9F-C4-DD-17-C9-ED-70-FC-87-F1-82-87-96-D1-71-88-54-90-00-00
SFI 3, Record 1
70-1E-5A-08-49-74-90-82-42-33-08-25-5F-24-03-22-03-31-9F-4A-01-82-9F-07-02-FF-00-5F-28-02-02-50-90-00-00
SFI 3, Record 3
70-04-5F-34-01-00-90-00-00

APPID: A0-00-00-00-03-10-10, Priority: 2, VISA
SFI 1, Record 1
70-3F-5F-20-02-20-2F-9F-1F-18-33-34-38-37-37-37-34-37-38-30-30-30-30-30-30-34-37-38-30-30-30-30-30-30-9F-0B-1C-20-2F-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-20-90-00-00
SFI 2, Record 1
70-81-B3-90-81-B0-0D-1E-BE-B3-EC-F6-1B-D6-E3-74-24-CC-A5-A0-40-80-D1-95-2D-94-D0-31-B8-B0-55-C9-66-40-60-D7-84-58-31-5F-C0-4F-AA-5B-E6-8E-49-7F-20-EF-37-D4-95-00-4D-62-FA-98-8C-8B-48-A4-04-18-42-9F-83-88-A5-E9-1F-EE-6C-CC-2D-68-04-23-FA-A6-1C-9B-60-C3-45-58-84-D4-80-2D-ED-B4-16-7E-54-54-85-2D-CA-D0-1C-D6-E8-E3-C7-E5-06-05-E2-18-32-01-41-85-FB-91-41-32-31-BA-5B-58-EE-B9-A8-29-D4-69-2D-F3-EA-9F-EE-FC-D2-61-38-EB-27-56-1B-7B-38-C6-1A-05-3B-7A-2F-0C-B4-2E-E5-7C-1B-99-36-E3-FB-CB-01-70-AA-E7-32-14-D0-9A-3E-A3-0C-6B-59-40-6E-40-A2-52-4B-53-E2-76-90-00-00
SFI 2, Record 3
70-81-F7-8F-01-08-92-24-59-3A-16-C8-E0-DD-54-AC-EC-C7-F4-C3-3F-63-83-28-DF-42-3D-6A-08-74-9D-5F-2F-08-B3-BB-FB-9B-0B-11-7D-AA-37-8F-9F-32-01-03-9F-46-81-B0-71-EC-81-D6-1D-D4-D0-09-FF-61-39-D0-21-89-BA-05-FB-DE-2D-9C-D3-70-94-78-E2-35-FB-E5-0B-1B-F6-81-22-4C-ED-BF-48-25-A5-90-16-17-92-62-4B-C5-57-26-B0-E2-1E-F5-C1-2F-A7-53-98-60-8D-76-F8-7F-2F-FE-11-76-2F-59-C6-0D-1F-83-C1-26-70-CE-1A-26-DD-1C-87-A8-86-23-22-5B-38-0D-68-4F-E9-CF-68-0D-94-2D-A1-AD-76-E4-3F-15-5C-19-CF-42-2A-6F-02-33-FB-4E-9D-BF-F5-BB-99-55-53-F0-17-9A-A5-DC-1B-A3-5E-AB-29-9A-D7-9A-59-A6-4A-BA-9B-E0-22-EB-D5-4C-A2-47-65-51-5E-6F-FF-10-4A-64-BF-DA-F9-92-65-A0-45-00-1F-3E-1C-F5-A8-D9-19-50-9D-6C-A8-FA-83-FA-BF-E1-9F-47-01-03-9F-08-02-00-96-9F-48-0A-3B-A5-D0-4A-61-8C-E6-A1-09-CB-90-00-00
SFI 3, Record 1
70-1E-5A-08-49-74-90-82-42-33-08-25-5F-24-03-22-03-31-9F-4A-01-82-9F-07-02-FF-00-5F-28-02-02-50-90-00-00
SFI 3, Record 1
70-1E-5A-08-49-74-90-82-42-33-08-25-5F-24-03-22-03-31-9F-4A-01-82-9F-07-02-FF-00-5F-28-02-02-50-90-00-00

APPID: A0-00-00-00-42-10-10, Priority: 1, CB
SFI 2, Record 1
70-81-A2-57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0A-00-00-00-00-00-00-00-00-1F-03-9F-07-02-FF-00-9F-08-02-00-03-9F-0D-05-B4-60-60-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-60-98-00-9F-42-02-09-78-9F-4A-01-82-90-00-00
SFI 3, Record 1
70-81-E0-8F-01-07-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-89-77-9D-D3-FF-DE-88-16-A9-1E-CF-27-5C-14-0E-C8-C3-CA-29-F9-AA-D7-89-2D-E8-47-2A-86-36-7F-B1-B5-DD-BD-6C-D1-F4-13-54-FC-C5-0A-E5-75-B9-1B-41-37-F1-F0-34-F1-6C-E2-3D-D4-7A-94-33-97-CC-14-0D-6F-87-3C-E4-D9-DB-54-B4-F5-F6-29-81-BE-4E-E8-27-29-24-F7-ED-63-BE-83-29-16-85-82-B1-B3-AE-62-43-28-96-D3-CD-DA-31-E7-3A-F4-C8-13-B7-91-AF-B9-B0-7C-84-27-44-D6-0F-BC-DE-BE-5A-89-1D-A0-28-21-ED-D7-1F-87-E4-B8-64-61-38-DF-8B-25-C7-C2-4D-15-6F-8E-E8-3E-B5-78-FA-27-0E-ED-6B-7F-DD-1F-C8-F2-9E-14-1A-49-10-83-73-73-AF-ED-DB-D4-8F-6F-FA-0E-32-26-90-00-00
70-13-9F-47-03-01-00-01-9F-48-0A-CC-67-69-F3-2B-EE-D0-F2-8B-CB-90-00-00
SFI 4, Record 2
70-81-B4-9F-46-81-B0-9E-2B-AE-30-D2-57-A4-A1-16-8D-11-D8-98-E1-7B-C8-E6-98-59-BE-68-38-4B-80-3F-39-F5-C9-09-82-5C-A9-A9-65-D8-5B-3D-9B-B0-D7-03-BA-F8-D0-AD-EE-2D-C7-8F-CB-C7-CF-FF-95-B6-95-B2-89-28-FB-09-24-6B-77-02-DF-CA-3C-7B-F9-94-6B-89-7E-E9-36-EE-2E-F7-19-DF-5E-9F-EB-B4-DD-27-C1-D8-1A-9D-B2-4D-23-09-BB-05-59-92-CF-10-6F-0B-E6-D7-26-47-AA-FF-51-DF-A7-33-34-DF-13-52-5B-99-95-F9-B4-4E-36-F5-B6-E5-12-9C-BA-01-41-A8-97-9D-50-F6-A6-70-F7-DD-BB-BE-31-0D-50-81-F5-D0-6E-D7-0D-BD-1F-F8-37-FF-31-92-2B-99-41-B8-AD-B4-F6-1A-9F-35-B0-3D-B7-08-B7-53-7A-90-00-00

APPID: A0-00-00-00-04-10-10, Priority: 2, MASTERCARD
SFI 1, Record 1
70-75-9F-6C-02-00-01-9F-65-02-00-70-9F-66-02-0E-0E-9F-6B-13-51-36-36-40-00-86-25-20-D2-10-52-01-90-00-01-00-00-00-0F-9F-67-01-03-56-34-42-35-31-33-36-33-36-34-30-30-30-38-36-32-35-32-30-5E-20-2F-5E-32-31-30-35-32-30-31-30-31-30-30-30-33-33-33-30-30-30-32-32-32-32-32-30-30-30-31-31-31-31-30-9F-62-06-00-00-00-38-00-00-9F-63-06-00-00-00-00-E0-E0-9F-64-01-03-90-00-00
SFI 2, Record 1
70-81-A6-57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0E-00-00-00-00-00-00-00-00-42-03-1E-03-1F-03-9F-07-02-FF-00-9F-08-02-00-02-9F-0D-05-B4-60-84-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-84-80-00-9F-42-02-09-78-9F-4A-01-82-90-00-00
SFI 3, Record 1
70-81-E0-8F-01-05-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-9E-4C-5B-44-4C-5A-ED-60-5D-44-F5-78-A4-83-65-22-1F-99-91-A0-9D-3C-8B-EE-53-60-38-7A-50-40-EE-84-BF-86-EC-16-92-54-16-4F-3B-D5-E0-80-EF-0E-60-FD-72-C8-BF-98-99-0B-9D-05-64-15-73-09-91-78-45-BC-42-BF-AA-BE-63-AE-81-7E-E9-97-95-1A-7A-EE-91-FF-28-70-08-59-CB-E3-C7-7F-71-A4-2E-50-56-49-E3-4C-9D-5B-4F-B5-8C-02-02-21-1A-D1-D4-DF-BD-CE-99-9E-D3-7D-3E-DF-46-E7-C0-1B-96-FD-7E-D2-9F-A4-2F-50-E9-A1-36-F6-47-65-08-F3-1D-8A-9E-BF-F2-C8-5C-3D-E8-8D-98-93-C8-E2-BA-F8-B8-8B-DE-9E-79-B3-72-DB-BA-89-B4-94-78-76-B1-46-CC-F6-24-9F-1F-7A-63-D1-90-00-00
SFI 3, Record 2
70-03-93-01-FF-90-00-00
SFI 4, Record 1
70-13-9F-47-03-01-00-01-9F-48-0A-8B-D1-39-05-52-C7-DF-F2-5E-7D-90-00-00
SFI 4, Record 2
70-81-B4-9F-46-81-B0-8F-05-72-FD-84-CD-B2-E8-EB-43-C4-55-C8-BE-76-4A-38-BC-7C-8E-67-46-7D-FC-A9-88-E2-9A-DA-BD-AF-11-30-67-56-B3-64-03-5F-9C-44-30-BF-3C-59-70-47-37-EC-A8-39-07-05-A5-98-6E-99-F5-99-91-8B-6F-39-FA-B4-CE-88-CF-78-73-2F-27-9F-6C-0E-56-8A-D7-12-36-C5-AB-E2-89-A4-C3-8F-9A-C0-F7-28-1D-A3-14-72-1E-AF-10-93-AD-19-41-A7-2A-13-50-E8-2B-9A-72-1C-91-E4-40-28-7E-DA-A7-63-8E-E6-0F-16-27-7C-C0-9E-A6-AE-C2-B6-62-88-0A-CD-13-73-C4-BA-A9-E3-42-43-81-EC-29-8D-66-97-AB-26-1C-84-77-D3-5D-5E-86-EA-F9-F2-4B-7E-39-22-57-A4-18-2F-97-D8-FA-F3-3B-F2-F5-90-00-00


Dedicated Name: 32-50-41-59-2E-53-59-53-2E-44-44-46-30-31
APPID: A0-00-00-00-42-10-10, Priority: 1, CB
ok
SFI 1, Record 1
Error: 6A-82-00
SFI 1, Record 2
Error: 6A-82-00
SFI 1, Record 3
Error: 6A-82-00
SFI 1, Record 4
Error: 6A-82-00
SFI 2, Record 1
70-81-A2-57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0A-00-00-00-00-00-00-00-00-1F-03-9F-07-02-FF-00-9F-08-02-00-03-9F-0D-05-B4-60-60-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-60-98-00-9F-42-02-09-78-9F-4A-01-82-90-00-00
Tag: 0070, Data: 57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0A-00-00-00-00-00-00-00-00-1F-03-9F-07-02-FF-00-9F-08-02-00-03-9F-0D-05-B4-60-60-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-60-98-00-9F-42-02-09-78-9F-4A-01-82
Tag: 0057, Data: 51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F
Tag: 005A, Data: 51-36-36-40-00-86-25-20
Tag: 5F24, Data: 21-05-31
Tag: 5F25, Data: 19-04-01
Tag: 5F28, Data: 02-50
Tag: 5F34, Data: 01
Tag: 008C, Data: 9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14
Tag: 008D, Data: 91-0A-8A-02-95-05-9F-37-04-9F-4C-08
Tag: 008E, Data: 00-00-00-00-00-00-00-00-1F-03
Tag: 9F07, Data: FF-00
Tag: 9F08, Data: 00-03
Tag: 9F0D, Data: B4-60-60-80-00
Tag: 9F0E, Data: 00-10-00-00-00
Tag: 9F0F, Data: B4-60-60-98-00
Tag: 9F42, Data: 09-78
Tag: 9F4A, Data: 82
SFI 2, Record 2
Error: 6A-83-00
SFI 2, Record 3
Error: 6A-83-00
SFI 2, Record 4
Error: 6A-83-00
SFI 3, Record 1
70-81-E0-8F-01-07-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-89-77-9D-D3-FF-DE-88-16-A9-1E-CF-27-5C-14-0E-C8-C3-CA-29-F9-AA-D7-89-2D-E8-47-2A-86-36-7F-B1-B5-DD-BD-6C-D1-F4-13-54-FC-C5-0A-E5-75-B9-1B-41-37-F1-F0-34-F1-6C-E2-3D-D4-7A-94-33-97-CC-14-0D-6F-87-3C-E4-D9-DB-54-B4-F5-F6-29-81-BE-4E-E8-27-29-24-F7-ED-63-BE-83-29-16-85-82-B1-B3-AE-62-43-28-96-D3-CD-DA-31-E7-3A-F4-C8-13-B7-91-AF-B9-B0-7C-84-27-44-D6-0F-BC-DE-BE-5A-89-1D-A0-28-21-ED-D7-1F-87-E4-B8-64-61-38-DF-8B-25-C7-C2-4D-15-6F-8E-E8-3E-B5-78-FA-27-0E-ED-6B-7F-DD-1F-C8-F2-9E-14-1A-49-10-83-73-73-AF-ED-DB-D4-8F-6F-FA-0E-32-26-90-00-00
Tag: 0070, Data: 8F-01-07-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-89-77-9D-D3-FF-DE-88-16-A9-1E-CF-27-5C-14-0E-C8-C3-CA-29-F9-AA-D7-89-2D-E8-47-2A-86-36-7F-B1-B5-DD-BD-6C-D1-F4-13-54-FC-C5-0A-E5-75-B9-1B-41-37-F1-F0-34-F1-6C-E2-3D-D4-7A-94-33-97-CC-14-0D-6F-87-3C-E4-D9-DB-54-B4-F5-F6-29-81-BE-4E-E8-27-29-24-F7-ED-63-BE-83-29-16-85-82-B1-B3-AE-62-43-28-96-D3-CD-DA-31-E7-3A-F4-C8-13-B7-91-AF-B9-B0-7C-84-27-44-D6-0F-BC-DE-BE-5A-89-1D-A0-28-21-ED-D7-1F-87-E4-B8-64-61-38-DF-8B-25-C7-C2-4D-15-6F-8E-E8-3E-B5-78-FA-27-0E-ED-6B-7F-DD-1F-C8-F2-9E-14-1A-49-10-83-73-73-AF-ED-DB-D4-8F-6F-FA-0E-32-26
Tag: 008F, Data: 07
Tag: 9F32, Data: 03
Tag: 0092, Data: 24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99
Tag: 0090, Data: 89-77-9D-D3-FF-DE-88-16-A9-1E-CF-27-5C-14-0E-C8-C3-CA-29-F9-AA-D7-89-2D-E8-47-2A-86-36-7F-B1-B5-DD-BD-6C-D1-F4-13-54-FC-C5-0A-E5-75-B9-1B-41-37-F1-F0-34-F1-6C-E2-3D-D4-7A-94-33-97-CC-14-0D-6F-87-3C-E4-D9-DB-54-B4-F5-F6-29-81-BE-4E-E8-27-29-24-F7-ED-63-BE-83-29-16-85-82-B1-B3-AE-62-43-28-96-D3-CD-DA-31-E7-3A-F4-C8-13-B7-91-AF-B9-B0-7C-84-27-44-D6-0F-BC-DE-BE-5A-89-1D-A0-28-21-ED-D7-1F-87-E4-B8-64-61-38-DF-8B-25-C7-C2-4D-15-6F-8E-E8-3E-B5-78-FA-27-0E-ED-6B-7F-DD-1F-C8-F2-9E-14-1A-49-10-83-73-73-AF-ED-DB-D4-8F-6F-FA-0E-32-26
SFI 3, Record 2
Error: 6A-83-00
SFI 3, Record 3
Error: 6A-83-00
SFI 3, Record 4
Error: 6A-83-00
SFI 4, Record 1
70-13-9F-47-03-01-00-01-9F-48-0A-CC-67-69-F3-2B-EE-D0-F2-8B-CB-90-00-00
Tag: 0070, Data: 9F-47-03-01-00-01-9F-48-0A-CC-67-69-F3-2B-EE-D0-F2-8B-CB
Tag: 9F47, Data: 01-00-01
Tag: 9F48, Data: CC-67-69-F3-2B-EE-D0-F2-8B-CB
SFI 4, Record 2
70-81-B4-9F-46-81-B0-9E-2B-AE-30-D2-57-A4-A1-16-8D-11-D8-98-E1-7B-C8-E6-98-59-BE-68-38-4B-80-3F-39-F5-C9-09-82-5C-A9-A9-65-D8-5B-3D-9B-B0-D7-03-BA-F8-D0-AD-EE-2D-C7-8F-CB-C7-CF-FF-95-B6-95-B2-89-28-FB-09-24-6B-77-02-DF-CA-3C-7B-F9-94-6B-89-7E-E9-36-EE-2E-F7-19-DF-5E-9F-EB-B4-DD-27-C1-D8-1A-9D-B2-4D-23-09-BB-05-59-92-CF-10-6F-0B-E6-D7-26-47-AA-FF-51-DF-A7-33-34-DF-13-52-5B-99-95-F9-B4-4E-36-F5-B6-E5-12-9C-BA-01-41-A8-97-9D-50-F6-A6-70-F7-DD-BB-BE-31-0D-50-81-F5-D0-6E-D7-0D-BD-1F-F8-37-FF-31-92-2B-99-41-B8-AD-B4-F6-1A-9F-35-B0-3D-B7-08-B7-53-7A-90-00-00
Tag: 0070, Data: 9F-46-81-B0-9E-2B-AE-30-D2-57-A4-A1-16-8D-11-D8-98-E1-7B-C8-E6-98-59-BE-68-38-4B-80-3F-39-F5-C9-09-82-5C-A9-A9-65-D8-5B-3D-9B-B0-D7-03-BA-F8-D0-AD-EE-2D-C7-8F-CB-C7-CF-FF-95-B6-95-B2-89-28-FB-09-24-6B-77-02-DF-CA-3C-7B-F9-94-6B-89-7E-E9-36-EE-2E-F7-19-DF-5E-9F-EB-B4-DD-27-C1-D8-1A-9D-B2-4D-23-09-BB-05-59-92-CF-10-6F-0B-E6-D7-26-47-AA-FF-51-DF-A7-33-34-DF-13-52-5B-99-95-F9-B4-4E-36-F5-B6-E5-12-9C-BA-01-41-A8-97-9D-50-F6-A6-70-F7-DD-BB-BE-31-0D-50-81-F5-D0-6E-D7-0D-BD-1F-F8-37-FF-31-92-2B-99-41-B8-AD-B4-F6-1A-9F-35-B0-3D-B7-08-B7-53-7A
Tag: 9F46, Data: 9E-2B-AE-30-D2-57-A4-A1-16-8D-11-D8-98-E1-7B-C8-E6-98-59-BE-68-38-4B-80-3F-39-F5-C9-09-82-5C-A9-A9-65-D8-5B-3D-9B-B0-D7-03-BA-F8-D0-AD-EE-2D-C7-8F-CB-C7-CF-FF-95-B6-95-B2-89-28-FB-09-24-6B-77-02-DF-CA-3C-7B-F9-94-6B-89-7E-E9-36-EE-2E-F7-19-DF-5E-9F-EB-B4-DD-27-C1-D8-1A-9D-B2-4D-23-09-BB-05-59-92-CF-10-6F-0B-E6-D7-26-47-AA-FF-51-DF-A7-33-34-DF-13-52-5B-99-95-F9-B4-4E-36-F5-B6-E5-12-9C-BA-01-41-A8-97-9D-50-F6-A6-70-F7-DD-BB-BE-31-0D-50-81-F5-D0-6E-D7-0D-BD-1F-F8-37-FF-31-92-2B-99-41-B8-AD-B4-F6-1A-9F-35-B0-3D-B7-08-B7-53-7A
SFI 4, Record 3
Error: 6A-83-00
SFI 4, Record 4
Error: 6A-83-00
APPID: A0-00-00-00-04-10-10, Priority: 2, MASTERCARD
ok
SFI 1, Record 1
70-75-9F-6C-02-00-01-9F-65-02-00-70-9F-66-02-0E-0E-9F-6B-13-51-36-36-40-00-86-25-20-D2-10-52-01-90-00-01-00-00-00-0F-9F-67-01-03-56-34-42-35-31-33-36-33-36-34-30-30-30-38-36-32-35-32-30-5E-20-2F-5E-32-31-30-35-32-30-31-30-31-30-30-30-33-33-33-30-30-30-32-32-32-32-32-30-30-30-31-31-31-31-30-9F-62-06-00-00-00-38-00-00-9F-63-06-00-00-00-00-E0-E0-9F-64-01-03-90-00-00
Tag: 0070, Data: 9F-6C-02-00-01-9F-65-02-00-70-9F-66-02-0E-0E-9F-6B-13-51-36-36-40-00-86-25-20-D2-10-52-01-90-00-01-00-00-00-0F-9F-67-01-03-56-34-42-35-31-33-36-33-36-34-30-30-30-38-36-32-35-32-30-5E-20-2F-5E-32-31-30-35-32-30-31-30-31-30-30-30-33-33-33-30-30-30-32-32-32-32-32-30-30-30-31-31-31-31-30-9F-62-06-00-00-00-38-00-00-9F-63-06-00-00-00-00-E0-E0-9F-64-01-03
Tag: 9F6C, Data: 00-01
Tag: 9F65, Data: 00-70
Tag: 9F66, Data: 0E-0E
Tag: 9F6B, Data: 51-36-36-40-00-86-25-20-D2-10-52-01-90-00-01-00-00-00-0F
Tag: 9F67, Data: 03
Tag: 0056, Data: 42-35-31-33-36-33-36-34-30-30-30-38-36-32-35-32-30-5E-20-2F-5E-32-31-30-35-32-30-31-30-31-30-30-30-33-33-33-30-30-30-32-32-32-32-32-30-30-30-31-31-31-31-30
Tag: 9F62, Data: 00-00-00-38-00-00
Tag: 9F63, Data: 00-00-00-00-E0-E0
Tag: 9F64, Data: 03
SFI 1, Record 2
Error: 6A-83-00
SFI 1, Record 3
Error: 6A-83-00
SFI 1, Record 4
Error: 6A-83-00
SFI 2, Record 1
70-81-A6-57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0E-00-00-00-00-00-00-00-00-42-03-1E-03-1F-03-9F-07-02-FF-00-9F-08-02-00-02-9F-0D-05-B4-60-84-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-84-80-00-9F-42-02-09-78-9F-4A-01-82-90-00-00
Tag: 0070, Data: 57-13-51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F-5A-08-51-36-36-40-00-86-25-20-5F-24-03-21-05-31-5F-25-03-19-04-01-5F-28-02-02-50-5F-34-01-01-8C-27-9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14-8D-0C-91-0A-8A-02-95-05-9F-37-04-9F-4C-08-8E-0E-00-00-00-00-00-00-00-00-42-03-1E-03-1F-03-9F-07-02-FF-00-9F-08-02-00-02-9F-0D-05-B4-60-84-80-00-9F-0E-05-00-10-00-00-00-9F-0F-05-B4-60-84-80-00-9F-42-02-09-78-9F-4A-01-82
Tag: 0057, Data: 51-36-36-40-00-86-25-20-D2-10-52-01-20-88-67-82-60-00-0F
Tag: 005A, Data: 51-36-36-40-00-86-25-20
Tag: 5F24, Data: 21-05-31
Tag: 5F25, Data: 19-04-01
Tag: 5F28, Data: 02-50
Tag: 5F34, Data: 01
Tag: 008C, Data: 9F-02-06-9F-03-06-9F-1A-02-95-05-5F-2A-02-9A-03-9C-01-9F-37-04-9F-35-01-9F-45-02-9F-4C-08-9F-34-03-9F-21-03-9F-7C-14
Tag: 008D, Data: 91-0A-8A-02-95-05-9F-37-04-9F-4C-08
Tag: 008E, Data: 00-00-00-00-00-00-00-00-42-03-1E-03-1F-03
Tag: 9F07, Data: FF-00
Tag: 9F08, Data: 00-02
Tag: 9F0D, Data: B4-60-84-80-00
Tag: 9F0E, Data: 00-10-00-00-00
Tag: 9F0F, Data: B4-60-84-80-00
Tag: 9F42, Data: 09-78
Tag: 9F4A, Data: 82
SFI 2, Record 2
Error: 6A-83-00
SFI 2, Record 3
Error: 6A-83-00
SFI 2, Record 4
Error: 6A-83-00
SFI 3, Record 1
70-81-E0-8F-01-05-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-9E-4C-5B-44-4C-5A-ED-60-5D-44-F5-78-A4-83-65-22-1F-99-91-A0-9D-3C-8B-EE-53-60-38-7A-50-40-EE-84-BF-86-EC-16-92-54-16-4F-3B-D5-E0-80-EF-0E-60-FD-72-C8-BF-98-99-0B-9D-05-64-15-73-09-91-78-45-BC-42-BF-AA-BE-63-AE-81-7E-E9-97-95-1A-7A-EE-91-FF-28-70-08-59-CB-E3-C7-7F-71-A4-2E-50-56-49-E3-4C-9D-5B-4F-B5-8C-02-02-21-1A-D1-D4-DF-BD-CE-99-9E-D3-7D-3E-DF-46-E7-C0-1B-96-FD-7E-D2-9F-A4-2F-50-E9-A1-36-F6-47-65-08-F3-1D-8A-9E-BF-F2-C8-5C-3D-E8-8D-98-93-C8-E2-BA-F8-B8-8B-DE-9E-79-B3-72-DB-BA-89-B4-94-78-76-B1-46-CC-F6-24-9F-1F-7A-63-D1-90-00-00
Tag: 0070, Data: 8F-01-05-9F-32-01-03-92-24-24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99-90-81-B0-9E-4C-5B-44-4C-5A-ED-60-5D-44-F5-78-A4-83-65-22-1F-99-91-A0-9D-3C-8B-EE-53-60-38-7A-50-40-EE-84-BF-86-EC-16-92-54-16-4F-3B-D5-E0-80-EF-0E-60-FD-72-C8-BF-98-99-0B-9D-05-64-15-73-09-91-78-45-BC-42-BF-AA-BE-63-AE-81-7E-E9-97-95-1A-7A-EE-91-FF-28-70-08-59-CB-E3-C7-7F-71-A4-2E-50-56-49-E3-4C-9D-5B-4F-B5-8C-02-02-21-1A-D1-D4-DF-BD-CE-99-9E-D3-7D-3E-DF-46-E7-C0-1B-96-FD-7E-D2-9F-A4-2F-50-E9-A1-36-F6-47-65-08-F3-1D-8A-9E-BF-F2-C8-5C-3D-E8-8D-98-93-C8-E2-BA-F8-B8-8B-DE-9E-79-B3-72-DB-BA-89-B4-94-78-76-B1-46-CC-F6-24-9F-1F-7A-63-D1
Tag: 008F, Data: 05
Tag: 9F32, Data: 03
Tag: 0092, Data: 24-C6-1F-4C-2C-D2-16-52-1C-60-A7-E5-C9-B6-67-A7-24-46-79-02-58-73-6B-F8-E5-26-F7-1A-84-5C-E3-8C-B4-78-33-99
Tag: 0090, Data: 9E-4C-5B-44-4C-5A-ED-60-5D-44-F5-78-A4-83-65-22-1F-99-91-A0-9D-3C-8B-EE-53-60-38-7A-50-40-EE-84-BF-86-EC-16-92-54-16-4F-3B-D5-E0-80-EF-0E-60-FD-72-C8-BF-98-99-0B-9D-05-64-15-73-09-91-78-45-BC-42-BF-AA-BE-63-AE-81-7E-E9-97-95-1A-7A-EE-91-FF-28-70-08-59-CB-E3-C7-7F-71-A4-2E-50-56-49-E3-4C-9D-5B-4F-B5-8C-02-02-21-1A-D1-D4-DF-BD-CE-99-9E-D3-7D-3E-DF-46-E7-C0-1B-96-FD-7E-D2-9F-A4-2F-50-E9-A1-36-F6-47-65-08-F3-1D-8A-9E-BF-F2-C8-5C-3D-E8-8D-98-93-C8-E2-BA-F8-B8-8B-DE-9E-79-B3-72-DB-BA-89-B4-94-78-76-B1-46-CC-F6-24-9F-1F-7A-63-D1
SFI 3, Record 2
70-03-93-01-FF-90-00-00
Tag: 0070, Data: 93-01-FF
Tag: 0093, Data: FF
SFI 3, Record 3
Error: 6A-83-00
SFI 3, Record 4
Error: 6A-83-00
SFI 4, Record 1
70-13-9F-47-03-01-00-01-9F-48-0A-8B-D1-39-05-52-C7-DF-F2-5E-7D-90-00-00
Tag: 0070, Data: 9F-47-03-01-00-01-9F-48-0A-8B-D1-39-05-52-C7-DF-F2-5E-7D
Tag: 9F47, Data: 01-00-01
Tag: 9F48, Data: 8B-D1-39-05-52-C7-DF-F2-5E-7D
SFI 4, Record 2
70-81-B4-9F-46-81-B0-8F-05-72-FD-84-CD-B2-E8-EB-43-C4-55-C8-BE-76-4A-38-BC-7C-8E-67-46-7D-FC-A9-88-E2-9A-DA-BD-AF-11-30-67-56-B3-64-03-5F-9C-44-30-BF-3C-59-70-47-37-EC-A8-39-07-05-A5-98-6E-99-F5-99-91-8B-6F-39-FA-B4-CE-88-CF-78-73-2F-27-9F-6C-0E-56-8A-D7-12-36-C5-AB-E2-89-A4-C3-8F-9A-C0-F7-28-1D-A3-14-72-1E-AF-10-93-AD-19-41-A7-2A-13-50-E8-2B-9A-72-1C-91-E4-40-28-7E-DA-A7-63-8E-E6-0F-16-27-7C-C0-9E-A6-AE-C2-B6-62-88-0A-CD-13-73-C4-BA-A9-E3-42-43-81-EC-29-8D-66-97-AB-26-1C-84-77-D3-5D-5E-86-EA-F9-F2-4B-7E-39-22-57-A4-18-2F-97-D8-FA-F3-3B-F2-F5-90-00-00
Tag: 0070, Data: 9F-46-81-B0-8F-05-72-FD-84-CD-B2-E8-EB-43-C4-55-C8-BE-76-4A-38-BC-7C-8E-67-46-7D-FC-A9-88-E2-9A-DA-BD-AF-11-30-67-56-B3-64-03-5F-9C-44-30-BF-3C-59-70-47-37-EC-A8-39-07-05-A5-98-6E-99-F5-99-91-8B-6F-39-FA-B4-CE-88-CF-78-73-2F-27-9F-6C-0E-56-8A-D7-12-36-C5-AB-E2-89-A4-C3-8F-9A-C0-F7-28-1D-A3-14-72-1E-AF-10-93-AD-19-41-A7-2A-13-50-E8-2B-9A-72-1C-91-E4-40-28-7E-DA-A7-63-8E-E6-0F-16-27-7C-C0-9E-A6-AE-C2-B6-62-88-0A-CD-13-73-C4-BA-A9-E3-42-43-81-EC-29-8D-66-97-AB-26-1C-84-77-D3-5D-5E-86-EA-F9-F2-4B-7E-39-22-57-A4-18-2F-97-D8-FA-F3-3B-F2-F5
Tag: 9F46, Data: 8F-05-72-FD-84-CD-B2-E8-EB-43-C4-55-C8-BE-76-4A-38-BC-7C-8E-67-46-7D-FC-A9-88-E2-9A-DA-BD-AF-11-30-67-56-B3-64-03-5F-9C-44-30-BF-3C-59-70-47-37-EC-A8-39-07-05-A5-98-6E-99-F5-99-91-8B-6F-39-FA-B4-CE-88-CF-78-73-2F-27-9F-6C-0E-56-8A-D7-12-36-C5-AB-E2-89-A4-C3-8F-9A-C0-F7-28-1D-A3-14-72-1E-AF-10-93-AD-19-41-A7-2A-13-50-E8-2B-9A-72-1C-91-E4-40-28-7E-DA-A7-63-8E-E6-0F-16-27-7C-C0-9E-A6-AE-C2-B6-62-88-0A-CD-13-73-C4-BA-A9-E3-42-43-81-EC-29-8D-66-97-AB-26-1C-84-77-D3-5D-5E-86-EA-F9-F2-4B-7E-39-22-57-A4-18-2F-97-D8-FA-F3-3B-F2-F5
SFI 4, Record 3
Error: 6A-83-00
SFI 4, Record 4
Error: 6A-83-00

# Get processing options

https://stackoverflow.com/questions/23590256/getting-parser-error-on-request-for-gpo-command-for-emv-card/23591064#23591064
https://stackoverflow.com/questions/35881046/get-processing-options-response

# pin out

SCK
MISO
-MOSI/SDA/TX to PIN3 I2C(SDA) on RPi
-NSS/SCL/RX to PIN5 I2C(SCL) on RPi
-IRQ 
-RST
-GND to PIN4 GND on RPi
-5V to PIN5 5V on RPi

Test libnfc: http://wiki.sunfounder.cc/index.php?title=PN532_NFC_RFID_Module


# debug info

pi@rpinetcore:~/grove $ nfc-poll
nfc-poll uses libnfc 1.7.1
debug   libnfc.general  log_level is set to 3
debug   libnfc.general  allow_autoscan is set to true
debug   libnfc.general  allow_intrusive_scan is set to false
debug   libnfc.general  1 device(s) defined by user
debug   libnfc.general    #0 name: "_PN532_SPI", connstring: "pn532_spi:/dev/spidev0.0:500000"
debug   libnfc.driver.pn532_spi Attempt to open: /dev/spidev0.0 at 500000 Hz.
debug   libnfc.bus.spi  SPI port speed requested to be set to 500000 Hz.
debug   libnfc.bus.spi  ret 0
debug   libnfc.bus.spi  SPI port mode requested to be set to 0.
debug   libnfc.chip.pn53x       Diagnose
debug   libnfc.chip.pn53x       Timeout value: 500
debug   libnfc.bus.spi  RX: 53
debug   libnfc.driver.pn532_spi Got 53 byte from SPI line before wakeup
debug   libnfc.chip.pn53x       SAMConfiguration
debug   libnfc.chip.pn53x       Timeout value: 1000
debug   libnfc.bus.spi  TX: 01 00 00 ff 03 fd d4 14 01 17
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 15
debug   libnfc.bus.spi  RX: 16
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 01 00 00 ff 09 f7 d4 00 00 6c 69 62 6e 66 63 be
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 09
debug   libnfc.bus.spi  RX: f7
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 6c 69 62 6e 66 63
debug   libnfc.bus.spi  RX: bc
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       GetFirmwareVersion
debug   libnfc.bus.spi  TX: 01 00 00 ff 02 fe d4 02 2a
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 06
debug   libnfc.bus.spi  RX: fa
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 03
debug   libnfc.bus.spi  RX: 32
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 01 06 07
debug   libnfc.bus.spi  RX: e8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       SetParameters
debug   libnfc.bus.spi  TX: 01 00 00 ff 03 fd d4 12 14 06
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 13
debug   libnfc.bus.spi  RX: 18
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.general  "pn532_spi:/dev/spidev0.0" (pn532_spi:/dev/spidev0.0:500000) has been claimed.
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.spi  TX: 01 00 00 ff 0c f4 d4 06 63 02 63 03 63 0d 63 38 63 3d b0
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 07
debug   libnfc.bus.spi  RX: f9
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 07
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 00 00
debug   libnfc.bus.spi  RX: 24
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxMode (Defines the transmission data rate and framing during transmission)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_RxMode (Defines the transmission data rate and framing during receiving)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.spi  TX: 01 00 00 ff 08 f8 d4 08 63 02 80 63 03 80 59
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 09
debug   libnfc.bus.spi  RX: 22
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.spi  TX: 01 00 00 ff 04 fc d4 32 01 00 f9
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 33
debug   libnfc.bus.spi  RX: f8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.spi  TX: 01 00 00 ff 04 fc d4 32 01 01 f8
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 33
debug   libnfc.bus.spi  RX: f8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.spi  TX: 01 00 00 ff 06 fa d4 32 05 ff ff ff f8
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 33
debug   libnfc.bus.spi  RX: f8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
NFC reader: pn532_spi:/dev/spidev0.0 opened
NFC device will poll during 30000 ms (20 pollings of 300 ms for 5 modulations)
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.spi  TX: 01 00 00 ff 0e f2 d4 06 63 02 63 03 63 05 63 38 63 3c 63 3d 19
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 08
debug   libnfc.bus.spi  RX: f8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 07
debug   libnfc.bus.spi  RX: 80
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 80 00 00 00 00
debug   libnfc.bus.spi  RX: 24
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxAuto (Controls the settings of the antenna driver)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_Control (Contains miscellaneous control bits)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.spi  TX: 01 00 00 ff 08 f8 d4 08 63 05 40 63 3c 10 cd
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
^Cdebug libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 09
debug   libnfc.bus.spi  RX: 22
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       InAutoPoll
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.spi  TX: 01 00 00 ff 0a f6 d4 60 14 02 20 10 03 11 12 04 5c
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
error   libnfc.driver.pn532_spi Unable to wait for SPI data. (RX)
nfc_initiator_poll_target: Operation Aborted
debug   libnfc.chip.pn53x       InRelease
debug   libnfc.bus.spi  TX: 01 00 00 ff 03 fd d4 52 00 da
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
^Xdebug libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 03
debug   libnfc.bus.spi  RX: fd
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 53
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.spi  TX: 01 00 00 ff 04 fc d4 32 01 00 f9
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 02
debug   libnfc.bus.spi  RX: fe
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 33
debug   libnfc.bus.spi  RX: f8
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
debug   libnfc.chip.pn53x       PowerDown
debug   libnfc.bus.spi  TX: 01 00 00 ff 03 fd d4 16 f0 26
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 02
debug   libnfc.bus.spi  RX: 01
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00 00 ff 03
debug   libnfc.bus.spi  RX: fd
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: d5
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 17
debug   libnfc.bus.spi  RX: 00
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 14
debug   libnfc.bus.spi  TX: 03
debug   libnfc.bus.spi  RX: 00
pi@rpinetcore:~/grove $

# Serial

pi@rpinetcore:~/grove $ sudo nano /etc/nfc/libnfc.conf
pi@rpinetcore:~/grove $ nfc-list
nfc-list uses libnfc 1.7.1
nfc-list: ERROR: Unable to open NFC device: pn532_hsu:/dev/ttyS0
pi@rpinetcore:~/grove $ sudo nano /etc/nfc/libnfc.conf
pi@rpinetcore:~/grove $
pi@rpinetcore:~/grove $ nfc-list
nfc-list uses libnfc 1.7.1
NFC device: pn532_uart:/dev/ttyS0 opened
pi@rpinetcore:~/grove $ nfc-poll
nfc-poll uses libnfc 1.7.1
NFC reader: pn532_uart:/dev/ttyS0 opened
NFC device will poll during 30000 ms (20 pollings of 300 ms for 5 modulations)
^Cnfc_initiator_poll_target: Operation Aborted
pi@rpinetcore:~/grove $ sudo nano /etc/nfc/libnfc.conf
pi@rpinetcore:~/grove $ nfc-list
debug   libnfc.general  log_level is set to 3
debug   libnfc.general  allow_autoscan is set to true
debug   libnfc.general  allow_intrusive_scan is set to false
debug   libnfc.general  1 device(s) defined by user
debug   libnfc.general    #0 name: "_PN532_UART", connstring: "pn532_uart:/dev/ttyS0"
nfc-list uses libnfc 1.7.1
debug   libnfc.general  0 device(s) found using acr122_usb driver
debug   libnfc.general  0 device(s) found using pn53x_usb driver
debug   libnfc.driver.pn532_uart        Attempt to open: /dev/ttyS0 at 115200 bauds.
debug   libnfc.bus.uart Serial port speed requested to be set to 115200 bauds.
debug   libnfc.chip.pn53x       Diagnose
debug   libnfc.chip.pn53x       Timeout value: 500
debug   libnfc.bus.uart TX: 55 55 00 00 00 00 00 00 00 00 00 00 00 00 00 00
debug   libnfc.chip.pn53x       SAMConfiguration
debug   libnfc.chip.pn53x       Timeout value: 1000
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 14 01 17 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 15
debug   libnfc.bus.uart RX: 16 00
debug   libnfc.bus.uart TX: 00 00 ff 09 f7 d4 00 00 6c 69 62 6e 66 63 be 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 09 f7
debug   libnfc.bus.uart RX: d5 01
debug   libnfc.bus.uart RX: 00 6c 69 62 6e 66 63
debug   libnfc.bus.uart RX: bc 00
debug   libnfc.chip.pn53x       GetFirmwareVersion
debug   libnfc.bus.uart TX: 00 00 ff 02 fe d4 02 2a 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 06 fa
debug   libnfc.bus.uart RX: d5 03
debug   libnfc.bus.uart RX: 32 01 06 07
debug   libnfc.bus.uart RX: e8 00
debug   libnfc.chip.pn53x       SetParameters
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 12 14 06 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 13
debug   libnfc.bus.uart RX: 18 00
debug   libnfc.general  "pn532_uart:/dev/ttyS0" (pn532_uart:/dev/ttyS0) has been claimed.
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 0c f4 d4 06 63 02 63 03 63 0d 63 38 63 3d b0 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 07 f9
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 00 00 00 00 00
debug   libnfc.bus.uart RX: 24 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxMode (Defines the transmission data rate and framing during transmission)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_RxMode (Defines the transmission data rate and framing during receiving)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 08 63 02 80 63 03 80 59 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 09
debug   libnfc.bus.uart RX: 22 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 00 f9 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 01 f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
NFC device: pn532_uart:/dev/ttyS0 opened
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 0e f2 d4 06 63 02 63 03 63 05 63 38 63 3c 63 3d 19 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 08 f8
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 80 80 00 00 00 00
debug   libnfc.bus.uart RX: 24 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxAuto (Controls the settings of the antenna driver)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_Control (Contains miscellaneous control bits)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 08 63 05 40 63 3c 10 cd 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 09
debug   libnfc.bus.uart RX: 22 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 4a 01 00 e1 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 09 f7 d4 4a 01 01 00 ff ff 01 00 e1 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 09 f7 d4 4a 01 02 00 ff ff 01 00 e0 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 05 fb d4 4a 01 03 00 de 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 06 63 02 63 03 5b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 04 fc
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 83 83
debug   libnfc.bus.uart RX: 1e 00
debug   libnfc.chip.pn53x       InCommunicateThru
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 42 01 0b 3f 80 1f 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 43
debug   libnfc.bus.uart RX: 01
debug   libnfc.bus.uart RX: e7 00
debug   libnfc.chip.pn53x       Chip error: "Timeout" (01), returned error: "RF Transmission Error" (-20))
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 06 63 02 63 03 5b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 04 fc
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 83 83
debug   libnfc.bus.uart RX: 1e 00
debug   libnfc.chip.pn53x       InCommunicateThru
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 42 06 00 e4 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 43
debug   libnfc.bus.uart RX: 01
debug   libnfc.bus.uart RX: e7 00
debug   libnfc.chip.pn53x       Chip error: "Timeout" (01), returned error: "RF Transmission Error" (-20))
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 06 63 02 63 03 5b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 04 fc
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 83 83
debug   libnfc.bus.uart RX: 1e 00
debug   libnfc.chip.pn53x       InCommunicateThru
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 42 10 da 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 43
debug   libnfc.bus.uart RX: 01
debug   libnfc.bus.uart RX: e7 00
debug   libnfc.chip.pn53x       Chip error: "Timeout" (01), returned error: "RF Transmission Error" (-20))
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 4a 01 04 dd 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InRelease
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 52 00 da 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 53
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: d8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 00 f9 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       PowerDown
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 16 f0 26 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 17
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: 14 00
pi@rpinetcore:~/grove $ ^C
pi@rpinetcore:~/grove $


debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
nfc_initiator_target_is_present: Target Released
Waiting for card removing...done.
debug   libnfc.chip.pn53x       InRelease
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 52 00 da 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 53
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: d8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 00 f9 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       PowerDown
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 16 f0 26 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 17
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: 14 00
pi@rpinetcore:~ $



pi@rpinetcore:~ $ nfc-poll
nfc-poll uses libnfc 1.7.1
debug   libnfc.general  log_level is set to 3
debug   libnfc.general  allow_autoscan is set to true
debug   libnfc.general  allow_intrusive_scan is set to false
debug   libnfc.general  1 device(s) defined by user
debug   libnfc.general    #0 name: "_PN532_UART", connstring: "pn532_uart:/dev/ttyS0"
debug   libnfc.driver.pn532_uart        Attempt to open: /dev/ttyS0 at 115200 bauds.
debug   libnfc.bus.uart Serial port speed requested to be set to 115200 bauds.
debug   libnfc.chip.pn53x       Diagnose
debug   libnfc.chip.pn53x       Timeout value: 500
debug   libnfc.bus.uart TX: 55 55 00 00 00 00 00 00 00 00 00 00 00 00 00 00
debug   libnfc.chip.pn53x       SAMConfiguration
debug   libnfc.chip.pn53x       Timeout value: 1000
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 14 01 17 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 15
debug   libnfc.bus.uart RX: 16 00
debug   libnfc.bus.uart TX: 00 00 ff 09 f7 d4 00 00 6c 69 62 6e 66 63 be 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 09 f7
debug   libnfc.bus.uart RX: d5 01
debug   libnfc.bus.uart RX: 00 6c 69 62 6e 66 63
debug   libnfc.bus.uart RX: bc 00
debug   libnfc.chip.pn53x       GetFirmwareVersion
debug   libnfc.bus.uart TX: 00 00 ff 02 fe d4 02 2a 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 06 fa
debug   libnfc.bus.uart RX: d5 03
debug   libnfc.bus.uart RX: 32 01 06 07
debug   libnfc.bus.uart RX: e8 00
debug   libnfc.chip.pn53x       SetParameters
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 12 14 06 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 13
debug   libnfc.bus.uart RX: 18 00
debug   libnfc.general  "pn532_uart:/dev/ttyS0" (pn532_uart:/dev/ttyS0) has been claimed.
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 0c f4 d4 06 63 02 63 03 63 0d 63 38 63 3d b0 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 07 f9
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 00 00 00 00 00
debug   libnfc.bus.uart RX: 24 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxMode (Defines the transmission data rate and framing during transmission)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_RxMode (Defines the transmission data rate and framing during receiving)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 08 63 02 80 63 03 80 59 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 09
debug   libnfc.bus.uart RX: 22 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 00 f9 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 01 f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
NFC reader: pn532_uart:/dev/ttyS0 opened
NFC device will poll during 30000 ms (20 pollings of 300 ms for 5 modulations)
debug   libnfc.chip.pn53x       ReadRegister
debug   libnfc.bus.uart TX: 00 00 ff 0e f2 d4 06 63 02 63 03 63 05 63 38 63 3c 63 3d 19 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 08 f8
debug   libnfc.bus.uart RX: d5 07
debug   libnfc.bus.uart RX: 80 80 00 00 00 00
debug   libnfc.bus.uart RX: 24 00
debug   libnfc.chip.pn53x       PN53X_REG_CIU_TxAuto (Controls the settings of the antenna driver)
debug   libnfc.chip.pn53x       PN53X_REG_CIU_Control (Contains miscellaneous control bits)
debug   libnfc.chip.pn53x       WriteRegister
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 08 63 05 40 63 3c 10 cd 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 09
debug   libnfc.bus.uart RX: 22 00
debug   libnfc.chip.pn53x       InAutoPoll
debug   libnfc.chip.pn53x       No timeout
debug   libnfc.bus.uart TX: 00 00 ff 0a f6 d4 60 14 02 20 10 03 11 12 04 5c 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0e f2
debug   libnfc.bus.uart RX: d5 61
debug   libnfc.bus.uart RX: 01 10 09 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 49 00
ISO/IEC 14443A (106 kbps) target:
    ATQA (SENS_RES): 00  04
       UID (NFCID1): 27  8b  70  34
      SAK (SEL_RES): 08
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 0c f4
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 01 01 00 04 08 04 27 8b 70 34
debug   libnfc.bus.uart RX: 78 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       target_is_present(): Ping MFC
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 00 01 02 f2 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       InListPassiveTarget
debug   libnfc.chip.pn53x       Timeout value: 300
debug   libnfc.bus.uart TX: 00 00 ff 08 f8 d4 4a 01 00 27 8b 70 34 8b 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 4b
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: e0 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 06 fa d4 32 05 ff ff ff f8 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
nfc_initiator_target_is_present: Target Released
Waiting for card removing...done.
debug   libnfc.chip.pn53x       InRelease
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 52 00 da 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 53
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: d8 00
debug   libnfc.chip.pn53x       RFConfiguration
debug   libnfc.bus.uart TX: 00 00 ff 04 fc d4 32 01 00 f9 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 02 fe
debug   libnfc.bus.uart RX: d5 33
debug   libnfc.bus.uart RX: f8 00
debug   libnfc.chip.pn53x       PowerDown
debug   libnfc.bus.uart TX: 00 00 ff 03 fd d4 16 f0 26 00
debug   libnfc.bus.uart RX: 00 00 ff 00 ff 00
debug   libnfc.chip.pn53x       PN53x ACKed
debug   libnfc.bus.uart RX: 00 00 ff 03 fd
debug   libnfc.bus.uart RX: d5 17
debug   libnfc.bus.uart RX: 00
debug   libnfc.bus.uart RX: 14 00
pi@rpinetcore:~ $

using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        ParsePassword("TM-CQZT-+?!HZDKIRDSTPR+E");
    }

    const string Alphabet = "-ABCDEFGHIJKLMNOPQRSTUVWXYZ!?#+=";
    const int OffsetChecksum = 116;
    const int OffsetRotation = 112;

    public static void ParsePassword (string phrase) {
        char[] chars = phrase.ToCharArray();
        byte[] charBytes = new byte[24];
        for (int i = 0; i < chars.Length; i++) {
            charBytes[i] = (byte)Alphabet.IndexOf(chars[i]);
        }

        int[] unpackedBits = new int[charBytes.Length * 5];
        for (int i = 0; i < unpackedBits.Length; i++)
        {
            unpackedBits[i] = ((charBytes[i / 5] >> (i % 5)) & 0x1);
        }

        int runlength = GetValueFromBitStream(unpackedBits, OffsetRotation, 4);
        int checksum = GetValueFromBitStream(unpackedBits, OffsetChecksum, 4);

        WriteValueToBitStream(unpackedBits, 0, OffsetChecksum, 4);
        WriteValueToBitStream(unpackedBits, 0, OffsetRotation, 4);

        int rotationLength = unpackedBits.Length - 8;
        int[] rotatedBits = new int[rotationLength];

        byte runLengthCounter = 0;

        for (int i = 0; i < rotationLength; i++) {
            rotatedBits[i] = unpackedBits[(i + (rotationLength - RunLengthTable[runlength])) % (rotationLength)];
            runLengthCounter = (byte)((runLengthCounter + rotatedBits[i]) & 0xFF);
        }

        {
            byte thisCheckSum = (byte)((runLengthCounter >> 4) & 0xF);
            thisCheckSum = (byte)((runLengthCounter ^ ~thisCheckSum) & 0xF);
            if (checksum != thisCheckSum)
                throw new InvalidOperationException("Checksum does not match");
        }
        runLengthCounter &= 0xf;
    }

    static int[] GetBitStreamFromValue (int value, int length) {
        int[] output = new int[length];
        for (int i = 0; i < length; i++) {
            output[i] = (value >> i) & 0x1;
        }
        return output;
    }

    static int GetValueFromBitStream (int[] bits, int start, int length) {
        int output = 0;
        for (int i = 0; i < length; i++) {
            output |= bits[start + i] << i;
        }
        return output;
    }

    static void WriteValueToBitStream (int[] bits, int value, int start, int length) {
        for (int i = 0; i < length; i++) {
            bits[i + start] = (value >> i) & 0x1;
        }
    }

    static readonly int[] RunLengthTable = new int[]
        {
            81,      91,      101,      31,     37,     41,     43,     47,    53,
            7,     11,     13,     17,     19,     23,     29,             
        };
}
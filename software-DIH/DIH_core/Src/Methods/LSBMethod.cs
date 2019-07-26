using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DIH_core.Src.Methods
{
    public class LSBMethod : MethodBase, IHidingMethod
    {
        private static readonly int[] bitsPerSampleConstraints = new int[]{ 1, 2, 4, 8 };

        private int bitsPerSampleCoding = 1;
        [HidingMethodParam(
            Type = HidingMethodParamAttribute.ParamType.Set, 
            Set = new int[] { 1,2,4,8 }, 
            Description = "Defines how many last significant bits of sample will be used."
            )]
        public int BitsPerSampleCoding
        {
            get { return bitsPerSampleCoding; }
            set {
                if (bitsPerSampleConstraints.Contains(value))
                    bitsPerSampleCoding = value;
                else
                    throw new ApplicationException("Attempt to set invalid value for BitsPerSampleCoding paramether.");
            }
        }

        public long GetAvailableSpace(DigitalAudio cover)
        {
            var additionalInfo =(StegoMark.Length
                + msgSizeLength); //for message size
            return cover.SamplesInChannel / (8 / bitsPerSampleCoding) - additionalInfo;
        }

        public DigitalAudio Embed(DigitalAudio audio, byte[] message)
        {
            var signal = audio.GetSignal();
            if (GetAvailableSpace(audio) < message.Length)
                throw new ApplicationException("Attempt to insert a message that is longer than the container covers.");

            //data
            var embeddedMessage = CreateEmbeddedMessage(message);
            int samplei = 0;
            int mask = ~((1 << bitsPerSampleCoding) - 1);
            int length = embeddedMessage.Length;
            for (int i = 0; i < length;)
            {
                signal[samplei] &= mask;
                for (int j = 0; j < bitsPerSampleCoding; j++)
                    signal[samplei] |= ((embeddedMessage[i++] ? 1 : 0) << j);
                samplei++;
            }
            return audio;
        }

        private byte[] ReadBits(int[] signal, int offset, long length)
        {
            byte[] data = new byte[length];
            int index = offset;
            int mask = (1 << bitsPerSampleCoding) - 1;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < bitsInByte; j+= bitsPerSampleCoding)
                    data[i] |= (byte)((signal[index++] & mask) << j);
            return data;
        }
        public byte[] Extract(DigitalAudio audio)
        {
            var signal = audio.GetSignal();
            byte[] stegoMark = ReadBits(signal, 0, StegoMark.LongLength);
            if (!stegoMark.SequenceEqual(StegoMark))
                return null;
            byte[] messageSize = ReadBits(signal, (StegoMark.Length * bitsInByte)/ bitsPerSampleCoding, msgSizeLength);
            if (!BitConverter.IsLittleEndian)
                messageSize.Reverse();
            long length = BitConverter.ToInt64(messageSize, 0);
            byte[] message = ReadBits(signal, ((StegoMark.Length + msgSizeLength) * bitsInByte) / bitsPerSampleCoding, length);
            return message;
        }
        public string Description()
        {
            return "LSB description";
        }
    }
}

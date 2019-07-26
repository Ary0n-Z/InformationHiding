using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH_core.Src.Methods
{
    class ParityCoding : MethodBase, IHidingMethod
    {
        [HidingMethodParam(
            Type = HidingMethodParamAttribute.ParamType.IntValue,
            NotInEmbed = true,
            Description = "Message length, it need for calculating frame size of the segment."
            )]
        public int MessageLength { get; set; } = 0;
        public string Description()
        {
            return "Parity coding";
        }
        private byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        public DigitalAudio Embed(DigitalAudio cover, byte[] message)
        {
            var coverSignal = cover.GetSignal();
            BitArray embeddingMessage = CreateEmbeddedMessage(message);
            int FrameSize = (coverSignal.Length / embeddingMessage.Length);
            int parityBit = 0;
            int iframe = 0;
            for (int b = 0; b < embeddingMessage.Length; b++)
            {
                for (int i = 0; i < FrameSize; i++)
                    if ((coverSignal[iframe + i] & 1) == 1)
                        parityBit++;
                parityBit %= 2;

                if ((embeddingMessage[b] ? 1:0) != parityBit)
                    coverSignal[iframe] ^= 1;
                iframe += FrameSize;
                parityBit = 0;
            }
            return cover;
        }
        private byte[] ExtractBytes(int[] signal,int fromi,int bytesCount,int FrameSize)
        {
            List<bool> message = new List<bool>();
            int iframe = fromi;
            int parityBit = 0;

            for (int i = 0; i < bytesCount*8; i++)
            {
                for (int j = 0; j < FrameSize; j++)
                    if ((signal[iframe + j] & 1) == 1)
                        parityBit+=1;
                parityBit %= 2;
                message.Add(parityBit == 1);
                parityBit = 0;
                iframe += FrameSize;
            }
            return BitArrayToByteArray(new BitArray(message.ToArray()));
        }
        public byte[] Extract(DigitalAudio stegoAudio)
        {
            int[] signal = stegoAudio.GetSignal();
            int FrameSize = signal.Length / ((MessageLength + StegoMark.Length + msgSizeLength)*8);
            byte[] stegoMark = ExtractBytes(signal, 0, StegoMark.Length,FrameSize);
            if (!stegoMark.SequenceEqual(StegoMark))
                return null;
            byte[] messageSize = ExtractBytes(signal, StegoMark.Length * 8 * FrameSize, msgSizeLength,FrameSize);
            if (!BitConverter.IsLittleEndian)
                messageSize.Reverse();
            long length = BitConverter.ToInt64(messageSize, 0);
            byte[] message = ExtractBytes(signal, (msgSizeLength + StegoMark.Length) * 8 * FrameSize, (int)length,FrameSize);
            return message;
        }

        public long GetAvailableSpace(DigitalAudio stegoAudio)
        {
            return stegoAudio.SamplesInChannel/8 - (StegoMark.Length + msgSizeLength);
        }
    }
}

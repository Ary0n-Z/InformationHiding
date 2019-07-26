

using System;
using System.IO;

namespace DIH_core.Src.WAV
{
    public class WavWriter: BinaryWriter
    {
        private static readonly byte[] ID_RIFF = { 82, 73, 70, 70 };
        private static readonly byte[] ID_WAVE = { 87, 65, 86, 69 };
        private static readonly byte[] ID_fmt_ = { 102, 109, 116, 32 };
        private static readonly byte[] ID_data = { 100, 97, 116, 97 };
        public WavWriter(Stream stream)
            :base(stream)
        {
        }
        public void WriteWavDefault(DigitalAudio audio)
        {
            WaveFormat format = new WaveFormat();
            format.SectionSize = 16;
            format.NumberOfChannels = audio.NumOfChannels;
            format.CompressionCode = 1;
            format.SampleRate = audio.SampleRate;
            format.SignificantBitsPerSample = audio.SignificantBitsPerSample;
            format.BlockAlign = (ushort)((format.SignificantBitsPerSample * format.NumberOfChannels) / 8);
            format.AvarageBytesPerSecond = format.SampleRate * format.BlockAlign;
            var dataSize = audio.DataSize * (audio.SignificantBitsPerSample / 8);
            uint data_Size = (uint)((dataSize % 2 == 0) ? dataSize : dataSize + 1);
            uint RIFF_Size = 
                + 4/*RIFF Type = 'WAVE'*/
                +8/*fmt  section*/
                + format.SectionSize
                + 8/*data  section*/
                + data_Size;
            BaseStream.SetLength(RIFF_Size + 8);// + 8 RIFF section
            Write(ID_RIFF);
            Write(RIFF_Size);
            Write(ID_WAVE);
            Write(ID_fmt_);
            Write(format.SectionSize);
            Write(format.CompressionCode);
            Write(format.NumberOfChannels);
            Write(format.SampleRate);
            Write(format.AvarageBytesPerSecond);
            Write(format.BlockAlign);
            Write(format.SignificantBitsPerSample);

            Write(ID_data);
            Write(data_Size);
            int[][] channels = audio.GetAllSignals();
            for (int i = 0; i < audio.SamplesInChannel; i++)
                for (int j = 0; j < audio.NumOfChannels; j++)
                    Write((short)channels[j][i]);
        }
    }
}

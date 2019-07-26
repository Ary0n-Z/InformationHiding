using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DIH_core.Src.WAV
{
    public class WavReader : BinaryReader
    {
        private const int Chunk_ID_Size = 4;
        private byte[] tmpChunkID = new byte[Chunk_ID_Size];
        private bool ReadExpectedChunkID(byte[] expectedID)
        {
            Read(tmpChunkID, 0, Chunk_ID_Size);
            return tmpChunkID.SequenceEqual(expectedID);
        }

        private static readonly byte[] ID_WAVE = { 87, 65, 86, 69 }; // "WAVE"
        private static readonly byte[] ID_fmt_ = { 102, 109, 116, 32 }; // "fmt "
        private static readonly byte[] ID_data = { 100, 97, 116, 97 }; // "data"
        private const ushort PCMUncompressed = 0x0001;
        private WaveFormat format = new WaveFormat();
        public WavReader(Stream stream) : base(stream)
        {
            if (!BaseStream.CanRead)
                throw new ApplicationException("Can't read from the stream.");
            BaseStream.Position = 0;
            BaseStream.Seek(0x08, SeekOrigin.Begin); //Skip RIFF ID section
            //[RIFF ID] 'WAVE' expected
            if (!ReadExpectedChunkID(ID_WAVE))
                throw new ApplicationException("RIFF Type ID is not found. 'WAVE' expected.");
            //[Format section]
            //-- 'fmt ' header
            if (!ReadExpectedChunkID(ID_fmt_))
                throw new ApplicationException("'fmt ' section is missing.");
            format.SectionSize = ReadUInt32();
            //-- 'fmt ' data
            format.CompressionCode = ReadUInt16();
            if (format.CompressionCode != PCMUncompressed)
                throw new ApplicationException("Unsupported compresion. PCM/uncompressed expected.");
            format.NumberOfChannels = ReadUInt16();
            format.SampleRate = ReadUInt32();
            format.AvarageBytesPerSecond = ReadUInt32();
            format.BlockAlign = ReadUInt16();
            format.SignificantBitsPerSample = ReadUInt16();
            if (format.SignificantBitsPerSample != 16)
                throw new ApplicationException("Unsupported significant bits per sample. 16 bits expected.");
            BaseStream.Position = 0;
        }
        public DigitalAudio ReadAudio()
        {
            int[][] channels = null;
            DigitalAudio audio = new DigitalAudio();
            audio.SampleRate = format.SampleRate;
            audio.SignificantBitsPerSample = format.SignificantBitsPerSample;
            channels = ReadPCMData();
            for (uint i = 0; i < format.NumberOfChannels; i++)
                audio.AddChannel(channels[i]);
            return audio;
        }
        private void SkipChunk()
        {
            int chunkSize = ReadInt32();
            BaseStream.Seek(chunkSize, SeekOrigin.Current);
        }
        private int[][] ReadPCMData()
        {
            //Move to the data section
            //RIFF header 4 + 4
            //RIFF Type +4
            //FORMAT header 4 + 4 = 20
            BaseStream.Seek(format.SectionSize + 20, SeekOrigin.Begin);
            //[Data section]
            //-- 'data' header
            while (!ReadExpectedChunkID(ID_data))
            {
                SkipChunk();
                if (!BaseStream.CanRead)
                    throw new ApplicationException("'data' section is missing.");
            }
            uint dataSize = ReadUInt32();
            if (dataSize == 0)
                throw new ApplicationException("No PCM data in the 'data' section.");
            //-- 'data' data
            ushort bytesPerSample = (ushort)(format.SignificantBitsPerSample / 8);
            long channelSize = dataSize / (format.NumberOfChannels * bytesPerSample);
            if (channelSize > int.MaxValue)
            {
                string msg = "Audio file is too big!. Max possible size is {0} bytes";
                msg = string.Format(msg, format.SectionSize + 12 + int.MaxValue);
                throw new ApplicationException(msg);
            }
            // Memmory alloc
            int[][] channels = new int[format.NumberOfChannels][];
            for (int i = 0; i < format.NumberOfChannels; i++)
                channels[i] = new int[channelSize];
            // Read all channels
            for (int i = 0; i < channelSize; i++)
                for (int j = 0; j < format.NumberOfChannels; j++)
                    channels[j][i] = ReadInt16();
            BaseStream.Position = 0;
            return channels;
        }
    }
}

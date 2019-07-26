using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DIH_core.Src.Methods
{
    class PhaseCoding : MethodBase, IHidingMethod
    {
        private bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        public string Description()
        {
            return "Phase Method description";
        }
        //public enum FrequencyEncode { Hi, Low }
        //[HidingMethodParam(
        //    Description = "Define from which frequency starts coding."
        //    , Type = HidingMethodParamAttribute.ParamType.EnumValues
        //    , EnumValues = new string[] { "Hi", "Low" }
        //    , EnumType = typeof(FrequencyEncode))]
        //public FrequencyEncode FrequencyEncodeStart { get; set; } = FrequencyEncode.Hi;
        private int segmentLength = 4;

        [HidingMethodParam(Description = "Define size of the segments for FFT and message capacity. Must be grather then 4 and be a power of 2.",
            Type = HidingMethodParamAttribute.ParamType.ConstrValue)]
        public int SegmentLength
        {
            get
            {
                return segmentLength;
            }
            set
            {
                if (!IsPowerOfTwo((ulong)value))
                    throw new ApplicationException("Segment length must be power of 2.");
                if (value < 4)
                    throw new ApplicationException("Segment length cant be less then 4.");
                segmentLength = value;
            }
        }
        public DigitalAudio Embed(DigitalAudio cover, byte[] message)
        {
            BitArray embeddingMessage = CreateEmbeddedMessage(message);
            var msgLength = embeddingMessage.Length;
            // Create embeddingMessage
            var value = (float)(Math.PI / 2.0);
            float[] embeddingData = new float[msgLength];
            for (int i = 0; i < msgLength; i++)
                if (embeddingMessage[i])
                    embeddingData[i] = -value;
                else
                    embeddingData[i] = value;

            // Signal
            var coverSignal = cover.GetSignal();
            var ComplexSignal = coverSignal.Select(x => new Complex32(x, 0)).ToArray();
           
            double ratio = cover.SamplesInChannel / segmentLength;
            int N = (int)Math.Floor(ratio); //Segmnets count
            //---------------------------------------------
            List<Complex32[]> signalSegments = new List<Complex32[]>(N);
            List<float[]> phases = new List<float[]>(N);
            List<float[]> magnitude = new List<float[]>(N);
            List<float[]> deltaPhases = new List<float[]>(N);
            for (int seg = 0; seg < N; seg++)
            {
                //----------Create-----
                signalSegments.Add(new Complex32[segmentLength]);
                phases.Add(new float[segmentLength]);
                magnitude.Add(new float[segmentLength]);
                deltaPhases.Add(new float[segmentLength]);
                //---------------------
                //---Segments init---
                Array.Copy(ComplexSignal, seg * segmentLength, signalSegments[seg], 0, segmentLength); // Signal copy
                //---------------------
                //-------FFT----------
                Fourier.Forward(signalSegments[seg], FourierOptions.Matlab); // Signal transform for each segment
                //--------------------
                for (int j = 0; j < segmentLength; j++)
                {
                    phases[seg][j] = signalSegments[seg][j].Phase; //Phases for each segment
                    magnitude[seg][j] = signalSegments[seg][j].Magnitude; //Magnitude for each segment
                }
            }

            var spectrumMiddle = segmentLength / 2;

            // Delta phases
            for (int seg = 1; seg < N; seg++)
                for (int j = 0; j < segmentLength; j++)
                    deltaPhases[seg][j] = phases[seg][j] - phases[seg - 1][j];

            int startIndex = spectrumMiddle - 1;
            int startSymmetryIndex = spectrumMiddle + 1;

            for (int i = 0;i < msgLength; i++)
            {
                phases[0][startIndex - i] = embeddingData[i];
                phases[0][startSymmetryIndex + i] = -embeddingData[i]; // symmetry
            }
            // New phases
            for (int seg = 1; seg < N; seg++)
                for (int j = 0; j < segmentLength; j++)
                    phases[seg][j] = phases[seg - 1][j] + deltaPhases[seg][j];

            //Restore signal
            for (int seg = 0; seg < N; seg++)
            {
                for (int j = 0; j < segmentLength; j++)
                {
                    var A = magnitude[seg][j];
                    var phase = phases[seg][j];
                    signalSegments[seg][j] = Complex32.FromPolarCoordinates(A, phase);
                }
                Fourier.Inverse(signalSegments[seg], FourierOptions.Matlab);
            }

            for (int i = 0; i < N; i++)
                for (int j = 0; j < segmentLength; j++)
                    coverSignal[segmentLength * i + j] = (int)signalSegments[i][j].Real;

            return cover;
        }
        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        private byte[] ExtractRange(Complex32[] fourierImage,List<bool> output,int from,long to)
        {
            output.Clear();
            var value = Math.PI / 3.0;
            for (int i = from; i > to; i--)
            {
                if (fourierImage[i].Phase > value)
                    output.Add(false);
                if (fourierImage[i].Phase < -value)
                    output.Add(true);
            }
            return BitArrayToByteArray(new BitArray(output.ToArray()));
        }
        public byte[] Extract(DigitalAudio stegoAudio)
        {
            var stegoSignal = stegoAudio.GetSignal();
            Complex32[] segment = new Complex32[segmentLength];
            for (int i = 0; i < segmentLength; i++)
                segment[i] = new Complex32(stegoSignal[i], 0);
            Fourier.Forward(segment, FourierOptions.Matlab);
            //Check if has label            
            var headerLen = bitsInByte * StegoMark.Length;

            List<bool> tmp = new List<bool>(headerLen);
            //Read Header
            var startIndex = segmentLength / 2 - 1;
            long endIndex = startIndex - headerLen;
            byte[] bytesData = ExtractRange(segment, tmp, startIndex, endIndex);
            if (!bytesData.SequenceEqual(StegoMark))
                return null;
            // Read msg size
            var msgSizeLen = msgSizeLength * bitsInByte;
            startIndex = (int)endIndex;
            endIndex = startIndex - msgSizeLen;
            bytesData = ExtractRange(segment, tmp, startIndex, endIndex);
            if (!BitConverter.IsLittleEndian)
                bytesData.Reverse();
            long msgLength = BitConverter.ToInt64(bytesData, 0) * bitsInByte;
            //Read Data
            startIndex = (int)endIndex;
            endIndex = startIndex - msgLength;
            return ExtractRange(segment, tmp, startIndex, endIndex);
        }
        public long GetAvailableSpace(DigitalAudio stegoAudio)
        {
            return ((segmentLength / 2 )- 1 - (StegoMark.Length + msgSizeLength) * bitsInByte)/ bitsInByte;
        }
    }
}

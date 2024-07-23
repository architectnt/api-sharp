using System;
using System.Runtime.InteropServices;

namespace ArchitectAPI.Wrappers.Audio
{
    public unsafe partial class ArchitectAudioClipFormat
    {
        public const string AAFCPATH = "aafc";

        /// <summary>
        /// AAFC Header structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFC_HEADER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
            public string headr;

            public uint version;
            public uint freq;
            public byte channels;
            public uint samplelength;
            public byte bps;
            public byte sampletype;
        }

        /// <summary>
        /// Output struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFCOUTPUT
        {
            public byte* data;
            public nuint size;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFCDECOUTPUT
        {
            public AAFC_HEADER header;
            public float* data;
        }

        /// <summary>
        /// Native AAFC Import
        /// </summary>
        /// <param name="data">The managed data to utilize</param>
        /// <returns>Uncompressed 32 bit float AAFC data</returns>
        [DllImport(AAFCPATH)]
        public static extern AAFCDECOUTPUT aafc_import(byte* data);

        /// <summary>
        /// Native AAFC Export
        /// </summary>
        /// <param name="samples">The raw data to export</param>
        /// <param name="freq">The sample rate</param>
        /// <param name="channels">Number of channels to speakers</param>
        /// <param name="samplelength">The length of the audio data</param>
        /// <param name="bps">Bits per sample</param>
        /// <param name="sampletype">1 -> PCM, 2 -> ADPCM</param>
        [LibraryImport(AAFCPATH)]
        public static partial AAFCOUTPUT aafc_export(float* samples, uint freq, uint channels, uint samplelength, byte bps = 16, byte sampletype = 1, [MarshalAs(UnmanagedType.Bool)] bool forcemono = false, uint samplerateoverride = 0, [MarshalAs(UnmanagedType.Bool)] bool nm = false, float pitch = 1);

        /// <summary>
        /// Gets the header from AAFC data
        /// </summary>
        /// <param name="data">AAFC Input</param>
        /// <returns>28 byte header or 12 byte legacy header</returns>
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_getheader(byte* data);

        /// <summary>
        /// Natively convert floating point arrays to integer types
        /// </summary>
        /// <param name="arr">The input array</param>
        /// <param name="length">The absolute samplelength</param>
        /// <param name="type">Integer type (8 > byte, 16 > short, 32 > int)</param>
        /// <returns></returns>
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_float_to_int(float* arr, long length, byte type);

        /// <summary>
        /// Natively convert floating point arrays to integer types
        /// </summary>
        /// <param name="arr">The input array</param>
        /// <param name="length">The absolute samplelength</param>
        /// <param name="type">Integer type (8 > byte, 16 > short, 32 > int)</param>
        /// <returns></returns>
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_int_to_float(IntPtr arr, long length, byte type);

        /// <summary>
        /// Resample audio
        /// </summary>
        /// <param name="input"></param>
        /// <param name="samplerateoverride"></param>
        /// <param name="freq"></param>
        /// <param name="channels"></param>
        /// <param name="samplelength"></param>
        /// <returns>Resampled float array</returns>
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_resample_data(float* input, uint samplerateoverride, uint freq, byte channels, uint* samplelength, float pitch = 1);

        /// <summary>
        /// Normalizes the sample array
        /// </summary>
        /// <param name="arr">Array input</param>
        /// <param name="len">Absolute sample length</param>
        /// <returns></returns>
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_normalize(float* arr, int len);


        public unsafe static byte[] ToByteArray(float[] samples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            fixed (float* fptr = samples)
            {
                AAFCOUTPUT rstptr = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[rstptr.size];
                Marshal.Copy((IntPtr)rstptr.data, rst, 0, (int)rstptr.size);
                Marshal.FreeHGlobal((IntPtr)rstptr.data);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(byte[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (byte* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 8), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT rstptr = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[rstptr.size];
                Marshal.Copy((IntPtr)rstptr.data, rst, 0, (int)rstptr.size);
                Marshal.FreeHGlobal((IntPtr)rstptr.data);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(short[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, short pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (short* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 16), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT rstptr = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[rstptr.size];
                Marshal.Copy((IntPtr)rstptr.data, rst, 0, (int)rstptr.size);
                Marshal.FreeHGlobal((IntPtr)rstptr.data);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(int[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, int pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (int* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 32), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT rstptr = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[rstptr.size];
                Marshal.Copy((IntPtr)rstptr.data, rst, 0, (int)rstptr.size);
                Marshal.FreeHGlobal((IntPtr)rstptr.data);
                return rst;
            }
        }

        public unsafe static AAFC_Clip FromByteArray(byte[] bytes, string n)
        {
            fixed (byte* bptr = bytes)
            {
                AAFCDECOUTPUT d = aafc_import(bptr);
                return ((nuint)d.data != nuint.Zero)
                    ? new(n, d.data, d.header.samplelength / d.header.channels, d.header.freq, d.header.channels)
                    : null;
            }
        }

        // read aafc from file system
        public static AAFC_Clip LoadAAFC(string filename)
        {
            return FromByteArray(System.IO.File.ReadAllBytes(filename), System.IO.Path.GetFileNameWithoutExtension(filename));
        }

        public unsafe static byte[] ToByteSamples(AAFC_Clip clip)
        {
            byte[] rst = new byte[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 8), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }

        public unsafe static short[] ToShortSamples(AAFC_Clip clip)
        {
            short[] rst = new short[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 16), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }

        public unsafe static int[] ToIntSamples(AAFC_Clip clip)
        {
            int[] rst = new int[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 32), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }
    }

    public unsafe class AAFC_Clip : IDisposable
    {
        /// <summary>
        /// The name of the clip.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Audio data
        /// </summary>
        public float* Samples { get; private set; }
        /// <summary>
        /// Sample rate
        /// </summary>
        public uint Frequency { get; private set; }
        /// <summary>
        /// Interleaved sample length
        /// </summary>
        public uint SampleLength { get; private set; }
        /// <summary>
        /// Pre-multiplied Sample length
        /// </summary>
        public uint ActualSampleLength { get; private set; }
        /// <summary>
        /// Channels defined
        /// </summary>
        public byte Channels { get; private set; }

        bool disposed = false;

        public AAFC_Clip(string name, float[] samples, uint sampleLength, uint freq, byte channels)
        {
            Name = name;
            Samples = (float*)Marshal.AllocHGlobal(samples.Length * sizeof(float));
            Marshal.Copy(samples, 0, (nint)Samples, samples.Length);
            ActualSampleLength = (uint)samples.Length;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
        }

        public AAFC_Clip(string name, float* samples, uint sampleLength, uint freq, byte channels)
        {
            Name = name;
            Samples = samples;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
            ActualSampleLength = sampleLength * channels;
        }

        // oh
        public void Resample(uint newSampleRate, float pitch = 1)
        {
            if (newSampleRate == Frequency && pitch == 1)
                return;

            float* rsptr = Samples;
            uint newSampleLength = ActualSampleLength;
            IntPtr newSamplesPtr = ArchitectAudioClipFormat.aafc_resample_data(rsptr, newSampleRate, Frequency, Channels, &newSampleLength, pitch);

            if (newSamplesPtr != IntPtr.Zero)
            {
                if (Samples != null)
                    Marshal.FreeHGlobal((IntPtr)Samples);

                Frequency = newSampleRate;
                Samples = (float*)newSamplesPtr;
                ActualSampleLength = newSampleLength;
                SampleLength = ActualSampleLength / Channels;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (Samples != null)
                {
                    Marshal.FreeHGlobal((nint)Samples);
                    Samples = null;
                }

                disposed = true;
            }
        }

        ~AAFC_Clip()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

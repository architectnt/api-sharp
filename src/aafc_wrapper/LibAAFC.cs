using System;
using System.Runtime.InteropServices;

namespace ArchitectAPI.Wrappers.Audio
{
    public unsafe partial class LibAAFC
    {
        public const string AAFCPATH = "aafc";

        /// <summary>
        /// AAFC Header structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFC_HEADER
        {
            public ushort signature;
            public ushort version;
            public uint freq;
            public byte channels;
            public byte bps;
            public byte sampletype;
            public uint samplelength;
            public uint loopst;
            public uint loopend;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFC_LCHEADER
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
            public nuint size;
            public byte* data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFCDECOUTPUT
        {
            public AAFC_HEADER header;
            public float* data;
        }


        [LibraryImport(AAFCPATH)]
        public static partial ushort aafc_getversion();

        /// <summary>
        /// Native AAFC Import
        /// </summary>
        /// <param name="data">The managed data to utilize</param>
        /// <returns>Uncompressed 32 bit float AAFC data</returns>
        [LibraryImport(AAFCPATH)]
        public static partial AAFCDECOUTPUT aafc_import(byte* data);

        /// <summary>
        /// Native AAFC Export
        /// </summary>
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


        public static byte[] Export(float[] samples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            fixed (float* fptr = samples)
            {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(byte[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.LongLength];
            fixed (byte* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 8), samples, 0, isamples.Length);

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(short[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.LongLength];
            fixed (short* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.LongLength, 16), samples, 0, isamples.Length);

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(int[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.LongLength];
            fixed (int* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.LongLength, 32), samples, 0, isamples.Length);

            fixed (float* fptr = samples)
            {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static AudioClip Import(byte[] bytes, string n)
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
        public static AudioClip LoadAAFC(string filename)
        {
            return Import(System.IO.File.ReadAllBytes(filename), System.IO.Path.GetFileNameWithoutExtension(filename));
        }

        public static byte[] ToByteSamples(AudioClip clip)
        {
            byte[] rst = new byte[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 8), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }

        public static short[] ToShortSamples(AudioClip clip)
        {
            short[] rst = new short[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 16), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }

        public static int[] ToIntSamples(AudioClip clip)
        {
            int[] rst = new int[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 32), rst, 0, (int)clip.ActualSampleLength);
            return rst;
        }
    }

    public unsafe class AudioClip : IDisposable
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

        public AudioClip(string name, float[] samples, uint sampleLength, uint freq, byte channels)
        {
            Name = name;
            Samples = (float*)Marshal.AllocHGlobal(samples.Length * sizeof(float));
            Marshal.Copy(samples, 0, (nint)Samples, samples.Length);
            ActualSampleLength = (uint)samples.Length;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
        }

        public AudioClip(string name, float* samples, uint sampleLength, uint freq, byte channels)
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
            IntPtr newSamplesPtr = LibAAFC.aafc_resample_data(rsptr, newSampleRate, Frequency, Channels, &newSampleLength, pitch);

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

        ~AudioClip()
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

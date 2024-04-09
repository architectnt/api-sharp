using System;
using System.Runtime.InteropServices;

namespace ArchitectAPI.Wrappers.Audio
{
    public unsafe class ArchitectAudioClipFormat
    {
        public const string AAFCPATH = ".core/Internal/aafc";

        /// <summary>
        /// Native AAFC Import
        /// </summary>
        /// <param name="data">The managed data to utilize</param>
        /// <returns>Uncompressed 32 bit float AAFC data</returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_import(byte* data);

        /// <summary>
        /// Native AAFC Export
        /// </summary>
        /// <param name="samples">The raw data to export</param>
        /// <param name="freq">The sample rate</param>
        /// <param name="channels">Number of channels to speakers</param>
        /// <param name="samplelength">The length of the audio data</param>
        /// <param name="bps">Bits per sample</param>
        /// <param name="sampletype">1 -> PCM, 2 -> ADPCM</param>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_export(float* samples, int freq, int channels, int samplelength, byte bps = 16, byte sampletype = 1, bool forcemono = false, int samplerateoverride = 0, bool nm = false, float pitch = 1);

        /// <summary>
        /// Gets the header from AAFC data
        /// </summary>
        /// <param name="data">AAFC Input</param>
        /// <returns>28 byte header or 12 byte legacy header</returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_getheader(byte* data);

        /// <summary>
        /// Frees all AAFC imported data from memory
        /// </summary>
        /// <param name="arr"></param>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_free(float* arr);

        /// <summary>
        /// Frees all AAFC data from memory
        /// </summary>
        /// <param name="arr"></param>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_free_bytes(byte* arr);

        /// <summary>
        /// Natively convert floating point arrays to integer types
        /// </summary>
        /// <param name="arr">The input array</param>
        /// <param name="length">The absolute samplelength</param>
        /// <param name="type">Integer type (8 > byte, 16 > short, 32 > int)</param>
        /// <returns></returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_float_to_int(float* arr, long length, byte type);

        /// <summary>
        /// Natively convert floating point arrays to integer types
        /// </summary>
        /// <param name="arr">The input array</param>
        /// <param name="length">The absolute samplelength</param>
        /// <param name="type">Integer type (8 > byte, 16 > short, 32 > int)</param>
        /// <returns></returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_int_to_float(IntPtr arr, long length, byte type);

        /// <summary>
        /// Resample audio
        /// </summary>
        /// <param name="input"></param>
        /// <param name="samplerateoverride"></param>
        /// <param name="freq"></param>
        /// <param name="channels"></param>
        /// <param name="samplelength"></param>
        /// <returns>Resampled float array</returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_resample_data(float* input, int samplerateoverride, int freq, byte channels, ref int samplelength, float pitch = 1);

        /// <summary>
        /// Normalizes the sample array
        /// </summary>
        /// <param name="arr">Array input</param>
        /// <param name="len">Absolute sample length</param>
        /// <returns></returns>
        [DllImport(AAFCPATH)]
        public static extern IntPtr aafc_normalize(float* arr, int len);

        /// <summary>
        /// AAFC Header structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct AAFC_HEADER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
            public string headr;

            public int version;
            public int freq;
            public byte channels;
            public int samplelength;
            public byte bps;
            public byte sampletype;
        }

        static int GetFinalSize(byte bps, byte sampletype, int fnsplen)
        {
            int bsize = bps switch
            {
                4 => fnsplen / 2,
                8 => fnsplen * sizeof(byte),
                10 => ((fnsplen * 5 + 3) / 4),
                12 => ((fnsplen * 3) / 2),
                16 => fnsplen * sizeof(short),
                24 => fnsplen * 3,
                32 => fnsplen * sizeof(float),
                _ => 0
            };

            int bitm = sampletype switch
            {
                1 => Marshal.SizeOf<AAFC_HEADER>() + bsize,
                2 => Marshal.SizeOf<AAFC_HEADER>() + fnsplen / 2,
                3 => Marshal.SizeOf<AAFC_HEADER>() + fnsplen / 8,
                4 => Marshal.SizeOf<AAFC_HEADER>() + bsize,
                _ => 0,
            };

            return bitm;
        }

        public unsafe static byte[] ToByteArray(float[] samples, int channels, int samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, int sproverride = 0, bool nm = false, float pitch = 1)
        {
            fixed (float* fptr = samples)
            {
                byte* rstptr = (byte*)aafc_export(fptr, samplerate, channels, samples.Length, bps, sampletype, mono, sproverride, nm, pitch);

                int splen = (int)((mono ? ((float)samples.Length / channels) : samples.Length) / pitch);
                float reratio = sproverride != 0 ? (float)sproverride / samplerate : 0;
                int resampledlen = (int)(splen * reratio);

                int fnsplen = sproverride != 0 ? resampledlen : splen;

                int bitm = GetFinalSize(bps, sampletype, fnsplen);

                byte[] rst = new byte[bitm];

                Marshal.Copy((IntPtr)rstptr, rst, 0, bitm);
                Marshal.FreeHGlobal((IntPtr)rstptr);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(byte[] isamples, int channels, int samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, int sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (byte* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 8), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                byte* rstptr = (byte*)aafc_export(fptr, samplerate, channels, samples.Length, bps, sampletype, mono, sproverride, nm, pitch);

                int splen = (int)((mono ? ((float)samples.Length / channels) : samples.Length) / pitch);
                float reratio = sproverride != 0 ? (float)sproverride / samplerate : 0;
                int resampledlen = (int)(splen * reratio);

                int fnsplen = sproverride != 0 ? resampledlen : splen;

                int bitm = GetFinalSize(bps, sampletype, fnsplen);

                byte[] rst = new byte[bitm];

                Marshal.Copy((IntPtr)rstptr, rst, 0, bitm);
                Marshal.FreeHGlobal((IntPtr)rstptr);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(short[] isamples, int channels, int samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, int sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (short* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 16), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                byte* rstptr = (byte*)aafc_export(fptr, samplerate, channels, samples.Length, bps, sampletype, mono, sproverride, nm, pitch);

                int splen = (int)((mono ? ((float)samples.Length / channels) : samples.Length) / pitch);
                float reratio = sproverride != 0 ? (float)sproverride / samplerate : 0;
                int resampledlen = (int)(splen * reratio);

                int fnsplen = sproverride != 0 ? resampledlen : splen;

                int bitm = GetFinalSize(bps, sampletype, fnsplen);

                byte[] rst = new byte[bitm];

                Marshal.Copy((IntPtr)rstptr, rst, 0, bitm);
                Marshal.FreeHGlobal((IntPtr)rstptr);
                return rst;
            }
        }

        public unsafe static byte[] ToByteArray(int[] isamples, int channels, int samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, int sproverride = 0, bool nm = false, float pitch = 1)
        {
            float[] samples = new float[isamples.Length];
            fixed (int* ptr = isamples)
            {
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 32), samples, 0, isamples.Length);
            }

            fixed (float* fptr = samples)
            {
                byte* rstptr = (byte*)aafc_export(fptr, samplerate, channels, samples.Length, bps, sampletype, mono, sproverride, nm, pitch);

                int splen = (int)((mono ? ((float)samples.Length / channels) : samples.Length) / pitch);
                float reratio = sproverride != 0 ? (float)sproverride / samplerate : 0;
                int resampledlen = (int)(splen * reratio);

                int fnsplen = sproverride != 0 ? resampledlen : splen;

                int bitm = GetFinalSize(bps, sampletype, fnsplen);

                byte[] rst = new byte[bitm];

                Marshal.Copy((IntPtr)rstptr, rst, 0, bitm);
                Marshal.FreeHGlobal((IntPtr)rstptr);
                return rst;
            }
        }

        public unsafe static AAFC_Clip FromByteArray(byte[] bytes, string n)
        {
            fixed (byte* bptr = bytes)
            {
                IntPtr hptr = aafc_getheader(bptr);
                int freq, samplelength = 0;
                byte channels = 0;

                if (hptr != IntPtr.Zero)
                {
                    AAFC_HEADER header = Marshal.PtrToStructure<AAFC_HEADER>(hptr);
                    freq = header.freq; channels = header.channels; samplelength = header.samplelength;
                }
                else
                {
                    Console.WriteLine("Invalid header! It must be AAFC data.");
                    return null;
                }
                IntPtr samples = aafc_import(bptr);
                AAFC_Clip clip = new(n, (float*)samples, samplelength / channels, freq, channels);
                return clip;
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
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 8), rst, 0, clip.ActualSampleLength);
            return rst;
        }

        public unsafe static short[] ToShortSamples(AAFC_Clip clip)
        {
            short[] rst = new short[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 16), rst, 0, clip.ActualSampleLength);
            return rst;
        }

        public unsafe static int[] ToIntSamples(AAFC_Clip clip)
        {
            int[] rst = new int[clip.ActualSampleLength];
            Marshal.Copy(aafc_float_to_int(clip.Samples, clip.ActualSampleLength, 32), rst, 0, clip.ActualSampleLength);
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
        public int Frequency { get; private set; }
        /// <summary>
        /// Interleaved sample length
        /// </summary>
        public int SampleLength { get; private set; }
        /// <summary>
        /// Pre-multiplied Sample length
        /// </summary>
        public int ActualSampleLength { get; private set; }
        /// <summary>
        /// Channels defined
        /// </summary>
        public byte Channels { get; private set; }

        bool disposed = false;

        public AAFC_Clip(string name, float[] samples, int sampleLength, int freq, byte channels)
        {
            Name = name;
            Samples = (float*)Marshal.AllocHGlobal(samples.Length * sizeof(float));
            Marshal.Copy(samples, 0, (nint)Samples, samples.Length);
            ActualSampleLength = samples.Length;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
        }

        public AAFC_Clip(string name, float* samples, int sampleLength, int freq, byte channels)
        {
            Name = name;
            Samples = samples;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
            ActualSampleLength = sampleLength * channels;
        }

        // oh
        public void Resample(int newSampleRate, float pitch = 1)
        {
            if (newSampleRate == Frequency && pitch == 1)
                return;

            float* rsptr = Samples;
            int newSampleLength = ActualSampleLength;
            IntPtr newSamplesPtr = ArchitectAudioClipFormat.aafc_resample_data(rsptr, newSampleRate, Frequency, Channels, ref newSampleLength, pitch);

            if (newSamplesPtr != IntPtr.Zero)
            {
                if (Samples != null)
                {
                    Marshal.FreeHGlobal((IntPtr)Samples);
                }
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

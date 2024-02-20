using System.Runtime.InteropServices;

namespace ArchitectAPI.Wrappers.Audio
{
    // change dll path if using a unique file structure
    public class ArchitectAudioClipFormat
    {
        
        /// <summary>
        /// Native AAFC Import
        /// </summary>
        /// <param name="data">The managed data to utilize</param>
        /// <returns>Uncompressed 32 bit float AAFC data</returns>
        [DllImport("Internal/aafc.dll")]
        public static extern IntPtr aafc_import(byte[] data);

        /// <summary>
        /// Native AAFC Export
        /// </summary>
        /// <param name="samples">The raw data to export</param>
        /// <param name="freq">The sample rate</param>
        /// <param name="channels">Number of channels to speakers</param>
        /// <param name="samplelength">The length of the audio data</param>
        /// <param name="bps">Bits per sample</param>
        /// <param name="sampletype">1 -> PCM, 2 -> ADPCM</param>
        [DllImport("Internal/aafc.dll")]
        public static extern IntPtr aafc_export(float[] samples, int freq, int channels, int samplelength, byte bps = 16, byte sampletype = 1, bool forcemono = false);

        /// <summary>
        /// Gets the header from AAFC data
        /// </summary>
        /// <param name="data">AAFC Input</param>
        /// <returns>28 byte header or 12 byte legacy header</returns>
        [DllImport("Internal/aafc.dll")]
        public static extern IntPtr aafc_getheader(byte[] data);

        /// <summary>
        /// Frees all AAFC data from memory
        /// </summary>
        /// <param name="arr"></param>
        [DllImport("Internal/aafc.dll")]
        public static extern IntPtr aafc_free(IntPtr arr);

        /// <summary>
        /// Natively convert floating point arrays to integer types
        /// </summary>
        /// <param name="arr">The input array</param>
        /// <param name="length">The absolute samplelength</param>
        /// <param name="type">Integer type (8 > byte, 16 > short, 32 > int)</param>
        /// <returns></returns>
        [DllImport("Internal/aafc.dll")]
        public static extern IntPtr aafc_float_to_int(float* arr, long length, byte type)

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

        public unsafe static AudioClip FromByteArray(byte[] bytes, string n)
        {
            IntPtr hptr = aafc_getheader(bytes);
            int freq = 0;
            int channels = 0;
            int samplelength = 0;

            if (hptr != IntPtr.Zero)
            {
                if (bytes[0] == 'A' && bytes[1] == 'A' && bytes[2] == 'F' && bytes[3] == 'C')
                {
                    AAFC_HEADER header = Marshal.PtrToStructure<AAFC_HEADER>(hptr);
                    freq = header.freq; channels = header.channels; samplelength = header.samplelength;
                }
                else
                {
                    int* iptr = (int*)hptr;
                    freq = *iptr;
                    channels = *(iptr + 1);
                    samplelength = *(iptr + 2);
                }
            }
            else
            {
                Console.WriteLine("Invalid header! It must be AAFC data.");
                return null;
            }

            IntPtr samples = aafc_import(bytes);

            AudioClip clip = new(n, (float*)samples, samplelength / channels, freq, channels);

            return clip;
        }

        // read aafc from file system
        public static AudioClip LoadAAFC(string filename)
        {
            return FromByteArray(File.ReadAllBytes(filename), Path.GetFileNameWithoutExtension(filename));
        }
    }

    public unsafe class AudioClip : IDisposable
    {
        public string Name { get; private set; }
        public float* Samples { get; private set; }
        public int Frequency { get; private set; }
        public int SampleLength { get; private set; }
        public int ActualSampleLength { get; private set; }
        public int Channels { get; private set; }
        bool disposed = false;

        public AudioClip(string name, float[] samples, int sampleLength, int freq, int channels)
        {
            Name = name;
            Samples = (float*)Marshal.AllocHGlobal(samples.Length * sizeof(float));
            Marshal.Copy(samples, 0, (nint)Samples, samples.Length);
            ActualSampleLength = samples.Length;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
        }

        public AudioClip(string name, float* samples, int sampleLength, int freq, int channels)
        {
            Name = name;
            Samples = samples;
            Frequency = freq;
            SampleLength = sampleLength;
            Channels = channels;
            ActualSampleLength = sampleLength * channels;
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

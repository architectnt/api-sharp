// Alternative to System.Random
namespace ArchitectAPI.Internal
{
    public static class RandomProviders
    {
        /// <summary>
        /// The IHaveNoIdea of all time
        /// </summary>
        /// <returns>A seed</returns>
        public static ulong GenerateRandomSeed()
        {
            ulong iHaveNoIdea = (ulong)System.DateTime.Now.Ticks;
            iHaveNoIdea ^= (iHaveNoIdea << 21);
            iHaveNoIdea ^= (iHaveNoIdea >> 35);
            iHaveNoIdea ^= (iHaveNoIdea << 4);
            iHaveNoIdea &= (iHaveNoIdea >> 1);
            iHaveNoIdea |= (iHaveNoIdea << 3);

            return iHaveNoIdea;
        }

        public static void InitializeAll()
        {
            RandomPCG.Initialize();
            Random256.Initialize();
            Random128.Initialize();
            RandomLFSR.Initialize();
        }
    }

    /// <summary>
    /// Architect Enterprises - Xoshiro256** Random Number Generater
    /// </summary>
    public static class Random256
    {
        static ulong x, y, z, w;
        static ulong Value
        {
            get
            {
                ulong rst = Rotl(y * 5, 7) * 9;
                ulong t = y << 17;
                z ^= x; w ^= y; y ^= z; x ^= w; z ^= t;
                w = Rotl(w, 45);

                return rst;
            }
        }

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initialize(ulong seed = 0)
        {
            if (seed == 0) seed = RandomProviders.GenerateRandomSeed();

            x = seed;
            y = seed ^ (seed << 21);
            z = seed ^ (seed >> 17);
            w = seed ^ (seed << 43);
        }

        static ulong Rotl(ulong x, int k) => (x << k) | (x >> (64 - k));

        public static ulong Next()
        {
            ulong rst = Rotl(y * 5, 7) * 9;
            ulong t = y << 17;
            z ^= x; w ^= y; y ^= z; x ^= w; z ^= t;
            w = Rotl(w, 45);

            return rst;
        }

        public static uint Next32()
        {
            return (uint)(Value >> 32);
        }

        public static int Range(int min, int max)
        {
            return (int)(Value % ((ulong)(max - min))) + min;
        }

        public static float Range(float min, float max)
        {
            return (float)((Value / (double)ulong.MaxValue) * (max - min) + min);
        }

        public static int Range(int max)
        {
            return (int)(Value % ((ulong)max));
        }

        public static float Range(float max)
        {
            return (float)(Value / (double)ulong.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return (float)((Value / (double)ulong.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return (float)(Value / (double)ulong.MaxValue);
        }

        public static float Normalized()
        {
            return (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Value / (double)ulong.MaxValue;
        }
    }

    /// <summary>
    /// Architect Enterprises - Xorshift128 32-bit Random Number Generater
    /// </summary>
    public static class Random128
    {
        static uint x, y, z, w;
        static uint Value
        {
            get
            {
                uint t = x ^ (x << 11);
                x = y; y = z; z = w;
                return w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            }
        }

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initialize(uint seed = 0)
        {
            if (seed == 0)
            {
                ulong huh = RandomProviders.GenerateRandomSeed();
                x = (uint)(huh & 0xFFFFFFFF);
                y = (uint)((huh >> 32) & 0xFFFFFFFF);
                z = (uint)((huh >> 16) & 0xFFFFFFFF);
                w = (uint)(huh >> 24);
            }
            else
            {
                x = seed;
                y = seed ^ (seed << 7);
                z = seed ^ (seed >> 11);
                w = seed ^ (seed << 13);
            }
        }

        public static int Range(int min, int max)
        {
            return (int)(Value % ((uint)(max - min))) + min;
        }

        public static float Range(float min, float max)
        {
            return (Value / (float)uint.MaxValue) * (max - min) + min;
        }

        public static int Range(int max)
        {
            return (int)(Value % ((uint)max));
        }

        public static float Range(float max)
        {
            return (Value / (float)uint.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return ((Value / (float)uint.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return Value / (float)uint.MaxValue;
        }

        public static float Normalized()
        {
            return (Value / (float)uint.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Value / (double)uint.MaxValue;
        }
    }

    /// <summary>
    /// Architect Enterprises - Basic 64 bit LFSR Random Number Generater
    /// </summary>

    public static class RandomLFSR
    {
        static ulong rngn;
        static ulong Value
        {
            get
            {
                ulong b = (rngn >> 63) ^ (rngn >> 62) ^ (rngn >> 60) ^ (rngn >> 59);
                rngn <<= 1;
                rngn |= (~b) & 1;
                return rngn;
            }
        }

        public static void Initialize(ulong seed = 0)
        {
            rngn = seed == 0 ? RandomProviders.GenerateRandomSeed() : seed;
            if (rngn == 0) rngn = 1;
        }

        public static int Range(int min, int max)
        {
            return (int)(Value % ((ulong)(max - min))) + min;
        }

        public static float Range(float min, float max)
        {
            return (float)((Value / (double)ulong.MaxValue) * (max - min) + min);
        }

        public static int Range(int max)
        {
            return (int)(Value % ((ulong)max));
        }

        public static float Range(float max)
        {
            return (float)(Value / (double)ulong.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return (float)((Value / (double)ulong.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return (float)(Value / (double)ulong.MaxValue);
        }

        public static float Normalized()
        {
            return (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Value / (double)ulong.MaxValue;
        }
    }



    public static class RandomPCG
    {
        static ulong s;
        static ulong i;
        static ulong Value
        {
            get
            {
                ulong odst = s;
                s = odst * 6364136223846793005UL + (i | 1);
                ulong xshft = ((odst >> 18) ^ odst) >> 27;
                ulong r = odst >> 59;
                return (xshft >> (int)r) | (xshft << ((-(int)r) & 31));
            }
        }

        public static void Initialize(ulong seed = 0, ulong ic = 0)
        {
            if (seed == 0) seed = RandomProviders.GenerateRandomSeed();
            if (ic == 0) ic = RandomProviders.GenerateRandomSeed() | 1;
            s = 0U;
            i = (ic << 1) | 1;
            Next();
            s += seed;
            Next();
        }

        /* 
            because we cant basically do what it does in c we're limited by pointless casts to make roslyn happy
            beware casting overhead
        */
        public static ulong Next()
        {
            ulong odst = s;
            s = odst * 6364136223846793005UL + (i | 1);
            ulong xshft = ((odst >> 18) ^ odst) >> 27;
            ulong r = odst >> 59;
            return (xshft >> (int)r) | (xshft << ((-(int)r) & 31));
        }

        public static int Range(int min, int max)
        {
            return (int)(Value % ((ulong)(max - min))) + min;
        }

        public static float Range(float min, float max)
        {
            return (float)((Value / (double)ulong.MaxValue) * (max - min)) + min;
        }

        public static int Range(int max)
        {
            return (int)(Value % ((ulong)max));
        }

        public static float Range(float max)
        {
            return (float)(Value / (double)ulong.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return (float)((Value / (double)ulong.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return (float)(Value / (double)ulong.MaxValue);
        }

        public static float Normalized()
        {
            return (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Value / (double)ulong.MaxValue;
        }
    }
}

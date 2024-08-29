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
    }

    /// <summary>
    /// Architect Enterprises - Xoshiro256** Random Number Generater
    /// </summary>
    public static class Random256
    {
        static ulong x, y, z, w;

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initalize(ulong seed = 0)
        {
            if (seed == 0) seed = RandomProviders.GenerateRandomSeed();

            x = seed;
            y = seed ^ (seed << 21);
            z = seed ^ (seed >> 17);
            w = seed ^ (seed << 43);
        }

        static ulong Rotl(ulong x, int k) => (x << k) | (x >> (64 - k));

        public static ulong Next64()
        {
            ulong rst = Rotl(y * 5, 7) * 9;
            ulong t = y << 17;
            z ^= x; w ^= y; y ^= z; x ^= w; z ^= t;
            w = Rotl(w, 45);

            return rst;
        }

        public static uint Next()
        {
            return (uint)(Next64() >> 32);
        }

        public static int Range(int min, int max)
        {
            return min + (int)(Next64() % ((ulong)(max - min)));
        }

        public static float Range(float min, float max)
        {
            return min + ((Next64() / (float)ulong.MaxValue) * (max - min));
        }

        public static int Range(int max)
        {
            return (int)(Next64() % ((ulong)max));
        }

        public static float Range(float max)
        {
            return (Next64() / (float)ulong.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return ((Next64() / (float)ulong.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return Next64() / (float)ulong.MaxValue;
        }

        public static float Normalized()
        {
            return (Next64() / (float)ulong.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Next64() / (double)ulong.MaxValue;
        }
    }

    /// <summary>
    /// Architect Enterprises - Xorshift128 32-bit Random Number Generater
    /// </summary>
    public static class Random128
    {
        static uint x, y, z, w;

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initalize(uint seed = 0)
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

        public static uint Next()
        {
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            return w = w ^ (w >> 19) ^ (t ^ (t >> 8));
        }

        public static int Range(int min, int max)
        {
            return min + (int)(Next() % ((uint)(max - min)));
        }

        public static float Range(float min, float max)
        {
            return min + ((Next() / (float)uint.MaxValue) * (max - min));
        }

        public static int Range(int max)
        {
            return (int)(Next() % ((uint)max));
        }

        public static float Range(float max)
        {
            return (Next() / (float)uint.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return ((Next() / (float)uint.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return Next() / (float)uint.MaxValue;
        }

        public static float Normalized()
        {
            return (Next() / (float)uint.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Next() / (double)uint.MaxValue;
        }
    }

    /// <summary>
    /// Architect Enterprises - Basic 64 bit LFSR Random Number Generater
    /// </summary>

    public static class RandomLFSR
    {
        static ulong rngn;

        public static void Initalize(ulong seed = 0)
        {
            rngn = seed == 0 ? RandomProviders.GenerateRandomSeed() : seed;
            if (rngn == 0) rngn = 1;
        }

        public static ulong Next()
        {
            ulong b = (rngn >> 0) ^ (rngn >> 1) ^ (rngn >> 3) ^ (rngn >> 5) ^ (rngn >> 12) ^ (rngn >> 25);
            return (rngn = (rngn >> 1) | (b << 63));
        }

        public static int Range(int min, int max)
        {
            return min + (int)(Next() % ((ulong)(max - min)));
        }

        public static float Range(float min, float max)
        {
            return min + ((Next() / (float)ulong.MaxValue) * (max - min));
        }

        public static int Range(int max)
        {
            return (int)(Next() % ((ulong)max));
        }

        public static float Range(float max)
        {
            return (Next() / (float)ulong.MaxValue) * max;
        }

        public static bool InChance(float c)
        {
            return ((Next() / (float)ulong.MaxValue) * 100f) < c;
        }

        public static float RandomFloat()
        {
            return Next() / (float)ulong.MaxValue;
        }

        public static float Normalized()
        {
            return (Next() / (float)ulong.MaxValue) * 2.0f - 1.0f;
        }

        public static double RandomDouble()
        {
            return Next() / (double)ulong.MaxValue;
        }
    }
}
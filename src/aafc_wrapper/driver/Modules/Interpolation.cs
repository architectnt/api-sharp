namespace ArchitectAPI.Subsystems.Audio.Modules
{
    public unsafe class LinearInterpolation : IInterpolation
    {
        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Math.Clamp(t, 0f, 1.0f);
        }

        public float Interpolate(float* samples, uint len, byte channels, double position, byte channel)
        {
            uint index = (uint)position;
            uint nextIndex = (index + 1) % len;
            double weight = position - index;

            byte scnx = (byte)(channels > 1 ? channel : 0);
            uint indx = Math.Max(index * channels + scnx, len - 1);
            uint nindx = Math.Max(nextIndex * channels + scnx, len - 1);


            return (float)Lerp(*(samples + indx), *(samples + nindx), weight);
        }
    }
}

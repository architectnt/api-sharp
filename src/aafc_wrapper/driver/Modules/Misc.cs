/*
    Copyright (C) 2025 Architect Enterprises
    This file is apart of the API and are MIT licensed
*/

namespace ArchitectAPI.Subsystems.Audio.Modules
{
    // clunky...
    public class EqualizerBand
    {
        public float CenterFrequency { get; set; }
        public float Gain { get; set; }
        public float Q { get; set; }

        private float a0, a1, a2, b0, b1, b2;
        private float z1 = 0.0f, z2 = 0.0f;

        public EqualizerBand(float centerFrequency, float gain, float q)
        {
            CenterFrequency = centerFrequency;
            Gain = gain;
            Q = q;
            CalculateCoefficients();
        }

        private void CalculateCoefficients()
        {
            float A = (float)Math.Pow(10, Gain / 40);
            float omega = 2 * (float)Math.PI * CenterFrequency / AudioProcessor.frequency / 2;
            float cosOmega = (float)Math.Cos(omega);
            float sinOmega = (float)Math.Sin(omega);
            float alpha = sinOmega / (2 * Q);

            b0 = 1 + alpha * A;
            b1 = -2 * cosOmega;
            b2 = 1 - alpha * A;
            a0 = 1 + alpha / A;
            a1 = -2 * cosOmega;
            a2 = 1 - alpha / A;

            b0 /= a0;
            b1 /= a0;
            b2 /= a0;
            a1 /= a0;
            a2 /= a0;
        }

        public float ApplyEQ(float sample)
        {
            float output = b0 * sample + b1 * z1 + b2 * z2 - a1 * z1 - a2 * z2;

            z2 = z1;
            z1 = output;

            return output;
        }
    }
}

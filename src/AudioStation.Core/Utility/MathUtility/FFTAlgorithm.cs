using System.Numerics;

namespace AudioStation.Core.Utility.MathUtility
{
    public static class FFTAlgorithm
    {
        /// <summary>
        /// Calculates the FFT into the destination array; and outputs the maximum real of the array.
        /// </summary>
        public static void FFT(Complex[] destination)
        {
            var N = destination.Length;

            if (N == 1)
                return;

            if (N % 2 != 0)
                throw new ArgumentException("FFT input must be a power of 2");

            // Divide
            var even = new Complex[N / 2];
            var odd = new Complex[N / 2];

            for (int k = 0; k < N / 2; k++)
            {
                even[k] = destination[k * 2];
                odd[k] = destination[(k * 2) + 1];
            }

            // Conquer
            FFT(even);
            FFT(odd);

            // Combine
            for (int k = 0; k < N / 2; k++)
            {
                Complex t = Complex.FromPolarCoordinates(1.0, (-2.0D * Math.PI * k) / N) * odd[k];

                destination[k] = even[k] + t;
                destination[k + (N / 2)] = even[k] - t;
            }
        }

        /// <summary>
        /// Calculates the IFFT into the destination array; and outputs the maximum real of the array.
        /// </summary>
        public static void IFFT(Complex[] destination)
        {
            var N = destination.Length;

            if (N == 1)
                return;

            if (N % 2 != 0)
                throw new ArgumentException("FFT input must be a power of 2");

            // Divide
            var even = new Complex[N / 2];
            var odd = new Complex[N / 2];

            for (int i = 0; i < N / 2; i++)
            {
                even[i] = destination[i * 2];
                odd[i] = destination[(i * 2) + 1];
            }

            // Conquer
            IFFT(even);
            IFFT(odd);

            // Combine
            for (int k = 0; k < N / 2; k++)
            {
                Complex t = Complex.FromPolarCoordinates(1.0, (2.0D * Math.PI * k) / N) * odd[k];

                destination[k] = even[k] + t;
                destination[k + (N / 2)] = even[k] - t;
            }
        }

        /// <summary>
        /// Generates a Guassian window function value. https://en.wikipedia.org/wiki/Window_function
        /// </summary>
        /// <param name="sigma">Standard deviation, should be less than 0.5 for this function</param>
        /// <param name="n">Current index of the signal vector</param>
        /// <param name="N">Length signal vector. MUST BE MULTIPLE OF 2!</param>
        public static double GaussianWindow(double sigma, int n, int N)
        {
            if (sigma > 0.5)
                throw new Exception("Must use a value of sigma less than or equal to 0.5 for the GuassianWindow algorithm");

            if (N % 2 != 0)
                throw new Exception("Must use an even size array for the GuassianWindow algorithm");

            int Nover2 = N / 2;

            return Math.Exp(-0.5 * Math.Pow((n - (Nover2)) / (sigma * Nover2), 2));
        }
    }
}

using NAudio.Dsp;

namespace AudioStation.Component.AudioProcessing
{
    public class EqualizerResultSet
    {
        [Flags]
        public enum UpdateType
        {
            /// <summary>
            /// No update this cycle (sample period)
            /// </summary>
            None = 0,

            /// <summary>
            /// The update for the output should be the standard output, which is the "Result".
            /// </summary>
            Output = 1,

            /// <summary>
            /// The update for the output should be the peak output, which is the "ResultPeaks".
            /// </summary>
            PeakOutput = 2
        }

        /// <summary>
        /// Number of "channels" input by the FFT SampleAggregator. This must be a power of 2. The
        /// default is 1024.
        /// </summary>
        public int ChannelsInput { get; private set; }

        /// <summary>
        /// Number of output channels. This will take bands of the FFT result and average them.
        /// </summary>
        public int ChannelsOutput { get; private set; }

        /// <summary>
        /// Sample period of integration before sending a result. The default is 100 (samples).
        /// </summary>
        public int IntegrationPeriod { get; private set; }

        /// <summary>
        /// Sample period (number) to measure peak values before providing a result. The minimum value
        /// must be at least the integration period; and the default value is 5 * the integration period.
        /// </summary>
        public int PeakPeriod { get; private set; }

        /// <summary>
        /// Interpolation is used to calculate the current equalizer signal. This will prevent the
        /// output from being too "jumpy". The default value is 0.1; and must be less than 1.
        /// </summary>
        public float ReleaseCoefficient { get; private set; }

        /// <summary>
        /// Normalized result set for the integration period.
        /// </summary>
        public float[] Result { get; private set; }

        /// <summary>
        /// The max value peaks for the previous period.
        /// </summary>
        public float[] ResultPeaks { get; private set; }

        /// <summary>
        /// Counter for the integration period 
        /// </summary>
        protected int Counter { get; private set; }

        /// <summary>
        /// Counter for the peak period
        /// </summary>
        protected int PeakCounter { get; private set; }

        // Set of values matching the input channel count
        private float[] _inputBuffer;
        private float[] _outputBuffer;

        /// <summary>
        /// Updates the result set with the current NAudio FFT buffer
        /// </summary>
        public UpdateType Update(Complex[] fftBuffer)
        {
            if (_inputBuffer.Length != fftBuffer.Length)
                throw new ArgumentException("Improper configuration of input buffer. Size of NAudio FFT Buffer not equal to the input buffer length.");

            var result = UpdateType.None;

            // Result "Channels"
            var bucketSize = (int)(fftBuffer.Length / (float)this.ChannelsOutput);
            var lastBucketIndex = -1;

            for (int index = 0; index < fftBuffer.Length; index++)
            {
                // Current output "channel"
                var bucketIndex = index / bucketSize;

                var fftOutput = (float)Math.Sqrt((fftBuffer[index].X * fftBuffer[index].X) +
                                                 (fftBuffer[index].Y * fftBuffer[index].Y));

                if (lastBucketIndex != bucketIndex)
                {
                    _outputBuffer[bucketIndex] = 0;
                    lastBucketIndex = bucketIndex;
                }

                // Take input directly (we may use some interpolation / relaxation on the input)
                _inputBuffer[index] = fftOutput;

                // Average the input into buckets
                _outputBuffer[bucketIndex] += fftOutput / (float)bucketSize;

                // Integrate this for the period (running average)
                //var fftOutputAverage = this.Counter == 0 ? fftOutput : (fftOutput + (this.Counter * _inputBuffer[index])) / (this.Counter + 1);

                // Relax the output by using interpolation
                //_inputBuffer[index] = this.Counter == 0 ? 0 : (_inputBuffer[index] * (1 - this.ReleaseCoefficient)) + (this.ReleaseCoefficient * fftOutputAverage);

                // Average the output "channels"
                //this.Result[bucketIndex] += (_inputBuffer[index] / bucketSize);
                //this.Result[bucketIndex] += (fftOutput / bucketSize);
                //this.ResultPeaks[bucketIndex] = this.PeakCounter == 0 ? this.Result[bucketIndex] : Math.Max(this.Result[bucketIndex], this.ResultPeaks[bucketIndex]);
            }

            // Integrate the output until we're ready to read it
            for (int bucketIndex = 0; bucketIndex < this.ChannelsOutput; bucketIndex++)
            {
                // Change the result output using the release coefficient parameter to linearly interpolate over a number of samples
                this.Result[bucketIndex] += (_outputBuffer[bucketIndex] - this.Result[bucketIndex]) * (1 - this.ReleaseCoefficient);

                // Running peak (for the peak period)
                var peakValue = Math.Max(this.Result[bucketIndex], this.ResultPeaks[bucketIndex]);

                if (this.PeakCounter == 0)
                    this.ResultPeaks[bucketIndex] = this.Result[bucketIndex];

                else
                    this.ResultPeaks[bucketIndex] = peakValue;

                //var average = (_outputBuffer[bucketIndex] + (this.Counter * this.Result[bucketIndex])) / ((float)(this.Counter + 1));

                //// Running average
                //this.Result[bucketIndex] = this.Counter == 0 ?
                //                               _outputBuffer[bucketIndex] :
                //                               (this.Result[bucketIndex] * this.ReleaseCoefficient) + ((1.0f - this.ReleaseCoefficient) * average);


                //// Running peak (for the peak period)
                //this.ResultPeaks[bucketIndex] = this.PeakCounter == 0 ?
                //                                    this.Result[bucketIndex] :
                //                                    Math.Max(this.Result[bucketIndex], this.ResultPeaks[bucketIndex]);
            }

            this.Counter++;
            this.PeakCounter++;

            if (this.Counter >= this.IntegrationPeriod)
            {
                this.Counter = 0;

                result |= UpdateType.Output;
            }
            if (this.PeakCounter >= this.PeakPeriod)
            {
                this.PeakCounter = 0;

                result |= UpdateType.PeakOutput;
            }

            return result;
        }

        public EqualizerResultSet(int channelsInput, int channelsOutput, int integrationPeriod, int peakPeriod, float releaseCoefficient)
        {
            this.ChannelsInput = channelsInput;
            this.ChannelsOutput = channelsOutput;
            this.IntegrationPeriod = integrationPeriod;
            this.PeakPeriod = peakPeriod < integrationPeriod ? integrationPeriod * 5 : peakPeriod;
            this.ReleaseCoefficient = releaseCoefficient;

            this.Counter = 0;
            this.PeakCounter = 0;

            this.Result = new float[this.ChannelsOutput];
            this.ResultPeaks = new float[this.ChannelsOutput];

            _inputBuffer = new float[this.ChannelsInput];
            _outputBuffer = new float[this.ChannelsOutput];
        }
    }
}

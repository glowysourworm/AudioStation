using System.Numerics;

using MathNet.Numerics;

namespace AudioStation.Model.AudioProcessing
{
    public class EqualizerResultSet
    {

        /// <summary>
        /// Delegate for iterating the result set to use full options, including the windowed result set. If you're using
        /// the windowed result the index and length will be adjusted before sending you results.
        /// </summary>
        /// <param name="index">Index (both windowed and normal)</param>
        /// <param name="length">Length (both windowed and normal)</param>
        /// <param name="result">Current result item</param>
        /// <param name="resultReleaseHalf">Current result "release half" item</param>
        /// <param name="resultReleaseFull">Current result "release full" item</param>
        /// <param name="resultPeak">Current result peak</param>
        /// <param name="maxValue">Normalizer (max result)</param>
        /// <param name="maxValueReleaseHalf">Normalizer (max result release half)</param>
        /// <param name="maxValueReleaseFull">Normalizer (max result release full)</param>
        /// <param name="maxPeak">Normalizer (max peak)</param>
        public delegate void EqualizerResultSetIterator(int index,
                                                        int length,
                                                        float result,
                                                        float resultReleaseHalf,
                                                        float resultReleaseFull,
                                                        float resultPeak,
                                                        float maxValue,
                                                        float maxValueReleaseHalf,
                                                        float maxValueReleaseFull,
                                                        float maxPeak);

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
        /// The FFT results tend to be non-ideal for viewing. These window types will help to 
        /// provide the result arrays in a format ready to view
        /// </summary>
        public enum ResultWindowType
        {
            /// <summary>
            /// Returns the full output
            /// </summary>
            None = 0,

            /// <summary>
            /// Returns the middle half of the FFT, leaving a quarter off the front and back ends.
            /// </summary>
            SymmetricHalf,


            /// <summary>
            /// Returns the middle quarter (2/8) of the FFT, leaving (6/8) trimmed equally off the
            /// front and back
            /// </summary>
            SymmetricQuarter
        }

        /// <summary>
        /// Normalized result set for the integration period.
        /// </summary>
        public float[] Result { get; private set; }

        /// <summary>
        /// Normalized result with half release coefficient (these will be "half as slow" as the result)
        /// </summary>
        public float[] ResultReleaseHalf { get; private set; }

        /// <summary>
        /// Normalized result with full release coefficient (these will be the slowest compared to the result)
        /// </summary>
        public float[] ResultReleaseFull { get; private set; }

        /// <summary>
        /// The max value peaks for the previous period. These will follow the peak integration period; and 
        /// not be modified by the release coefficient.
        /// </summary>
        public float[] ResultPeaks { get; private set; }


        // Set of values matching the input channel count
        private float[] _inputBuffer;
        private float[] _outputBuffer;

        float _maxValue;
        float _maxPeak;
        float _maxValueReleaseHalf;
        float _maxValueReleaseFull;

        private int _inputChannels;
        private int _outputChannels;

        private int _integrationPeriod;
        private int _peakIntegrationPeriod;

        private int _counter;
        private int _peakCounter;

        private bool _counterResetPending;
        private bool _peakCounterResetPending;

        private float _releaseCoefficient;

        /// <summary>
        /// Iterates the result set and provides a callback with all required results and indexes
        /// pre-calculated.
        /// </summary>
        public void Iterate(ResultWindowType windowType, EqualizerResultSetIterator iterator)
        {
            var length = 0;
            var startIndex = 0;
            var endIndex = 0;

            // Calcualate index window
            switch (windowType)
            {
                case ResultWindowType.None:
                    length = _outputChannels;
                    startIndex = 0;
                    endIndex = _outputChannels - 1;
                    break;
                case ResultWindowType.SymmetricHalf:
                    length = _outputChannels / 2;
                    startIndex = (_outputChannels / 4) - 1;
                    endIndex = (_outputChannels * 3 / 4) - 1;
                    break;
                case ResultWindowType.SymmetricQuarter:
                    length = _outputChannels / 4;
                    startIndex = (_outputChannels * 3 / 8) - 1;
                    endIndex = (_outputChannels * 5 / 8) - 1;
                    break;
                default:
                    throw new Exception("Unhandled result window type");
            }

            // Callback
            for (int index = startIndex; index <= endIndex; index++)
            {
                iterator(index - startIndex, length,
                         this.Result[index],
                         this.ResultReleaseHalf[index],
                         this.ResultReleaseFull[index],
                         this.ResultPeaks[index],
                         _maxValue,
                         _maxValueReleaseHalf,
                         _maxValueReleaseFull,
                         _maxPeak);
            }
        }

        public int GetWindowedResultLength(ResultWindowType windowType)
        {
            switch (windowType)
            {
                case ResultWindowType.None:
                    return _outputChannels;
                case ResultWindowType.SymmetricHalf:
                    return _outputChannels / 2;
                case ResultWindowType.SymmetricQuarter:
                    return _outputChannels / 4;
                default:
                    throw new Exception("Unhandled result window type");
            }
        }

        /// <summary>
        /// Updates the result set with the current NAudio FFT buffer
        /// </summary>
        public UpdateType Update(Complex[] fftBuffer)
        {
            if (_inputBuffer.Length != fftBuffer.Length)
                throw new ArgumentException("Improper configuration of input buffer. Size of NAudio FFT Buffer not equal to the input buffer length.");

            var result = UpdateType.None;

            // Result "Channels" (these are validated as powers of two)
            var bucketSize = _inputChannels / _outputChannels;

            // Reset Output
            if (_counterResetPending)
            {
                // Reset this bucket
                for (int bucketIndex = 0; bucketIndex < _outputChannels; bucketIndex++)
                {
                    _outputBuffer[bucketIndex] = 0;
                }

                _counter = 0;
                _counterResetPending = false;
            }

            // Reset Peak
            if (_peakCounterResetPending)
            {
            }

            _maxValue = 0;
            _maxValueReleaseHalf = 0;
            _maxValueReleaseFull = 0;
            _maxPeak = 0;

            // Input
            for (int index = 0; index < fftBuffer.Length; index++)
            {
                // Current output "channel"
                var bucketIndex = (int)(index / (float)bucketSize);

                var fftOutput = (float)Math.Sqrt((fftBuffer[index].Real * fftBuffer[index].Real) +
                                                 (fftBuffer[index].Imaginary * fftBuffer[index].Imaginary));

                // Store input (we were using this before; but it may not be needed) (can try averaging.. other tricks)
                _inputBuffer[index] = fftOutput;

                // Accumulate Output
                _outputBuffer[bucketIndex] += fftOutput / (float)bucketSize;

                // Track Max Value
                _maxValue = Math.Max(_maxValue, _outputBuffer[bucketIndex]);
            }

            // Output
            for (int bucketIndex = 0; bucketIndex < _outputChannels; bucketIndex++)
            {
                // Change the result output using the release coefficient parameter to linearly interpolate over a number of samples
                this.Result[bucketIndex] = _outputBuffer[bucketIndex];
                this.ResultReleaseHalf[bucketIndex] += (_outputBuffer[bucketIndex] - this.ResultReleaseHalf[bucketIndex]) * (1 - _releaseCoefficient) * 0.5f;
                this.ResultReleaseFull[bucketIndex] += (_outputBuffer[bucketIndex] - this.ResultReleaseFull[bucketIndex]) * (1 - _releaseCoefficient);

                // Running peak (for the peak period)
                //var peakValue = Math.Max(this.Result[bucketIndex], this.ResultPeaks[bucketIndex]);

                //if (_peakCounterResetPending)
                //    this.ResultPeaks[bucketIndex] = this.Result[bucketIndex];

                //else
                //    this.ResultPeaks[bucketIndex] = peakValue;

                // Less Than:  Relax to the output level
                if (_outputBuffer[bucketIndex] < this.ResultPeaks[bucketIndex])
                    this.ResultPeaks[bucketIndex] += (_outputBuffer[bucketIndex] - this.ResultPeaks[bucketIndex]) * (1 - _releaseCoefficient);

                // Greater Than:  Jump to the output level
                else
                    this.ResultPeaks[bucketIndex] = _outputBuffer[bucketIndex];

                // Track Max Peak
                _maxPeak = Math.Max(_maxPeak, this.ResultPeaks[bucketIndex]);
                _maxValueReleaseHalf = Math.Max(_maxValueReleaseHalf, this.ResultReleaseHalf[bucketIndex]);
                _maxValueReleaseFull = Math.Max(_maxValueReleaseFull, this.ResultReleaseFull[bucketIndex]);
            }

            // Reset Pending Peak Counter
            if (_peakCounterResetPending)
            {
                _peakCounterResetPending = false;
                _peakCounter = 0;
            }

            _counter++;
            _peakCounter++;

            if (_counter >= _integrationPeriod)
            {
                // Give user a chance to get the data out before resetting
                _counterResetPending = true;

                result |= UpdateType.Output;
            }
            if (_peakCounter >= _peakIntegrationPeriod)
            {
                // Give user a chance to get the data out before resetting
                _peakCounterResetPending = true;

                result |= UpdateType.PeakOutput;
            }

            //return result;

            return UpdateType.PeakOutput | UpdateType.Output;
        }

        public EqualizerResultSet(int channelsInput, int channelsOutput, int integrationPeriod, int peakPeriod, float releaseCoefficient)
        {
            if (!channelsInput.IsPowerOfTwo() ||
                !channelsOutput.IsPowerOfTwo())
                throw new ArgumentException("Input / Output channel count must be powers of two");

            if (channelsOutput > channelsInput)
                throw new ArgumentException("Output channels must be less than or equal to input channel count");

            if (channelsOutput < Math.Pow(2, 5))
                throw new ArgumentException("Output channels must be greater than " + Math.Pow(2, 4).ToString());

            _inputChannels = channelsInput;
            _outputChannels = channelsOutput;
            _integrationPeriod = integrationPeriod;
            _peakIntegrationPeriod = peakPeriod < integrationPeriod ? integrationPeriod * 5 : peakPeriod;
            _releaseCoefficient = releaseCoefficient;

            _counter = 0;
            _peakCounter = 0;

            this.Result = new float[_outputChannels];
            this.ResultPeaks = new float[_outputChannels];
            this.ResultReleaseHalf = new float[_outputChannels];
            this.ResultReleaseFull = new float[_outputChannels];

            _inputBuffer = new float[_inputChannels];
            _outputBuffer = new float[_outputChannels];
        }
    }
}

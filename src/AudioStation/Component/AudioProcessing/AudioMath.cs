using static AudioStation.Component.AudioProcessing.AudioScale;

namespace AudioStation.Component.AudioProcessing
{
    public class AudioScale
    {
        public enum AudioScaleType
        {
            /// <summary>
            /// Line [-1,1] -> [-1,1]
            /// </summary>
            LineStandardLinear,

            /// <summary>
            /// Line [-1, 1] -> [threshold, 30]
            /// </summary>
            LineStandardLogarithmic,

            /// <summary>
            /// Gain [0,1] -> [0,1]
            /// </summary>
            GainStandardLinear,

            /// <summary>
            /// Gain [0,1] -> [threshold, 30]
            /// </summary>
            GainStandardLogarithmic
        }

        public double InputMin { get; private set; }
        public double InputMax { get; private set; }
        public double OutputMin { get; private set; }
        public double OutputMax { get; private set; }
        public double ScaleFactor { get; private set; }
        public bool Logarithmic { get; private set; }

        public const double THRESHOLD_LOG = 0.0000001;
        public const double SCALE_LOG = 20;

        public AudioScale(double inputMin, double inputMax, double outputMin, double outputMax, double scaleFactor, bool logarithmic)
        {
            if (logarithmic)
            {
                if (outputMin <= 0)
                    throw new ArgumentException("Logarithmic scales must have output limits greater than zero");
            }

            this.InputMin = inputMin;
            this.InputMax = inputMax;
            this.OutputMin = outputMin;
            this.OutputMax = outputMax;
            this.ScaleFactor = scaleFactor;
            this.Logarithmic = logarithmic;
        }

        public double Compute(double input)
        {
            if (!this.Logarithmic)
            {
                return LinearShift(input);
            }
            else
            {
                var inputShifted = LinearShift(input);

                return this.ScaleFactor * Math.Log(input + this.OutputMin);
            }
        }

        // Transfers the input value to the output (linear) scale before computing the logarithm
        private double LinearShift(double input)
        {
            var slope = (this.OutputMax - this.OutputMin) / (this.InputMax - this.InputMin);
            var intercept = this.OutputMax - (slope * this.InputMax);

            return Math.Clamp((input * slope) + intercept, this.OutputMin, this.OutputMax);
        }

        public static AudioScale GetScale(AudioScaleType type)
        {
            switch (type)
            {
                case AudioScaleType.LineStandardLinear:
                    return new AudioScale(-1, 1, -1, 1, 1, false);
                case AudioScaleType.LineStandardLogarithmic:
                    return new AudioScale(-1, 1, THRESHOLD_LOG, 10, SCALE_LOG, true);
                case AudioScaleType.GainStandardLinear:
                    return new AudioScale(0, 1, 0, 1, 1, false);
                case AudioScaleType.GainStandardLogarithmic:
                    return new AudioScale(0, 1, THRESHOLD_LOG, 10, SCALE_LOG, true);
                default:
                    throw new Exception("Unhandled audio scale type");
            }
        }
    }

    public static class AudioMath
    {
        /// <summary>
        /// Computes the Db value for a signal with the provided type. (e.g. line / gain signal -> Db) 
        /// </summary>
        public static double ToDecibel(float signal, AudioScaleType type = AudioScaleType.LineStandardLogarithmic)
        {
            var scale = AudioScale.GetScale(type);

            if (signal < scale.InputMin || signal > scale.InputMax)
                throw new ArgumentException("Input signal exceeds limits:  AudioMath.cs");

            return scale.Compute(signal);
        }
    }
}

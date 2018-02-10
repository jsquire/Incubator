using System;
using System.Collections.Generic;
using System.Linq;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    public class BasicImageClassifier : IImageClassifier<ImageObservation, IEnumerable<int>>
    {
        private readonly ImageDistanceCalculator<int> distanceCalculator;
        private IEnumerable<ImageObservation> observations;

        public BasicImageClassifier(ImageDistanceCalculator<int> calculator) =>
            this.distanceCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));

        public void Train(IEnumerable<ImageObservation> observations) => 
            this.observations = observations ?? throw new ArgumentNullException(nameof(observations));

        public string Classify(IEnumerable<int> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (this.observations == null)
            {
                throw new InvalidOperationException("The classifier has not been trained.");
            }

            // Find the observation from the training set that most closely resembles this image
            // by doing a pixel-by-pixel comparison.            

            return this.observations
                .Select(obs => (label: obs.Label, distance: this.distanceCalculator(obs.Pixels, data)))
                .Aggregate( (min, current) => (current.distance < min.distance) ? current : min)
                .label;
        }
    }
}

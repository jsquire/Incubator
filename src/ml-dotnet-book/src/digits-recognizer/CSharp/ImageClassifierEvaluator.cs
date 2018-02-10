using System;
using System.Collections.Generic;
using System.Linq;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    public static class ImageClassifierEvaluator
    {
        public static double CalculateCorrectPercentage<T>(IEnumerable<ImageObservation<T>>                      validationSet,
                                                           IImageClassifier<ImageObservation<T>, IEnumerable<T>> classifier)
        {
            if (validationSet == null)
            {
                throw new ArgumentNullException(nameof(validationSet));
            }

            if (classifier == null)
            {
                throw new ArgumentNullException(nameof(classifier));
            }

            return validationSet
                .Select(obs => (classifier.Classify(obs.Pixels) == obs.Label) ? 1d : 0d)
                .Average();
        }
                                                           
    }
}

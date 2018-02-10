using System;
using System.Linq;
using System.Collections.Generic;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    /// <summary>
    ///   The signature for functions that compute distances between images.
    /// </summary>
    /// 
    /// <typeparam name="T">The type used to represent the pixels of an image</typeparam>
    /// 
    /// <param name="pixels">The first set of pixels to consider.</param>
    /// <param name="otherPixels">The other set of pixels to consider.</param>
    /// 
    /// <returns>The calculated distance between the two images.</returns>
    /// 
    public delegate double ImageDistanceCalculator<T>(IEnumerable<T> pixels, 
                                                      IEnumerable<T> otherPixels);

    public static class ImageDistanceCalculators
    {
        public static double Manhattan(IEnumerable<int> pixels, 
                                       IEnumerable<int> otherPixels)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (otherPixels == null)
            {
                throw new ArgumentNullException(nameof(otherPixels));
            }

            if (Object.ReferenceEquals(pixels, otherPixels))
            {
                return 0;
            }

            // Yuck.  If this were real code, we'd have to figure a better way to
            // handle this.   For this exercise, it's good enough.

            if (pixels.Count() != otherPixels.Count())
            {
                throw new ArgumentException("Inconsistent image sizes");
            }

            return pixels
                .Zip(otherPixels, (fst, snd) => Math.Abs(fst - snd))
                .Sum();
        }

        public static double Euclidean(IEnumerable<int> pixels, 
                                       IEnumerable<int> otherPixels)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (otherPixels == null)
            {
                throw new ArgumentNullException(nameof(otherPixels));
            }

            if (Object.ReferenceEquals(pixels, otherPixels))
            {
                return 0;
            }

            // Yuck.  If this were real code, we'd have to figure a better way to
            // handle this.   For this exercise, it's good enough.

            if (pixels.Count() != otherPixels.Count())
            {
                throw new ArgumentException("Inconsistent image sizes");
            }

            return Math.Sqrt
            (
                pixels
                    .Zip(otherPixels, (fst, snd) => Math.Pow((fst - snd), 2))
                    .Sum()
            );
        }
    }
    
}

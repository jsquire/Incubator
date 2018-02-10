using System;
using System.Collections.Generic;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    /// <summary>
    ///   An observation made about an image.
    /// </summary>
    /// 
    /// <typeparam name="T">The type representing the pixels of an image.</typeparam>
    /// 
    public class ImageObservation<T>
    {        
        /// <summary>The textual label associated with the image, used to explain what the image represents.</summary>
        public string Label;
        
        /// <summary>The set of pixels that comprise the image.</summary>
        public IEnumerable<T> Pixels;

        /// <summary>
        ///   Instantiates an instance of the <see cref="ImageObservation{T}"/> class.
        /// </summary>
        /// 
        public ImageObservation()
        {
        }

        /// <summary>
        ///   Instantiates an instance of the <see cref="ImageObservation{T}"/> class.
        /// </summary>
        /// 
        /// <param name="label">The label associated with the image.</param>
        /// <param name="pixels">The pixels that comprise the image.</param>
        /// 
        public ImageObservation(string         label, 
                                IEnumerable<T> pixels)
        {
            if (String.IsNullOrEmpty(label))
            {  
                throw new ArgumentNullException(nameof(label));
            }

            this.Label  = label;
            this.Pixels = pixels ?? throw new ArgumentNullException(nameof(pixels));
        }
    }
}

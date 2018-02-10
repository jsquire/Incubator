using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearningBook.DigitsRecognizer.CSharp
{
    public sealed class ImageObservation
    {
        public string Label { get; }
        public IEnumerable<int> Pixels { get; }

        public ImageObservation(string label, IEnumerable<int> pixels)
        {
            this.Label  = label;
            this.Pixels = pixels;
        }
    }
}

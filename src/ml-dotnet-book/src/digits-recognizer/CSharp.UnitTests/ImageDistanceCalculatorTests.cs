using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace MachineLearningBook.DigitsRecognizer.CSharp.UnitTests
{
    public class ImageDistanceCalculatorTests
    {
        private static IEnumerable<ImageDistanceCalculator<int>> AllImageCalculators => 
            typeof(ImageDistanceCalculators)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(calculator => (ImageDistanceCalculator<int>) calculator.CreateDelegate(typeof(ImageDistanceCalculator<int>)) );

        public static IEnumerable<object[]> ImageCalculatorRequiresPixelsData => throw new NotImplementedException();
         
        

        [Theory]
        [MemberData(nameof(ImageCalculatorRequiresPixelsData))]
        public void CalculatorRequiresPixels(ImageDistanceCalculator<int> calculator,
                                             IEnumerable<int> pixels,
                                             IEnumerable<int> otherPixels)
        {
        }
    }
}

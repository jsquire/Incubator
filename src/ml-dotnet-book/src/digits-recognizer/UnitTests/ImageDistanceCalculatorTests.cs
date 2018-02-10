using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace MachineLearningBook.DigitsRecognizer.UnitTests
{
    public class ImageDistanceCalculatorTests
    {   
        private static Lazy<IEnumerable<ImageDistanceCalculator<int>>> AllImageCalculators = new Lazy<IEnumerable<ImageDistanceCalculator<int>>>( () =>
        {
            return typeof(ImageDistanceCalculators)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(calculator => (ImageDistanceCalculator<int>) calculator.CreateDelegate(typeof(ImageDistanceCalculator<int>)));

        }, LazyThreadSafetyMode.PublicationOnly);     
        
        public static IEnumerable<object[]> ImageCalculatorData =>
            ImageDistanceCalculatorTests.AllImageCalculators.Value.Select(calculator => new object[] { calculator });
                
        public static IEnumerable<object[]> CalculatorDisallowsMismatchedPixelSizesData =>
            ImageDistanceCalculatorTests.AllImageCalculators.Value
                .Select(calculator => new object[] { calculator, new int[] { 1, 2, 4 }, new int[] { 1, 2, 3, 4 }});
         
        
        public static IEnumerable<object[]> ImageCalculatorRequiresPixelsData
        {
            get
            {
                foreach (var calculator in ImageDistanceCalculatorTests.AllImageCalculators.Value)
                {
                    yield return new object[] { calculator, new int[0], null };
                    yield return new object[] { calculator, null, new int[0] };
                }
            }
        }

        public static IEnumerable<object[]> ImageCalculatorDetectsTheSameReferenceData
        {
            get
            {
                var pixels      = new[] { 1, 5, 6 };
                var otherPixels = new[] { 1, 5, 7 };

                foreach (var calculator in ImageDistanceCalculatorTests.AllImageCalculators.Value)
                {
                    yield return new object[] { calculator, pixels, pixels, true };
                    yield return new object[] { calculator, pixels, otherPixels, false };
                }
            }
        }

        public static IEnumerable<object[]> ImageCalculatorDetectsEqualSetsData
        {
            get
            {
                var pixels      = new[] { 1, 5, 6 };
                var otherPixels = new[] { 1, 5, 6 };

                foreach (var calculator in ImageDistanceCalculatorTests.AllImageCalculators.Value)
                {
                    yield return new object[] { calculator, pixels, pixels };
                    yield return new object[] { calculator, pixels, otherPixels };
                }
            }
        }

        public static IEnumerable<object[]> ImageCalculatorDetectsDifferentSetsData
        {
            get
            {
                var pixels = new int[] { 1, 5, 6 };

                foreach (var calculator in ImageDistanceCalculatorTests.AllImageCalculators.Value)
                {
                    yield return new object[] { calculator, pixels, new[] { 0, 0, 0 } };
                    yield return new object[] { calculator, pixels, new[] { 6, 5, 1 } };
                    yield return new object[] { calculator, pixels, new[] { 5, 6, 1 } };
                    yield return new object[] { calculator, pixels, new[] { 2, 5, 6 } };
                    yield return new object[] { calculator, pixels, new[] { 2, 7, 5 } };
                    yield return new object[] { calculator, pixels, new[] { 7, 4, 2 } };
                    yield return new object[] { calculator, pixels, new[] { 999, 999, 999 } };
                    yield return new object[] { calculator, pixels, new[] { -1, -5, -6 } };
                    yield return new object[] { calculator, pixels, new[] { -1, 5, 6 } };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ImageCalculatorRequiresPixelsData))]
        public void CalculatorRequiresPixels(ImageDistanceCalculator<int> calculator,
                                             IEnumerable<int>             pixels,
                                             IEnumerable<int>             otherPixels)
        {            
            var nullArg = (pixels == null) ? nameof(pixels) : nameof(otherPixels);
            
            Action actionUnderTest = () => calculator(pixels, otherPixels);
            actionUnderTest.Should().Throw<ArgumentNullException>($"because { nullArg } was null when calling { calculator.Method.Name }");
        }

        [Theory]
        [MemberData(nameof(CalculatorDisallowsMismatchedPixelSizesData))]
        public void CalculatorDisallowsMismatchedPixelSizes(ImageDistanceCalculator<int> calculator,
                                                            IEnumerable<int>             pixels,
                                                            IEnumerable<int>             otherPixels)
        {
            Action actionUnderTest = () => calculator(pixels, otherPixels);
            actionUnderTest.Should().Throw<ArgumentException>($"because the two sets of pixels did not have the same size when calling { calculator.Method.Name }");
        }

        [Theory]
        [MemberData(nameof(ImageCalculatorDetectsTheSameReferenceData))]
        public void ImageCalculatorDetectsTheSameReference(ImageDistanceCalculator<int> calculator,
                                                            IEnumerable<int>            pixels,
                                                            IEnumerable<int>            otherPixels,
                                                            bool                        isSameReference)
        {           
            var result = calculator(pixels, otherPixels);
            
            if (isSameReference)
            {
                result.Should().Be(0, $"because the same instance of pixels was passed when calling { calculator.Method.Name }");
            }
            else
            {
                result.Should().NotBe(0, $"because the a different instance of pixels with different values was passed when calling { calculator.Method.Name }");
            }
        }

        [Theory]
        [MemberData(nameof(ImageCalculatorDetectsEqualSetsData))]
        public void ImageCalculatorDetectsEqualSets(ImageDistanceCalculator<int> calculator,
                                                    IEnumerable<int>             pixels,
                                                    IEnumerable<int>             otherPixels)
        {           
            calculator(pixels, otherPixels).Should().Be(0, $"because the sets were equal when calling { calculator.Method.Name }");
        }

        [Theory]
        [MemberData(nameof(ImageCalculatorDetectsDifferentSetsData))]
        public void ImageCalculatorDetectsDifferentSets(ImageDistanceCalculator<int> calculator,
                                                        IEnumerable<int>             pixels,
                                                        IEnumerable<int>             otherPixels)
        {           
            calculator(pixels, otherPixels).Should().NotBe(0, $"because the sets were not equal when calling { calculator.Method.Name }");
        }

       [Fact]
       public void ManhattanCalculatesDistanceForInverseSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Manhattan;
           var pixels     = new[] { 0, 1, 2 };
           var other      = new[] { 3, 1, 0 };
           var expected   = 5;

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void ManhattanCalculatesDistanceForNegativeSet()
       {   
           
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Manhattan;
           var pixels     = new[] { 0, 1, 2 };
           var other      = new[] { 0, -1, -2 };
           var expected   = 6;

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void ManhattanCalculatesDistanceForCloseSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Manhattan;
           var pixels     = new[] { 1, 1, 1 };
           var other      = new[] { 2, 3, 4 };
           var expected   = 6;

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void ManhattanCalculatesDistanceForFarSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Manhattan;
           var pixels     = new[] { 2, 3, 4 };
           var other      = new[] { 999, -999, 8};
           var expected   = 2003;

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void EuclideanCalculatesDistanceForInverseSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Euclidean;
           var pixels     = new[] { 1, 0, 0 };
           var other      = new[] { 0, 0, 1 };
           var expected   = Math.Sqrt(Math.Pow((double)(pixels[0] - other[0]), 2) + Math.Pow((double)(pixels[1] - other[1]), 2) + Math.Pow((double)(pixels[2] - other[2]), 2));

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void EuclideanCalculatesDistanceForNegativeSet()
       {   
           
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Euclidean;
           var pixels     = new[] { 2 };
           var other      = new[] {-2 };
           var expected   = Math.Sqrt(Math.Pow((double)(pixels[0] - other[0]), 2));

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void EuclideanCalculatesDistanceForCloseSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Euclidean;
           var pixels     = new[] { 1 };
           var other      = new[] { 2 };
           var expected   = Math.Sqrt(Math.Pow((double)(pixels[0] - other[0]), 2));

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }

       [Fact]
       public void EuclideanCalculatesDistanceForFarSet()
       {   
           var calculator = (ImageDistanceCalculator<int>)ImageDistanceCalculators.Euclidean;
           var pixels     = new[] { 2 };
           var other      = new[] { 999 };
           var expected   = Math.Sqrt(Math.Pow((double)(pixels[0] - other[0]), 2));

           calculator(pixels, other).Should().Be(expected, $"because that was the set distance when calling { calculator.Method.Name }");
       }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace MachineLearningBook.DigitsRecognizer.CSharp.UnitTests
{
    public class BasicImageClassifierTests
    {
        private static Lazy<IEnumerable<ImageDistanceCalculator<int>>> AllImageCalculators = new Lazy<IEnumerable<ImageDistanceCalculator<int>>>( () =>
        {
            return typeof(ImageDistanceCalculators)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(calculator => (ImageDistanceCalculator<int>) calculator.CreateDelegate(typeof(ImageDistanceCalculator<int>)));

        }, LazyThreadSafetyMode.PublicationOnly);     
        
        public static IEnumerable<object[]> ImageCalculatorData =>
            BasicImageClassifierTests.AllImageCalculators.Value.Select(calculator => new object[] { calculator });

        [Fact]
        public void ConstructorValidatesCalculator()
        {
            Action actionUnderTest = () => new BasicImageClassifier(null);
            actionUnderTest.ShouldThrow<ArgumentNullException>("becaue the calculator is required");
        }

        [Fact]
        public void TrainValidatesObservations()
        {
            Action actionUnderTest = () => new BasicImageClassifier(ImageDistanceCalculators.Manhattan).Train(null);
            actionUnderTest.ShouldThrow<ArgumentNullException>("becaue the training set of observations is required");
        }

        [Fact]
        public void ClassifyValidatesData()
        {
            Action actionUnderTest = () => new BasicImageClassifier(ImageDistanceCalculators.Manhattan).Classify(null);
            actionUnderTest.ShouldThrow<ArgumentNullException>("becaue the training set of data is required");
        }

        [Fact]
        public void ClassifyValidatesTrainingBeforeClassifying()
        {
            Action actionUnderTest = () => new BasicImageClassifier(ImageDistanceCalculators.Manhattan).Classify(new[] { 1, 2, 3 });
            actionUnderTest.ShouldThrow<InvalidOperationException>("becaue classifier must be trained before classifying");
        }

        [Theory]
        [MemberData(nameof(BasicImageClassifierTests.ImageCalculatorData))]
        public void ClassifyIsSuccessfulWithMatchingSingleSet(ImageDistanceCalculator<int> calculator)
        {
            var observations = new[] { new ImageObservation<int>("Test", new[] { 1, 2, 3 }) };
            var classifier   = new BasicImageClassifier(calculator);

            classifier.Train(observations);
            classifier.Classify(observations[0].Pixels).Should().Be(observations[0].Label, $"because the same single set was used for training and classifying with { calculator.Method.Name } distance");
        }

        [Theory]
        [MemberData(nameof(BasicImageClassifierTests.ImageCalculatorData))]
        public void ClassifyIsSuccessfulWithDifferentSingleSet(ImageDistanceCalculator<int> calculator)
        {
            var data         = new[] { 7, 8, 9 };
            var observations = new[] { new ImageObservation<int>("Test", new[] { 1, 2, 3 }) };
            var classifier   = new BasicImageClassifier(calculator);

            classifier.Train(observations);
            classifier.Classify(data).Should().Be(observations[0].Label, $"because a single set was used for training and classifying with { calculator.Method.Name } distance");
        }

        [Theory]
        [MemberData(nameof(BasicImageClassifierTests.ImageCalculatorData))]
        public void ClassifyIsSuccessfulWithSameSetPresent(ImageDistanceCalculator<int> calculator)
        {
            var data       = new[] { 7, 8, 9 };
            var classifier = new BasicImageClassifier(calculator);

            var observations = new[] 
            { 
                new ImageObservation<int>("Test",     new[] { 1, 2, 3 }),
                new ImageObservation<int>("Expected", data             )
            };
            

            classifier.Train(observations);
            classifier.Classify(data).Should().Be(observations[1].Label, $"because a matching set was present in the training with { calculator.Method.Name } distance");
        }

        [Theory]
        [MemberData(nameof(BasicImageClassifierTests.ImageCalculatorData))]
        public void ClassifyChoosesMinimumDistance(ImageDistanceCalculator<int> calculator)
        {
            var data       = new[] { -1, 0, 789 };
            var classifier = new BasicImageClassifier(calculator);

            var observations = new[] 
            { 
                new ImageObservation<int>("One",   new[] {   0,   0,    0 }),
                new ImageObservation<int>("Two",   new[] { 789,   0,   -1 }),
                new ImageObservation<int>("Three", new[] {   1,   2,    3 }),
                new ImageObservation<int>("Four",  new[] { 999, 888,  777 }),
                new ImageObservation<int>("Five",  new[] {   1,   0, -789 }),
                new ImageObservation<int>("Six",   new[] {  12,   8,  765 })
            };

            var minIndex = 0;
            var value    = 0d;
            var minValue = Double.MaxValue;

            for (var index = 0; index < observations.Length; ++index)
            {
                value = calculator(data, observations[index].Pixels);

                if (value < minValue)
                {
                    minIndex = index;
                    minValue = value;
                }
            }
            
            classifier.Train(observations);
            classifier.Classify(data).Should().Be(observations[minIndex].Label, $"because classifying should choose the image that is least different from the given data with { calculator.Method.Name } distance");
        }
    }
}

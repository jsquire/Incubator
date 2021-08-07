using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;

namespace MachineLearningBook.DigitsRecognizer.UnitTests
{
    public class ImageClassifierEvaludatorTests
    {
        [Fact]
        public void CalculateCorrectPercentageValidatesTheValidationSet()
        {
            Action actionUnderTest = () => ImageClassifierEvaluator.CalculateCorrectPercentage(null, Mock.Of<IImageClassifier<ImageObservation<int>, IEnumerable<int>>>());
            actionUnderTest.Should().Throw<ArgumentNullException>("because the validation set is required");
        }

        [Fact]
        public void CalculateCorrectPercentageValidatesTheClassifier()
        {
            Action actionUnderTest = () => ImageClassifierEvaluator.CalculateCorrectPercentage(new ImageObservation<int>[0], null);
            actionUnderTest.Should().Throw<ArgumentNullException>("because the classifier set is required");
        }

        [Fact]
        public void CalculateCorrectPercentageWhenAlwaysCorrect()
        {
            var expected   = "Expected";
            var classifier = new Mock<IImageClassifier<ImageObservation<int>, IEnumerable<int>>>();

            classifier
                .Setup(target => target.Classify(It.IsAny<IEnumerable<int>>()))
                .Returns(expected);


            var set = new[] 
            {
                new ImageObservation<int>(expected, new[] { 1, 2, 3 }),
                new ImageObservation<int>(expected, new[] { 4, 5, 6 }),
                new ImageObservation<int>(expected, new[] { 7, 8, 9 })
            };

            ImageClassifierEvaluator.CalculateCorrectPercentage(set, classifier.Object)
                .Should().Be(1d, "because the classifier is always correct");
        }

        [Fact]
        public void CalculateCorrectPercentageWhenNeverCorrect()
        {
            var expected   = "Expected";
            var classifier = new Mock<IImageClassifier<ImageObservation<int>, IEnumerable<int>>>();

            classifier
                .Setup(target => target.Classify(It.IsAny<IEnumerable<int>>()))
                .Returns(expected);


            var set = new[] 
            {
                new ImageObservation<int>("blah",  new[] { 1, 2, 3 }),
                new ImageObservation<int>("other", new[] { 4, 5, 6 }),
                new ImageObservation<int>("blah",  new[] { 7, 8, 9 })
            };

            ImageClassifierEvaluator.CalculateCorrectPercentage(set, classifier.Object)
                .Should().Be(0d, "because the classifier is never correct");
        }

        [Fact]
        public void CalculateCorrectPercentageCorrectExactlyHalf()
        {
            var expected   = "Expected";
            var classifier = new Mock<IImageClassifier<ImageObservation<int>, IEnumerable<int>>>();

            classifier
                .Setup(target => target.Classify(It.IsAny<IEnumerable<int>>()))
                .Returns(expected);

            var set = new[] 
            {
                new ImageObservation<int>(expected, new[] { 1, 2, 3 }),
                new ImageObservation<int>("blah",   new[] { 4, 5, 6 }),
                new ImageObservation<int>(expected, new[] { 7, 8, 9 }),
                new ImageObservation<int>("rando",  new[] { 2, 3, 4 }),
            };

            ImageClassifierEvaluator.CalculateCorrectPercentage(set, classifier.Object)
                .Should().Be(0.5d, "because the classifier is correct exactly half the time");
        }
    }
}

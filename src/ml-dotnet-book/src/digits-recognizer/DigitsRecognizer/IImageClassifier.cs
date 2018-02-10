using System.Collections.Generic;

namespace MachineLearningBook.DigitsRecognizer
{
    /// <summary>
    ///   A classifier for image data, serving as the unit of organization for training and executing
    ///   a learning model.
    /// </summary>
    /// 
    /// <typeparam name="TObservation">The type of observation used to train the classifier</typeparam>
    /// <typeparam name="TObservation">The type of input data that is being classified</typeparam>
    /// 
    public interface IImageClassifier<TObservation, TData>
    {
        /// <summary>
        ///   Trains the classifier using the provided observations.
        /// </summary>
        /// 
        /// <param name="observations">The observations to use for training.</param>
        /// 
        void Train(IEnumerable<TObservation> observations);

        /// <summary>
        ///   Classifies the specified data.
        /// </summary>
        /// 
        /// <param name="data">The data to consider for classification.</param>
        /// 
        /// <returns>A textual representation of the image classification.</returns>
        /// 
        string Classify(TData data);
    }
}

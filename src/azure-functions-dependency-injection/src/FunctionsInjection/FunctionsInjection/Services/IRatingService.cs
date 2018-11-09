using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionsInjection.Models;

namespace FunctionsInjection.Services
{
    /// <summary>
    ///   The contract to be implemented by review serivces.
    /// </summary>
    /// 
    public interface IRatingService
    {
        /// <summary>
        ///   Validates a review, ensuring that the fields conform to expected business constraints.
        /// </summary>
        /// 
        /// <param name="review">The review to validate.</param>
        /// 
        /// <returns>A pair indicating whether or not the review was valid and a set of errors/messages for those fields that were not.</returns>
        /// 
        Task<(bool, IEnumerable<ErrorResult>)> ValidateReviewAsync(ProductRating review);
    }
}

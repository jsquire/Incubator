using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionsInjection.Models;

namespace FunctionsInjection.Services
{
    /// <summary>
    ///   The service responsible for review-related operations.
    /// </summary>
    /// 
    /// <seealso cref="FunctionsInjection.Services.IRatingService" />
    /// 
    public class ReviewService : IRatingService
    {
        /// <summary>
        ///   Validates a review, ensuring that the fields conform to expected business constraints.
        /// </summary>
        /// 
        /// <param name="review">The review to validate.</param>
        /// 
        /// <returns>A pair indicating whether or not the review was valid and a set of errors/messages for those fields that were not.</returns>
        /// 
        public Task<(bool, IEnumerable<ErrorResult>)> ValidateReviewAsync(ProductRating review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            var errors = new List<ErrorResult>();

            if ((review.Id == null) || (review.Id == Guid.Empty))
            {
                errors.Add(new ErrorResult(nameof(review.Id), new[] { "The identifier must be populated" }));
            }

            if (review.UserId == Guid.Empty)
            {
                errors.Add(new ErrorResult(nameof(review.UserId), new[] { "The user identifier must be populated" }));
            }

            if (review.ProductId == Guid.Empty)
            {
                errors.Add(new ErrorResult(nameof(review.ProductId), new[] { "The product identifier must be populated" }));
            }

            if (String.IsNullOrEmpty(review.LocationName))
            {
                errors.Add(new ErrorResult(nameof(review.LocationName), new[] { "The location must be populated" }));
            }
            else if (review.LocationName.Length > 50)
            {
                errors.Add(new ErrorResult(nameof(review.LocationName), new[] { "The location must no more than 50 characters" }));
            }

            if ((review.Timestamp == null) || (review.Timestamp == default(DateTime)))
            {
                errors.Add(new ErrorResult(nameof(review.LocationName), new[] { "The location must be populated" }));
            }
            
            if (review.Rating > 5)
            {
                errors.Add(new ErrorResult(nameof(review.Rating), new[] { "The rating must be between 0 and 5" }));
            }

            if ((review.UserNotes != null) && (review.UserNotes.Length > 250))
            {
                errors.Add(new ErrorResult(nameof(review.UserNotes), new[] { "The user notes must no more than 250 characters" }));
            }

            return Task.FromResult(((errors.Count <= 0), (IEnumerable<ErrorResult>)errors));
        }
    }
}

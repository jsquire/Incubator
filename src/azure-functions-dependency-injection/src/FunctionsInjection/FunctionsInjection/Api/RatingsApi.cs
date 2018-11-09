
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FunctionsInjection.Infrastructure;
using FunctionsInjection.Models;
using FunctionsInjection.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunctionsInjection
{
    /// <summary>
    ///   Serves as the host for functions providing the Ratings API endpoints.
    /// </summary>
    /// 
    public static class RatingsApi
    {
        /// <summary>The default container instance to use for dependency resolution.</summary>
        private static readonly Lazy<IContainer> defaultContainerInstance;

        /// <summary>
        ///   The default container to be used for dependency resolution.
        /// </summary>
        /// 
        /// <value>
        ///   The default container, recognizing configuration from both environment variables and the 
        ///   default local setttings file.
        /// </value>
        /// 
        public static IContainer DefaultContainer => RatingsApi.defaultContainerInstance.Value;

        /// <summary>
        ///   Initializes the <see cref="RatingsApi"/> class.
        /// </summary>
        /// 
        static RatingsApi()
        {
            RatingsApi.defaultContainerInstance = new Lazy<IContainer>(() => AzureFunctionDependencyResolver.CreateContainer(Environment.CurrentDirectory, "local.settings.json"), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        ///   Gets all ratings recorded for a given user.
        /// </summary>
        /// 
        /// <param name="request">The incoming HTTP request.</param>
        /// <param name="ratings">The set of ratings recorded for the given <paramref name="user"/>.</param>
        /// <param name="user">The user for whome ratings were requested.</param>
        /// <param name="log">The <see cref="ILogger" /> reference to use for logging.</param>
        /// 
        /// <returns>The set of ratings recorded for the specified <paramref name="user"/>.</returns>
        /// 
        [FunctionName(nameof(GetRatingsForUser))]
        public static IActionResult GetRatingsForUser([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "users/{user}/ratings/")]HttpRequest request, 
                                                      [CosmosDB("%RatingsDatabaseName%", "%RatingsCollectionName%", ConnectionStringSetting="RatingsStoreConnection", PartitionKey ="{user}", SqlQuery = "SELECT * FROM collection WHERE collection.UserId={user}")] IEnumerable<ProductRating> ratings,
                                                      string  user,
                                                      ILogger log)
        {
            log.LogInformation($"Get Ratings request received for user: [{user}].");
            return new OkObjectResult(ratings);
        }

        /// <summary>
        ///   Gets the requested rating recorded for a given user.
        /// </summary>
        /// 
        /// <param name="request">The incoming HTTP request.</param>
        /// <param name="ratings">The specified rating.</param>
        /// <param name="user">The user for whome ratings were requested.</param>
        /// <param name="rating">The rating that is being requested.</param>
        /// <param name="log">The <see cref="ILogger" /> reference to use for logging.</param>
        /// 
        /// <returns>The set of ratings recorded for the specified <paramref name="user"/>.</returns>
        /// 
        [FunctionName(nameof(GetRatingForUser))]
        public static IActionResult GetRatingForUser([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "users/{user}/ratings/{rating}")]HttpRequest request, 
                                                     [CosmosDB("%RatingsDatabaseName%", "%RatingsCollectionName%", ConnectionStringSetting="RatingsStoreConnection", PartitionKey ="{user}", SqlQuery = "SELECT * FROM collection WHERE collection.Id={rating}")] IEnumerable<ProductRating> ratings,
                                                     string  user,
                                                     string  rating,
                                                     ILogger log)
        {
            log.LogInformation($"Get Rating [{rating}] request received for user: [{user}].");
            return new OkObjectResult(ratings?.FirstOrDefault());
        }

        /// <summary>
        ///   Gets the requested rating recorded for a given user.
        /// </summary>
        /// 
        /// <param name="request">The incoming HTTP request.</param>
        /// <param name="ratings">The specified rating.</param>
        /// <param name="user">The user for whome ratings were requested.</param>
        /// <param name="rating">The rating that is being requested.</param>
        /// <param name="log">The <see cref="ILogger" /> reference to use for logging.</param>
        /// 
        /// <returns>The set of ratings recorded for the specified <paramref name="user"/>.</returns>
        /// 
        [FunctionName(nameof(CreateRatingForUser))]
        public static IActionResult CreateRatingForUser([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "users/{user}/ratings")]ProductRating ratingRequest, 
                                                        [CosmosDB("%RatingsDatabaseName%", "%RatingsCollectionName%", ConnectionStringSetting="RatingsStoreConnection", PartitionKey ="{user}", CreateIfNotExists = true)] out ProductRating rating,
                                                        string  user,
                                                        ILogger log)
        {
            log.LogInformation($"Create reating request received for user: [{user}].");

            // Default the rating to null, so that nothing gets created in the data store on failures.

            rating = null;
            
            // Verify that there was a rating passed, and that rating is for the correct user.
            if (ratingRequest == null)
            {
                return new BadRequestObjectResult(new ErrorResult("Body", new[] { "The desired rating must be passed as part of the POST body" }));
            }

            if ((!Guid.TryParse(user, out var resourceUser)) || (ratingRequest.UserId != resourceUser))
            {
                // ForbidResult was causing an exception due to its reliance on the authentication manager in the HttpContext.

                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            // Ignore consiering extraneous data as a bad request.  Though it may have been set when 
            // being posted, ignore any given identifier and generate one.  If the
            // timestamp was passed, preserve it; otherwise, generate a new one.

            ratingRequest.Id        = Guid.NewGuid();
            ratingRequest.Timestamp = ratingRequest.Timestamp ?? DateTime.UtcNow;

            // Verify the rating is valid.  Since this happens asynchronously and the output binding
            // does not allow for asyncronous support, wrap it into a single processing function to make it easier to
            // force it back to synchronous.

            using (var scope = RatingsApi.DefaultContainer.BeginLifetimeScope())
            {
                var (valid, errors) = 
                    RatingsApi.ValidateCreateRatingAsync(ratingRequest, scope.Resolve<IProductService>(), scope.Resolve<IUserService>(), scope.Resolve<IRatingService>())
                        .GetAwaiter()
                        .GetResult();

                if (!valid)
                {
                    return new BadRequestObjectResult(errors);
                }
                
                rating = ratingRequest;
                return new OkObjectResult(ratingRequest);
            }
        }

        /// <summary>
        ///   Gets the requested rating.
        /// </summary>
        /// 
        /// <param name="request">The incoming HTTP request.</param>
        /// <param name="ratings">The specified rating.</param>
        /// <param name="user">The user for whome ratings were requested.</param>
        /// <param name="rating">The rating that is being requested.</param>
        /// <param name="log">The <see cref="ILogger" /> reference to use for logging.</param>
        /// 
        /// <returns>The set of ratings recorded for the specified <paramref name="user"/>.</returns>
        /// 
        [FunctionName(nameof(GetRating))]
        public static IActionResult GetRating([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "ratings/{rating}")]HttpRequest request, 
                                               [CosmosDB("%RatingsDatabaseName%", "%RatingsCollectionName%", ConnectionStringSetting="RatingsStoreConnection", SqlQuery = "SELECT * FROM collection WHERE collection.Id={rating}")] IEnumerable<ProductRating> ratings,
                                               string  rating,
                                               ILogger log)
        {
            log.LogInformation($"Get Rating [{rating}] request received.");
            return new OkObjectResult(ratings?.FirstOrDefault());
        }

        /// <summary>
        ///   Validates a rating that is intended for creation.
        /// </summary>
        /// 
        /// <param name="rating">The rating to consider.</param>
        /// <param name="productService">The ervice to use for product-related operations.</param>
        /// <param name="userService">The serivce to use for user-related operations.</param>
        /// <param name="ratingService">The service to use for rating-related operations.</param>
        /// 
        /// <returns>A pair indicating whether or not the review was valid and a set of errors/messages for those fields that were not.</returns>
        /// 
        private static async Task<(bool, IEnumerable<ErrorResult>)> ValidateCreateRatingAsync(ProductRating   rating,
                                                                                              IProductService productService,
                                                                                              IUserService    userService,
                                                                                              IRatingService  ratingService)
        {
            async Task<(bool, IEnumerable<ErrorResult>)> Validate(Func<Task<bool>> servicecCall, string memberName, string errorMessage)
            {
              var result = await servicecCall();
              var error  = result ? Enumerable.Empty<ErrorResult>() : (IEnumerable<ErrorResult>)new[] { new ErrorResult(memberName, new[] { errorMessage}) };
              
              return (result, error);
            }

            var results = (await Task.WhenAll<(bool, IEnumerable<ErrorResult>)>
            (
                Validate(() => productService.ProductExistsAsync(rating.ProductId), nameof(rating.ProductId), "The product does not exist"),
                Validate(() => userService.UserExistsAsync(rating.UserId), nameof(rating.UserId), "The user does not exist"),
                ratingService.ValidateReviewAsync(rating)
            ));

            var errors = results
               .Where(result => !result.Item1)
               .SelectMany(result => result.Item2)
               .ToList();

            return (!errors.Any(), errors);
        }
    }
}

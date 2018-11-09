using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FunctionsInjection.Infrastructure;

namespace FunctionsInjection.Services
{
    /// <summary>
    ///   The service responsible for product-related operations.
    /// </summary>
    /// 
    /// <seealso cref="FunctionsInjection.Services.IProductService" />
    /// 
    public class ProductService : IProductService
    {
        /// <summary>The HTTP client instance to use for any outgoing requests.</summary>
        private readonly HttpClient httpClient;

        /// <summary>The configuration which specifies the service-related values.</summary>
        private readonly ApplicationConfiguration configuration;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// 
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpClient">The configuration which specifies the service-related values..</param>
        /// 
        public ProductService(ApplicationConfiguration configuration, 
                              HttpClient               httpClient)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClient    = httpClient    ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> ProductExistsAsync(Guid productIdentifier)
        {
            if (String.IsNullOrEmpty(this.configuration?.GetProductUrlFormatMask))
            {
                throw new MissingDependencyException("GetUserUrlFormatMask was not configured");
            }

            var timeout  = TimeSpan.FromSeconds(this.configuration.ExternalRequestTimeoutSeconds);
            var response = default(HttpResponseMessage);

            // Make the request to the user service, bypassing the response body since we only care about the result code.

            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                response = await this.httpClient
                    .GetAsync(String.Format(this.configuration.GetProductUrlFormatMask, productIdentifier), HttpCompletionOption.ResponseHeadersRead, tokenSource.Token)
                    .WithTimeout(timeout, tokenSource);
            }

            // Consider an HTTP 404 (Not Found) and HTTP 400 (Bad Request) as a sign that the user does not exist, due to quirks in the
            // user API.  Anything other non-success should be considered a failure in the service.

            switch (response.StatusCode)
            {
                case var status when response.IsSuccessStatusCode:
                    return true;

                case var status when ((status == HttpStatusCode.NotFound) || (status == HttpStatusCode.BadRequest)):
                    return false;

                default:
                    throw new ServiceUnavailableException("The user service was not available.  Please retry at a later point.");
                    
            }
        }
    }
}

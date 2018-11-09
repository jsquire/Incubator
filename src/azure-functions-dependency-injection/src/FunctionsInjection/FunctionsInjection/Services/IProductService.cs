using System;
using System.Threading.Tasks;

namespace FunctionsInjection.Services
{
    /// <summary>
    ///   The contract to be implemented by product serivces.
    /// </summary>
    /// 
    public interface IProductService
    {
        /// <summary>
        ///   Determines if a product exists.
        /// </summary>
        /// 
        /// <param name="productIdentifier">The identifier of the product to consider.</param>
        /// 
        /// <returns><c>true</c> if the product exists; otherwise, <c>false</c>.</returns>
        /// 
        Task<bool> ProductExistsAsync(Guid productIdentifier);
    }
}

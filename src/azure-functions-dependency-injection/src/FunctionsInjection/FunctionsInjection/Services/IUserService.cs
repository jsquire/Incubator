using System;
using System.Threading.Tasks;

namespace FunctionsInjection.Services
{
    /// <summary>
    ///   The contract to be implemented by user serivces.
    /// </summary>
    /// 
    public interface IUserService
    {
        /// <summary>
        ///   Determines if a user exists.
        /// </summary>
        /// 
        /// <param name="userIdentifier">The identifier of the user to consider.</param>
        /// 
        /// <returns><c>true</c> if the user exists; otherwise, <c>false</c>.</returns>
        /// 
        Task<bool> UserExistsAsync(Guid userIdentifier);
    }
}

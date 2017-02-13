using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Sample.AspNetCore.BasicAuth
{
    /// <summary>
    /// Specifies callback methods which the <see cref="BasicMiddleware"></see> invokes to enable developer control over the authentication process.
    /// </summary>
    public interface IBasicEvents
    {
        /// <summary>
        /// Called when a request came with basic authentication credentials. By implementing this method the credentials can be converted to
        /// a principal.
        /// </summary>
        /// <param name="context">Contains information about the sign in request.</param>
        Task SignIn(BasicSignInContext context);

        /// <summary>
        /// Called when an exception occurs during request or response processing.
        /// </summary>
        /// <param name="context">Contains information about the exception that occurred</param>
        Task Exception(BasicExceptionContext context);
    }
}

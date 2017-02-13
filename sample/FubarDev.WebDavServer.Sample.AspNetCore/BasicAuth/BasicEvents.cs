using System;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Sample.AspNetCore.BasicAuth
{
    /// <summary>
    /// This default implementation of the <see cref="IBasicEvents"/> may be used if the
    /// application only needs to override a few of the interface methods. This may be used as a base class
    /// or may be instantiated directly.
    /// </summary>
    public class BasicEvents : IBasicEvents
    {
        /// <summary>
		/// A delegate assigned to this property will be invoked when the related method is called
		/// </summary>
		public Func<BasicSignInContext, Task> OnSignIn { get; set; } = context => Task.FromResult(0);

		/// <summary>
		/// A delegate assigned to this property will be invoked when the related method is called
		/// </summary>
		public Func<BasicExceptionContext, Task> OnException { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SignIn(BasicSignInContext context) => OnSignIn(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        public virtual Task Exception(BasicExceptionContext context) => OnException(context);
    }
}

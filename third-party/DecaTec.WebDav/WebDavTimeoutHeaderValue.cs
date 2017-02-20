using System;
using System.Text;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing the timeout values used by WebDAV.
    /// </summary>
    public class WebDavTimeoutHeaderValue
    {
        private string timeout;
        private string alternativeTimeout;

        private WebDavTimeoutHeaderValue()
        {
        }

        /// <summary>
        /// Creates a new WebDavTimeoutHeaderValue with infinite timeout.
        /// </summary>
        /// <returns>A a new WebDavTimeoutHeaderValue with infinite timeout.</returns>
        public static WebDavTimeoutHeaderValue CreateInfiniteWebDavTimeout()
        {
            var t = new WebDavTimeoutHeaderValue();
            t.timeout = "Infinite";
            return t;
        }

        /// <summary>
        /// Creates a new WebDavTimeoutHeaderValue with the timeout specified.
        /// </summary>
        /// <param name="timeout">The timeout as <see cref="TimeSpan"/>.</param>
        /// <returns>A new WebDavTimeoutHeaderValue with the timeout specified.</returns>
        public static WebDavTimeoutHeaderValue CreateWebDavTimeout(TimeSpan timeout)
        {
            var t = new WebDavTimeoutHeaderValue();
            t.timeout = "Second-" + timeout.TotalSeconds;
            return t;
        }

        /// <summary>
        /// Creates a new WebDavTimeoutHeaderValue with infinite timeout and an alternative timeout.
        /// </summary>
        /// <param name="alternativeTimeout">The alternative timeout as <see cref="TimeSpan"/>.</param>
        /// <returns>A new WebDavTimeoutHeaderValue with infinite timeout and an alternative timeout.</returns>
        public static WebDavTimeoutHeaderValue CreateInfiniteWebDavTimeoutWithAlternative(TimeSpan alternativeTimeout)
        {
            var t = CreateInfiniteWebDavTimeout();
            t.alternativeTimeout = "Second-" + alternativeTimeout.TotalSeconds;
            return t;
        }

        /// <summary>
        /// Gets the string representation of this WebDavTimeoutHeaderValue.
        /// </summary>
        /// <returns>The string representation of this WebDavTimeoutHeaderValue.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(timeout))
                sb.Append(timeout);

            if (!string.IsNullOrEmpty(alternativeTimeout))
            {
                sb.Append(", ");
                sb.Append(alternativeTimeout);
            }

            return sb.ToString();
        }
    }
}

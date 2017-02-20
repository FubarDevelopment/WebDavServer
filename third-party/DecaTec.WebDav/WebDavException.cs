using System;
using System.Runtime.Serialization;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class for WebDAV exceptions.
    /// </summary>
    [DataContract]
    public class WebDavException : Exception
    {
        /// <summary>
        /// Initializes a new instance of WebDavException.
        /// </summary>
        public WebDavException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public WebDavException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public WebDavException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

using System.Net;
using System.Net.Http;
using System.Linq;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing a WebDAV response.
    /// </summary>
    public class WebDavResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of WebDavResponseMessage.
        /// </summary>
        public WebDavResponseMessage()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of WebDavResponseMessage.
        /// </summary>
        /// <param name="statusCode">The request's HttpStatusCode.</param>
        public WebDavResponseMessage(HttpStatusCode statusCode)
            : base(statusCode)
        {

        }

        /// <summary>
        /// Initializes a new instance of WebDavResponseMessage.
        /// </summary>
        /// <param name="statusCode">The request's <see cref="WebDavStatusCode"/>.</param>
        public WebDavResponseMessage(WebDavStatusCode statusCode)
            : base((HttpStatusCode)statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavResponseMessage.
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/> the WebDavResponseMessage should be based on.</param>
        public WebDavResponseMessage(HttpResponseMessage httpResponseMessage)
            : base()
        {
            this.Content = httpResponseMessage.Content;
            this.ReasonPhrase = httpResponseMessage.ReasonPhrase;
            this.RequestMessage = httpResponseMessage.RequestMessage;
            this.StatusCode = (WebDavStatusCode)httpResponseMessage.StatusCode;
            this.Version = httpResponseMessage.Version;

            // Transfer headers.
            foreach (var header in httpResponseMessage.Headers)
            {
                if (header.Key == WebDavConstants.ETag && header.Value != null && header.Value.Any())
                {
                    // Workaround when server returns invalid header (e.g."686897696a7c876b7e" instead of "\"686897696a7c876b7e\"").
                    var eTagStr = header.Value.FirstOrDefault();

                    if (!string.IsNullOrEmpty(eTagStr) && !eTagStr.StartsWith("\"") && !eTagStr.EndsWith("\""))
                        Headers.Add(header.Key, "\"" + eTagStr + "\"");                    
                }
                else
                {
                    // This wrapper is supposed to be transparent.
                    // Assume the headers in the httpResponseMessage are always valid
                    // otherwise they should not have been there in the first place.
                    this.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="WebDavStatusCode"/> of this WebDavRespnseMessage.
        /// </summary>
        public new WebDavStatusCode StatusCode
        {
            get
            {
                return (WebDavStatusCode)base.StatusCode;
            }
            set
            {
                base.StatusCode = (HttpStatusCode)value;
            }
        }
    }
}

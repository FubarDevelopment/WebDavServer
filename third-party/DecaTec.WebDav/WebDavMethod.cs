using System.Net.Http;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing WebDAV methods.
    /// </summary>
    public sealed class WebDavMethod : HttpMethod
    {
        /// <summary>
        /// WebDavMethod for PROPFIND.
        /// </summary>
        public static readonly WebDavMethod PropFind = new WebDavMethod("PROPFIND");

        /// <summary>
        /// WebDavMethod for PROPPATCH.
        /// </summary>
        public static readonly WebDavMethod PropPatch = new WebDavMethod("PROPPATCH");

        /// <summary>
        ///  WebDavMethod for MKCOL.
        /// </summary>
        public static readonly WebDavMethod Mkcol = new WebDavMethod("MKCOL");

        /// <summary>
        ///  WebDavMethod for COPY.
        /// </summary>
        public static readonly WebDavMethod Copy = new WebDavMethod("COPY");

        /// <summary>
        ///  WebDavMethod for MOVE.
        /// </summary>
        public static readonly WebDavMethod Move = new WebDavMethod("MOVE");

        /// <summary>
        ///  WebDavMethod for LOCK.
        /// </summary>
        public static readonly WebDavMethod Lock = new WebDavMethod("LOCK");

        /// <summary>
        ///  WebDavMethod for UNLOCK.
        /// </summary>
        public static readonly WebDavMethod Unlock = new WebDavMethod("UNLOCK");

        /// <summary>
        /// Initializes a new instance of WebDavMethod.
        /// </summary>
        /// <param name="method">The method name.</param>
        private WebDavMethod(string method)
            : base(method)
        {

        }
    }
}

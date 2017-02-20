
namespace DecaTec.WebDav
{
    /// <summary>
    /// Class defining the headers specific for WebDAV.
    /// </summary>
    public static class WebDavRequestHeader
    {
        /// <summary>
        /// Constant for Depth header. It may have values defined in <see cref="WebDavDepthHeaderValue"/>.
        /// </summary>
        public const string Depth = "Depth";

        /// <summary>
        /// Constant for Destination header specifying the destination for COPY and MOVE commands.
        /// </summary>
        public const string Destination = "Destination";

        /// <summary>
        /// Constant for If header. This is used to submit lock tokens with a request.
        /// </summary>
        public const string If = "If";

        /// <summary>
        /// Constant for Lock-Token header. This is used with an UNLOCK request.
        /// </summary>
        public const string LockTocken = "Lock-Token";

        /// <summary>
        /// Constant for Overwrite Header.
        /// </summary>
        public const string Overwrite = "Overwrite";

        /// <summary>
        /// Constant for Timeout header. This is used to define the timeout values submitted by a LOCk command.
        /// </summary>
        public const string Timeout = "Timeout";
    }
}

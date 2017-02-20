
namespace DecaTec.WebDav
{
    /// <summary>
    /// Class defining the values for the WebDAV Overwrite header.
    /// </summary>
    public static class WebDavOverwriteHeaderValue
    {
        /// <summary>
        /// Constant for overwrite header value when a resource should be overwritten on a copy or move operation ('F').
        /// </summary>
        public const string NoOverwrite = "F";

        /// <summary>
        /// Constant for overwrite header value when a resource should not be overwritten on a copy or move operation ('T').
        /// </summary>
        public const string Overwrite = "T";
    }
}

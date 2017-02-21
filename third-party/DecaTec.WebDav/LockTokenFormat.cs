
namespace DecaTec.WebDav
{
    /// <summary>
    /// Represents the different lock token formats used with WebDAV.
    /// </summary>
    public enum LockTokenFormat
    {
        /// <summary>
        /// Lock token format as used in IF headers.
        /// </summary>
        IfHeader,

        /// <summary>
        /// Lock token format as used in Lock-Token headers.
        /// </summary>
        LockTokenHeader
    }
}

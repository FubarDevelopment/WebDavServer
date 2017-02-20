
namespace DecaTec.WebDav
{
    /// <summary>
    /// Class defining the WebDAV authentication types.
    /// </summary>
    public class WebDavAuthenticationType
    {
        private string authType;

        /// <summary>
        /// Basic authentication.
        /// </summary>
        public static readonly WebDavAuthenticationType Basic = new WebDavAuthenticationType("Basic");

        /// <summary>
        /// NTLM authentication.
        /// </summary>
        public static readonly WebDavAuthenticationType Ntlm = new WebDavAuthenticationType("NTLM");

        /// <summary>
        /// Digest authentication.
        /// </summary>
        public static readonly WebDavAuthenticationType Digest = new WebDavAuthenticationType("Digest");

        /// <summary>
        /// Kerberos authentication.
        /// </summary>
        public static readonly WebDavAuthenticationType Kerberos = new WebDavAuthenticationType("Kerberos");

        /// <summary>
        /// The authentication is negotiated between server and client.
        /// </summary>
        public static readonly WebDavAuthenticationType Negotiate = new WebDavAuthenticationType("Negotiate");

        private WebDavAuthenticationType(string authType)
        {
            this.authType = authType;
        }

        /// <summary>
        /// Gets the string representation of the WebDavAuthenticationType.
        /// </summary>
        /// <returns>The string representation of the WebDavAuthenticationType.</returns>
        public override string ToString()
        {
            return this.authType;
        }
    }
}

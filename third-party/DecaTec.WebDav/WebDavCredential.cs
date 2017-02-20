using System;
using System.Net;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class for credentials of a WebDAV request.
    /// </summary>
    /// <remarks>As there is a problem using HttpClient and a (custom) class implementing ICredentials with a UWP app, the class WebDavCredential is not longer supported.
    /// Use <see cref="NetworkCredential"/> instead.</remarks>
    public class WebDavCredential : ICredentials, ICredentialsByHost
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of WebDavCredential with basic authentication.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        public WebDavCredential(string userName, string password)
            : this(userName, password, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new instance of WebDavCredential with basic authentication.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        public WebDavCredential(string userName, string password, string domain)
            : this(userName, password, domain, null)
        {
        }

        /// <summary>
        /// Creates a new instance of WebDavCredential.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="webDavAuthenticationType">The <see cref="DecaTec.WebDav.WebDavAuthenticationType"/>.</param>
        public WebDavCredential(string userName, string password, string domain, WebDavAuthenticationType webDavAuthenticationType)
        {
            this.UserName = userName;
            this.Password = password;
            this.Domain = domain;
            this.WebDavAuthenticationType = webDavAuthenticationType;

            throw new WebDavException("As there is a problem using HttpClient and a (custom) class implementing ICredentials with a UWP app, the class WebDavCredential is not longer supported. Use NetworkCredential instead.");
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        public string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavAuthenticationType"/>.
        /// </summary>
        public WebDavAuthenticationType WebDavAuthenticationType
        {
            get;
            set;
        }

        #endregion Properties

        #region Public methods

        /// <summary>
        /// Gets the <see cref="NetworkCredential"/> for a <see cref="Uri"/> and authentication type specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/>.</param>
        /// <param name="authType">The <see cref="DecaTec.WebDav.WebDavAuthenticationType"/>.</param>
        /// <returns>The <see cref="NetworkCredential"/>.</returns>
        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            var authenticationType = this.WebDavAuthenticationType == null ? authType : this.WebDavAuthenticationType.ToString();
            var credentialCache = new CredentialCache();
            credentialCache.Add(uri, authenticationType, new NetworkCredential(this.UserName, this.Password, this.Domain));
            return credentialCache.GetCredential(uri, authType);
        }

        /// <summary>
        /// Gets the <see cref="NetworkCredential"/> for a host/port and authentication type specified.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="authType">The <see cref="DecaTec.WebDav.WebDavAuthenticationType"/>.</param>
        /// <returns>The <see cref="NetworkCredential"/>.</returns>
        public NetworkCredential GetCredential(string host, int port, string authType)
        {
            var authenticationType = this.WebDavAuthenticationType == null ? authType : this.WebDavAuthenticationType.ToString();
            var credentialCache = new CredentialCache();
            credentialCache.Add(host, port, authenticationType, new NetworkCredential(this.UserName, this.Password, this.Domain));
            return credentialCache.GetCredential(host, port, authType);
        }

        #endregion Public methods
    }
}

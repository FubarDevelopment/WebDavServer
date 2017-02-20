using DecaTec.WebDav.WebDavArtifacts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class for WebDAV sessions.
    /// </summary>
    /// <remarks>
    /// <para>This class acts as an abstraction layer between the application and the <see cref="DecaTec.WebDav.WebDavClient"/>, which is used to communicate with the WebDAV server.</para>
    /// <para>If you want to communicate with the WebDAV server directly, you should use the <see cref="DecaTec.WebDav.WebDavClient"/>.</para>
    /// <para>The WebDavSession can be used with a base URL/<see cref="System.Uri"/>. If such a base URL/<see cref="System.Uri"/> is specified, all subsequent operations involving an 
    /// URL/<see cref="System.Uri"/> will be relative on this base URL/<see cref="System.Uri"/>.
    /// If no base URL/<see cref="System.Uri"/> is specified, all operations has the be called with an absolute URL/<see cref="System.Uri"/>.</para>
    /// </remarks>
    /// <example>See the following code to list the content of a directory with the WebDavSession:
    /// <code>
    /// // You have to add a reference to DecaTec.WebDav.NetFx.dll.
    /// //
    /// // Specify the user credentials and use it to create a WebDavSession instance.
    /// var credentials = new NetworkCredential("MyUserName", "MyPassword");
    /// var webDavSession = new WebDavSession(@"http://www.myserver.com/webdav/", credentials);
    /// var items = await webDavSession.ListAsync(@"MyFolder/");
    ///
    /// foreach (var item in items)
    /// {
    ///     Console.WriteLine(item.Name);
    /// }
    /// 
    /// // Dispose the WebDavSession when it is not longer needed.
    /// webDavSession.Dispose();
    /// </code>
    /// <para></para>
    /// See the following code which uses locking with a WebDavSession:
    /// <code>
    /// // Specify the user credentials and use it to create a WebDavSession instance.
    /// var credentials = new NetworkDavCredential("MyUserName", "MyPassword");
    /// var webDavSession = new WebDavSession(@"http://www.myserver.com/webdav/", credentials);
    /// await webDavSession.LockAsync(@"Test/");
    ///
    /// // Create new folder and delete it.
    /// // You DO NOT have to care about that the folder is locked (i.e. you do not have to submit a lock token).
    /// // This is all handled by the WebDavSession itself.
    /// await webDavSession.CreateDirectoryAsync("MyFolder/NewFolder");
    /// await webDavSession.DeleteAsync("MyFolder/NewFolder");
    ///
    /// // Unlock the folder again.
    /// await webDavSession.UnlockAsync(@"MyFolder/");
    ///
    /// // You should always call Dispose on the WebDavSession when it is not longer needed.
    /// // During Dispose, all locks held by the WebDavSession will be automatically unlocked.
    /// webDavSession.Dispose();
    /// </code>
    /// </example>
    /// <seealso cref="DecaTec.WebDav.WebDavClient"/>
    public partial class WebDavSession : IDisposable
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of WebDavSession with a default <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="networkCredential">The <see cref="NetworkCredential"/> to use.</param>
        public WebDavSession(NetworkCredential networkCredential)
            : this(new HttpClientHandler() { PreAuthenticate = true, Credentials = networkCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base URL and a default <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for this WebDavSession.</param>
        /// <param name="networkCredential">The <see cref="NetworkCredential"/> to use.</param>
        public WebDavSession(string baseUrl, NetworkCredential networkCredential)
            : this(new Uri(baseUrl), new HttpClientHandler() { PreAuthenticate = true, Credentials = networkCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base <see cref="Uri"/> and a default <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> to use for this WebDavSession.</param>
        /// <param name="networkCredential">The <see cref="NetworkCredential"/> to use.</param>
        public WebDavSession(Uri baseUri, NetworkCredential networkCredential)
            : this(baseUri, new HttpClientHandler() { PreAuthenticate = true, Credentials = networkCredential })
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the <see cref="HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="httpMessageHandler">The <see cref="HttpMessageHandler"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(HttpMessageHandler httpMessageHandler)
            : this(string.Empty, httpMessageHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base URL and the <see cref="HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for this WebDavSession.</param>
        /// <param name="httpMessageHandler">The <see cref="HttpMessageHandler"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(string baseUrl, HttpMessageHandler httpMessageHandler)
            : this(string.IsNullOrEmpty(baseUrl) ? null : new Uri(baseUrl), httpMessageHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of WebDavSession with the given base <see cref="Uri"/> and the <see cref="HttpMessageHandler"/> specified.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> to use for this WebDavSession.</param>
        /// <param name="httpMessageHandler">The <see cref="HttpMessageHandler"/> to use with this WebDavSession.</param>
        /// <remarks>If credentials are needed, these are part of the <see cref="HttpMessageHandler"/> and are specified with it.</remarks>
        public WebDavSession(Uri baseUri, HttpMessageHandler httpMessageHandler)
        {
            this.permanentLocks = new ConcurrentDictionary<Uri, PermanentLock>();
            this.webDavClient = CreateWebDavClient(httpMessageHandler);
            this.BaseUri = baseUri;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IWebProxy"/> to use with this WebDavSession.
        /// </summary>
        public IWebProxy WebProxy
        {
            get;
            set;
        }

        #endregion Properties

        #region Public methods

        #region Download file

        /// <summary>
        ///  Downloads a file from the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the file to download.</param>
        /// <param name="localStream">The <see cref="Stream"/> to save the file to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(Uri uri, Stream localStream)
        {
            return await DownloadFileAsync(uri, localStream, CancellationToken.None);
        }

        /// <summary>
        ///  Downloads a file from the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="localStream">The <see cref="Stream"/> to save the file to.</param>
        /// <param name="ct">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(string url, Stream localStream, CancellationToken ct)
        {
            return await DownloadFileAsync(new Uri(url, UriKind.RelativeOrAbsolute), localStream, ct);
        }

        /// <summary>
        ///  Downloads a file from the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the file to download.</param>
        /// <param name="localStream">The <see cref="Stream"/> to save the file to.</param>
        /// <param name="ct">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(Uri uri, Stream localStream, CancellationToken ct)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var response = await this.webDavClient.GetAsync(uri, ct);

            if (response.Content != null)
            {
                try
                {
                    var contentStream = await response.Content.ReadAsStreamAsync();
                    await contentStream.CopyToAsync(localStream);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
            else
                return false;
        }

        #endregion Download file

        #region List

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the <see cref="Uri"/> specified using the <see cref="PropFind"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the directory which content should be listed. Has to be an absolute URI (including the base URI) or a relative URI (relative to base URI).</param>
        /// <param name="propFind">The <see cref="PropFind"/> to use. Different PropFind  types can be created using the static methods of the class <see cref="PropFind"/>.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IList<WebDavSessionListItem>> ListAsync(Uri uri, PropFind propFind)
        {
            if (propFind == null)
                throw new ArgumentException("Argument propFind must not be null.");

            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var response = await this.webDavClient.PropFindAsync(uri, WebDavDepthHeaderValue.One, propFind);

            // Remember the original port to include it in the hrefs later.
            var port = UriHelper.GetPort(uri);

            if (response.StatusCode != WebDavStatusCode.MultiStatus)
                throw new WebDavException(string.Format("Error while executing ListAsync (wrong response status code). Expected status code: 207 (MultiStatus); actual status code: {0} ({1})", (int)response.StatusCode, response.StatusCode));

            var multistatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content);

            var itemList = new List<WebDavSessionListItem>();

            foreach (var responseItem in multistatus.Response)
            {
                var webDavSessionItem = new WebDavSessionListItem();

                Uri href = null;

                if (!string.IsNullOrEmpty(responseItem.Href))
                {
                    if (Uri.TryCreate(responseItem.Href, UriKind.RelativeOrAbsolute, out href))
                    {
                        var fullQualifiedUri = UriHelper.CombineUri(uri, href, true);
                        fullQualifiedUri = UriHelper.SetPort(fullQualifiedUri, port);
                        webDavSessionItem.Uri = fullQualifiedUri;
                    }
                }

                // Skip the folder which contents were requested, only add children.
                if (href != null && WebUtility.UrlDecode(UriHelper.RemovePort(uri).ToString().Trim('/')).EndsWith(WebUtility.UrlDecode(UriHelper.RemovePort(href).ToString().Trim('/')), StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var item in responseItem.Items)
                {
                    var propStat = item as Propstat;

                    // Do not items where no properties could be found.
                    if (propStat == null || propStat.Status.ToLower().Contains("404 not found"))
                        continue;

                    // Do not add hidden items.
                    if (!string.IsNullOrEmpty(propStat.Prop.IsHidden) && propStat.Prop.IsHidden.Equals("1"))
                        continue;

                    webDavSessionItem.ContentClass = propStat.Prop.ContentClass;
                    webDavSessionItem.ContentLanguage = propStat.Prop.GetContentLanguage;

                    if (!string.IsNullOrEmpty(propStat.Prop.GetContentLength))
                        webDavSessionItem.ContentLength = long.Parse(propStat.Prop.GetContentLength, CultureInfo.InvariantCulture);

                    webDavSessionItem.ContentType = propStat.Prop.GetContentType;

                    if (propStat.Prop.CreationDateSpecified && !string.IsNullOrEmpty(propStat.Prop.CreationDate))
                        webDavSessionItem.CreationDate = DateTime.Parse(propStat.Prop.CreationDate, CultureInfo.InvariantCulture);

                    webDavSessionItem.DefaultDocument = propStat.Prop.DefaultDocument;
                    webDavSessionItem.DisplayName = propStat.Prop.DisplayName;
                    webDavSessionItem.ETag = propStat.Prop.GetEtag;

                    if (!string.IsNullOrEmpty(propStat.Prop.GetLastModified))
                        webDavSessionItem.LastModified = DateTime.Parse(propStat.Prop.GetLastModified, CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(propStat.Prop.IsReadonly))
                        webDavSessionItem.IsReadonly = propStat.Prop.IsReadonly.Equals("1");

                    if (!string.IsNullOrEmpty(propStat.Prop.IsRoot))
                        webDavSessionItem.IsRoot = propStat.Prop.IsRoot.Equals("1");

                    if (!string.IsNullOrEmpty(propStat.Prop.IsStructuredDocument))
                        webDavSessionItem.IsStructuredDocument = propStat.Prop.IsStructuredDocument.Equals("1");

                    if (!string.IsNullOrEmpty(propStat.Prop.LastAccessed))
                        webDavSessionItem.LastAccessed = DateTime.Parse(propStat.Prop.LastAccessed, CultureInfo.InvariantCulture);

                    webDavSessionItem.Name = propStat.Prop.Name;
                    webDavSessionItem.ParentName = propStat.Prop.ParentName;

                    if (!string.IsNullOrEmpty(propStat.Prop.QuotaAvailableBytes))
                        webDavSessionItem.QuotaAvailableBytes = long.Parse(propStat.Prop.QuotaAvailableBytes, CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(propStat.Prop.QuotaUsedBytes))
                        webDavSessionItem.QuotaUsedBytes = long.Parse(propStat.Prop.QuotaUsedBytes, CultureInfo.InvariantCulture);

                    // Make sure that the IsDirectory property is set if it's a directory.
                    if (!string.IsNullOrEmpty(propStat.Prop.IsCollection))
                        webDavSessionItem.IsCollection = propStat.Prop.IsCollection.Equals("1");
                    else if (propStat.Prop.ResourceType != null && propStat.Prop.ResourceType.Collection != null)
                        webDavSessionItem.IsCollection = true;

                    // Make sure that the name property is set.
                    // Naming priority:
                    // 1. displayname (only if it doesn't contain raw unicode, otherwise there are problems with non western characters)
                    // 2. name
                    // 3. (part of) URI.
                    if (!TextHelper.StringContainsRawUnicode(propStat.Prop.DisplayName))
                        webDavSessionItem.Name = propStat.Prop.DisplayName;

                    if (string.IsNullOrEmpty(webDavSessionItem.Name))
                        webDavSessionItem.Name = propStat.Prop.Name;

                    if (string.IsNullOrEmpty(webDavSessionItem.Name) && href != null)
                        webDavSessionItem.Name = WebUtility.UrlDecode(href.ToString().Split('/').Last(x => !string.IsNullOrEmpty(x)));
                }

                itemList.Add(webDavSessionItem);
            }

            return itemList;
        }

        #endregion List

        #region Upload file

        /// <summary>
        /// Uploads a file to the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the file to upload.</param>
        /// <param name="localStream">The <see cref="Stream"/> containing the file to upload.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> UploadFileAsync(Uri uri, Stream localStream)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var lockToken = GetAffectedLockToken(uri);
            var content = new StreamContent(localStream);
            var response = await this.webDavClient.PutAsync(uri, content, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Upload file

        #endregion Public methods

        #region Private methods

        private static WebDavClient CreateWebDavClient(HttpMessageHandler messageHandler)
        {
            return new WebDavClient(messageHandler, false);
        }

        #endregion Private methods
    }
}

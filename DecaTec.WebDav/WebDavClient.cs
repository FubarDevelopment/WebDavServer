using DecaTec.WebDav.WebDavArtifacts;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Provides a class for sending WebDAV requests and receiving WebDAV responses from a resource identified by <see cref="Uri"/>.
    /// </summary>
    /// <remarks>
    /// <para>WebDavClient inherits from <see cref="System.Net.Http.HttpClient"/> and adds WebDAV specific methods.</para>
    /// <para>It implements the <see href="http://tools.ietf.org/html/rfc4918">RFC 4918</see> specification and can be used to communicate directly with a WebDAV server.</para>
    /// <para>For most use cases regarding WebDAV communication, the <see cref="DecaTec.WebDav.WebDavSession"/> is sufficient because it hides most of the WebDAV specific communication 
    /// and provides an easy access to WebDAV servers.</para>
    /// <example>To send a propfind request you can use following code:
    /// <code>
    /// // You have to add a reference to DecaTec.WebDav.NetFx.dll.
    /// //
    /// // Specify the user credentials and pass it to a HttpClientHandler.
    /// var credentials = new NetworkCredential("MyUserName", "MyPassword");
    /// var httpClientHandler = new HttpClientHandler();
    /// httpClientHandler.Credentials = credentials;
    /// httpClientHandler.PreAuthenticate = true;
    ///
    /// // Use the HttpClientHandler to create the WebDavClient.
    /// var webDavClient = new WebDavClient(httpClientHandler);
    ///
    /// // Create a PropFind object with represents a so called 'allprop' request.
    /// PropFind pf = PropFind.CreatePropFindAllProp();
    /// var response = await webDavClient.PropFindAsync(@"http://www.myserver.com/webdav/MyFolder/", WebDavDepthHeaderValue.Infinity, pf);
    ///
    /// // You could also use an XML string directly for use with the WebDavClient.
    /// //var xmlString = "&lt;?xml version=\&quot;1.0\&quot; encoding=\&quot;utf-8\&quot;?&gt;&lt;D:propfind xmlns:D=\&quot;DAV:\&quot;&gt;&lt;D:allprop /&gt;&lt;/D:propfind&gt;";
    /// //var response = await webDavClient.PropFindAsync(@"http://www.myserver.com/webdav/MyFolder/", WebDavDepthHeaderValue.Infinity, xmlString);
    ///
    /// // Use the WebDavResponseContentParser to parse the response message and get a MultiStatus instance (this is also an async method).
    /// var multistatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content);
    ///
    /// // Now you can use the MultiStatus object to get access to the items properties.
    /// foreach (var responseItem in multistatus.Response)
    /// {
    ///     // Handle propfind multistatus response, e.g responseItem.Href is the URL of an item (folder or file).
    ///     Console.WriteLine(responseItem.Href);
    /// }
    /// </code>
    /// <para></para>
    /// See the following code which demonstrates locking using a WebDavClient:
    /// <code>
    /// // You have to add references to System.Net.Http and DecaTec.WebDav.
    /// //
    /// // Specify the user credentials and pass it to a HttpClientHandler.
    /// var credentials = new NetworkCredential("MyUserName", "MyPassword");
    /// var httpClientHandler = new HttpClientHandler();
    /// httpClientHandler.Credentials = credentials;
    /// httpClientHandler.PreAuthenticate = true;
    ///
    /// // Use the HttpClientHandler to create the WebDavClient.
    /// var webDavClient = new WebDavClient(httpClientHandler);
    ///
    /// // Create a LockInfo object with is needed for locking.
    /// // We are using the class LockInfo (from DecaTec.WebDav.WebDavArtifacts) to avoid building a XML string directly.
    /// var lockInfo = new LockInfo();
    /// lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
    /// lockInfo.LockType = LockType.CreateWriteLockType();
    /// lockInfo.Owner = new OwnerHref("test@test.com");
    ///
    /// // Lock the desired folder by specifying a WebDavTimeOutHeaderValue (in this example, the timeout should be infinite), a value for depth and the LockInfo.
    /// var lockResult = await webDavClient.LockAsync(@"http://www.myserver.com/webdav/MyFolder/", WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(), WebDavDepthHeaderValue.Infinity, lockInfo);
    ///            
    /// // On successful locking, a lock token will be returned by the WebDAV server.
    /// // We have to save this lock token in order to use it on operations which affect a locked folder.
    /// LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(lockResult);       
    ///
    /// // Now create a new folder in the locked location.
    /// // Notice that the LockToken has to be specified as a locked folder is affected by this operation.
    /// // If the LockToken would not be specified, the operation will fail!
    /// await webDavClient.MkcolAsync(@"http://www.myserver.com/webdav/MyFolder/NewFolder/", lockToken);
    ///
    /// // Delete the folder again.
    /// await webDavClient.DeleteAsync(@"http://www.myserver.com/webdav/MyFolder/NewFolder/", lockToken);
    ///
    /// // Unlock the locked folder.
    /// // Notice that the URL is the same as used with the lock method (see above).
    /// await webDavClient.UnlockAsync(@"http://www.myserver.com/webdav/MyFolder/", lockToken);
    /// </code>
    /// </example>
    /// </remarks>
    /// <seealso cref="DecaTec.WebDav.WebDavSession"/>
    public class WebDavClient : HttpClient
    {
        private static readonly XmlSerializer MultistatusSerializer = new XmlSerializer(typeof(Multistatus));
        private static readonly XmlSerializer PropFindSerializer = new XmlSerializer(typeof(PropFind));
        private static readonly XmlSerializer PropertyUpdateSerializer = new XmlSerializer(typeof(PropertyUpdate));
        private static readonly XmlSerializer LockInfoSerializer = new XmlSerializer(typeof(LockInfo));

        private const string MediaTypeXml = "application/xml";

        #region Constructor

        /// <summary>
        /// Initializes a new instance of WebDavClient.
        /// </summary>
        public WebDavClient()
            : base()
        {

        }

        /// <summary>
        ///  Initializes a new instance of WebDavClient.
        /// </summary>
        /// <param name="httpMessageHandler">The <see cref="HttpMessageHandler"/> responsible for processing the HTTP response messages.</param>
        public WebDavClient(HttpMessageHandler httpMessageHandler)
            : base(httpMessageHandler)
        {

        }

        /// <summary>
        ///  Initializes a new instance of WebDavClient.
        /// </summary>
        /// <param name="httpMessageHandler">The <see cref="HttpMessageHandler"/> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler">True if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
        public WebDavClient(HttpMessageHandler httpMessageHandler, bool disposeHandler)
            : base(httpMessageHandler, disposeHandler)
        {

        }

        #endregion Constructor        

        #region Copy

        /// <summary>
        /// Copies a resource from the source URL to the destination URL (Depth = 'infinity', Overwrite = false).
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(string sourceUrl, string destinationUrl)
        {
            return await CopyAsync(new Uri(sourceUrl), new Uri(destinationUrl), false, WebDavDepthHeaderValue.Infinity, null);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/> (Depth = 'infinity', Overwrite = false).
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/> .</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(Uri sourceUri, Uri destinationUri)
        {
            return await CopyAsync(sourceUri, destinationUri, false, WebDavDepthHeaderValue.Infinity, null);
        }

        /// <summary>
        /// Copies a resource from the source URL to the destination URL.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await CopyAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, WebDavDepthHeaderValue.Infinity, null);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            return await CopyAsync(sourceUri, destinationUri, overwrite, WebDavDepthHeaderValue.Infinity, null);
        }

        /// <summary>
        /// Copies a resource from the source URL to the destination URL.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> for the copy command. On collections, depth must be '0' or 'infinity'.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(string sourceUrl, string destinationUrl, bool overwrite, WebDavDepthHeaderValue depth)
        {
            return await CopyAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, depth, null);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> for the copy command. On collections, depth must be '0' or 'infinity'.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite, WebDavDepthHeaderValue depth)
        {
            return await CopyAsync(sourceUri, destinationUri, overwrite, depth, null);
        }

        /// <summary>
        /// Copies a resource from the source URL to the destination URL.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> for the copy command. On collections, depth must be '0' or 'infinity'.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(string sourceUrl, string destinationUrl, bool overwrite, WebDavDepthHeaderValue depth, LockToken lockTokenDestination)
        {
            return await CopyAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, depth, lockTokenDestination);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> for the copy command. On collections, depth must be '0' or 'infinity'.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite, WebDavDepthHeaderValue depth, LockToken lockTokenDestination)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.Copy, sourceUri);
            // Destination header must be present on copy commands.
            requestMethod.Headers.Add(WebDavRequestHeader.Destination, destinationUri.ToString());

            if (depth != null)
            {
                // On collections: Depth must be '0' or 'infinity'.
                requestMethod.Headers.Add(WebDavRequestHeader.Depth, depth.ToString());
            }

            if (overwrite)
                requestMethod.Headers.Add(WebDavRequestHeader.Overwrite, WebDavOverwriteHeaderValue.Overwrite);
            else
                requestMethod.Headers.Add(WebDavRequestHeader.Overwrite, WebDavOverwriteHeaderValue.NoOverwrite);

            if (lockTokenDestination != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockTokenDestination.ToString(LockTokenFormat.IfHeader));

            var taskHttpResponseMessage = await this.SendAsync(requestMethod);
            return taskHttpResponseMessage;
        }

        #endregion Copy

        #region Delete

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> DeleteAsync(string requestUrl)
        {
            return await DeleteAsync(new Uri(requestUrl), null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a DELETE request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> DeleteAsync(Uri requestUri)
        {
            return await DeleteAsync(requestUri, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> DeleteAsync(string requestUrl, LockToken lockToken)
        {
            return await DeleteAsync(new Uri(requestUrl), lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Sends a DELETE request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> DeleteAsync(Uri requestUri, LockToken lockToken)
        {
            return await DeleteAsync(requestUri, lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> DeleteAsync(string requestUrl, CancellationToken cancellationToken)
        {
            return await DeleteAsync(new Uri(requestUrl), null, cancellationToken);
        }

        /// <summary>
        /// Sends a DELETE request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> DeleteAsync(string requestUrl, LockToken lockToken, CancellationToken cancellationToken)
        {
            return await DeleteAsync(new Uri(requestUrl), lockToken, cancellationToken);
        }

        /// <summary>
        /// Sends a DELETE request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return await DeleteAsync(requestUri, null, cancellationToken);
        }

        /// <summary>
        /// Sends a DELETE request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> DeleteAsync(Uri requestUri, LockToken lockToken, CancellationToken cancellationToken)
        {
            // On collections: Clients must not use any other value for the Depth header but 'infinity'.
            // A DELETE command without depth header will be treated by the server as if Depth = 'infinity' was used.
            // Thus, no Depth header is explicitly specified.
            var requestMethod = new HttpRequestMessage(WebDavMethod.Delete, requestUri);

            if (lockToken != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, HttpCompletionOption.ResponseContentRead, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Delete

        #region Get

        /// <summary>
        /// Send a GET request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(string requestUrl)
        {
            return await GetAsync(new Uri(requestUrl), HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(Uri requestUri)
        {
            return await GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(string requestUrl, CancellationToken cancellationToken)
        {
            return await GetAsync(new Uri(requestUrl), HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a GET request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return await GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a GET request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(string requestUrl, HttpCompletionOption completionOption)
        {
            return await GetAsync(new Uri(requestUrl), completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return await GetAsync(requestUri, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(string requestUrl, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await GetAsync(new Uri(requestUrl), completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a GET request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/>  value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var httpResponseMessage = await base.GetAsync(requestUri, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Get

        #region Head

        /// <summary>
        /// Send a HEAD request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(string requestUrl)
        {
            return await HeadAsync(new Uri(requestUrl), HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a HEAD request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(Uri requestUri)
        {
            return await HeadAsync(requestUri, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a HEAD request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(string requestUrl, HttpCompletionOption completionOption)
        {
            return await HeadAsync(new Uri(requestUrl), completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a HEAD request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return await HeadAsync(requestUri, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a HEAD request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(string requestUrl, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await HeadAsync(new Uri(requestUrl), completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a HEAD request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> HeadAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(HttpMethod.Head, requestUri);
            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Head

        #region Lock

        #region Set lock

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo)
        {
            return await LockAsync(requestUrl, timeout, depth, lockInfo, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfoXmlString">The XML string specifying which item should be locked.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockInfoXmlString)
        {
            return await LockAsync(requestUrl, timeout, depth, lockInfoXmlString, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo)
        {
            return await LockAsync(requestUri, timeout, depth, lockInfo, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockinfoXmlString">The XML string specifying which item should be locked.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockinfoXmlString)
        {
            return await LockAsync(requestUri, timeout, depth, lockinfoXmlString, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo, HttpCompletionOption completionOption)
        {
            return await LockAsync(requestUrl, timeout, depth, lockInfo, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfoXmlString">The XML string specifying which item should be locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockInfoXmlString, HttpCompletionOption completionOption)
        {
            return await LockAsync(requestUrl, timeout, depth, lockInfoXmlString, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo, HttpCompletionOption completionOption)
        {
            return await LockAsync(requestUri, timeout, depth, lockInfo, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockinfoXmlString">The XML string specifying which item should be locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockinfoXmlString, HttpCompletionOption completionOption)
        {
            return await LockAsync(requestUri, timeout, depth, lockinfoXmlString, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await LockAsync(new Uri(requestUrl), timeout, depth, lockInfo, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfoXmlString">The XML string specifying which item should be locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockInfoXmlString, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await LockAsync(new Uri(requestUrl), timeout, depth, lockInfoXmlString, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockInfo">The <see cref="LockInfo"/> object specifying the lock.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, LockInfo lockInfo, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            string requestContentString = string.Empty;

            if (lockInfo != null)
                requestContentString = WebDavHelper.GetUtf8EncodedXmlWebDavRequestString(LockInfoSerializer, lockInfo);

            return await LockAsync(requestUri, timeout, depth, requestContentString, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="lockinfoXmlString">The XML string specifying which item should be locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> LockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, WebDavDepthHeaderValue depth, string lockinfoXmlString, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (depth == WebDavDepthHeaderValue.One)
                throw new WebDavException("Values other than '0' or 'infinity' must not be used on a LOCK command.");

            var requestMethod = new HttpRequestMessage(WebDavMethod.Lock, requestUri);

            if (depth != null)
                requestMethod.Headers.Add(WebDavRequestHeader.Depth, depth.ToString());

            if (timeout != null)
                requestMethod.Headers.Add(WebDavRequestHeader.Timeout, timeout.ToString());

            if (!String.IsNullOrEmpty(lockinfoXmlString))
            {
                var httpContent = new StringContent(lockinfoXmlString, Encoding.UTF8, MediaTypeXml);
                requestMethod.Content = httpContent;
            }

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Set lock

        #region Refresh lock

        /// <summary>
        /// Send a LOCK request to the specified URL in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, LockToken lockToken)
        {
            return await RefreshLockAsync(new Uri(requestUrl), timeout, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/> in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, LockToken lockToken)
        {
            return await RefreshLockAsync(requestUri, timeout, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await RefreshLockAsync(new Uri(requestUrl), timeout, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/> in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await RefreshLockAsync(requestUri, timeout, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a LOCK request to the specified URL in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(string requestUrl, WebDavTimeoutHeaderValue timeout, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await RefreshLockAsync(new Uri(requestUrl), timeout, lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a LOCK request to the specified <see cref="Uri"/> in order to refresh an already existing lock.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="timeout">The <see cref="WebDavTimeoutHeaderValue"/> to use for the lock. The server might ignore this timeout.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of the lock which should be refreshed.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> RefreshLockAsync(Uri requestUri, WebDavTimeoutHeaderValue timeout, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (lockToken == null)
                throw new WebDavException("No lock token specified. A lock token is required to refresh a lock.");

            var requestMethod = new HttpRequestMessage(WebDavMethod.Lock, requestUri);

            if (timeout != null)
                requestMethod.Headers.Add(WebDavRequestHeader.Timeout, timeout.ToString());

            requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Refresh lock

        #endregion Lock

        #region Mkcol

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl)
        {
            return await MkcolAsync(new Uri(requestUrl), null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri)
        {
            return await MkcolAsync(requestUri, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl, LockToken lockToken)
        {
            return await MkcolAsync(new Uri(requestUrl), lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri, LockToken lockToken)
        {
            return await MkcolAsync(requestUri, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl, HttpCompletionOption completionOption)
        {
            return await MkcolAsync(new Uri(requestUrl), null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await MkcolAsync(new Uri(requestUrl), lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return await MkcolAsync(requestUri, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await MkcolAsync(requestUri, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MkcolAsync(new Uri(requestUrl), null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Creates a collection at the URL specified.
        /// </summary>
        /// <param name="requestUrl">The URL of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(string requestUrl, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MkcolAsync(new Uri(requestUrl), lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        ///  Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MkcolAsync(requestUri, null, completionOption, cancellationToken);
        }

        /// <summary>
        ///  Creates a collection at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> of the collection to create.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MkcolAsync(Uri requestUri, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.Mkcol, requestUri);

            if (lockToken != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Mkcol

        #region Move

        /// <summary>
        /// Moves a resource to another URL (Overwrite = false).
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), false, null, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/> (Overwrite = false).
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri)
        {
            return await MoveAsync(sourceUri, destinationUri, false, null, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another URL (Overwrite = false).
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, null, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/> (Overwrite = false).
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            return await MoveAsync(sourceUri, destinationUri, overwrite, null, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, lockTokenSource, lockTokenDestination, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination)
        {
            return await MoveAsync(sourceUri, destinationUri, overwrite, lockTokenSource, lockTokenDestination, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite, HttpCompletionOption completionOption)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, null, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination, HttpCompletionOption completionOption)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, lockTokenSource, lockTokenDestination, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite, HttpCompletionOption completionOption)
        {
            return await MoveAsync(sourceUri, destinationUri, overwrite, null, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination, HttpCompletionOption completionOption)
        {
            return await MoveAsync(sourceUri, destinationUri, overwrite, lockTokenSource, lockTokenDestination, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Moves a resource to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, null, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Moves a resource to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the resource which should be moved.</param>
        /// <param name="destinationUrl">The target URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MoveAsync(new Uri(sourceUrl), new Uri(destinationUrl), overwrite, lockTokenSource, lockTokenDestination, completionOption, cancellationToken);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await MoveAsync(sourceUri, destinationUri, overwrite, null, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Moves a resource to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the resource which should be moved.</param>
        /// <param name="destinationUri">The target <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <param name="lockTokenSource">The <see cref="LockToken"/> of the source. Specify null if the source is not locked.</param>
        /// <param name="lockTokenDestination">The <see cref="LockToken"/> of the destination. Specify null if the destination is not locked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite, LockToken lockTokenSource, LockToken lockTokenDestination, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.Move, sourceUri);
            // Destination header must be present on MOVE commands.
            requestMethod.Headers.Add(WebDavRequestHeader.Destination, destinationUri.ToString());

            // On Collections: Clients must not submit other Depth header than 'infinity', any other depth header does not make sense on non-collections.
            // So set the depth header always to 'infinity'.
            requestMethod.Headers.Add(WebDavRequestHeader.Depth, WebDavDepthHeaderValue.Infinity.ToString());

            if (lockTokenSource != null || lockTokenSource != null)
            {
                var sb = new StringBuilder();

                if (lockTokenSource != null)
                    sb.Append(lockTokenSource.ToString(LockTokenFormat.IfHeader));

                if (lockTokenDestination != null)
                    sb.Append(lockTokenDestination.ToString(LockTokenFormat.IfHeader));

                if (lockTokenSource != null)
                    requestMethod.Headers.Add(WebDavRequestHeader.If, sb.ToString());
            }

            if (overwrite)
                requestMethod.Headers.Add(WebDavRequestHeader.Overwrite, WebDavOverwriteHeaderValue.Overwrite);
            else
                requestMethod.Headers.Add(WebDavRequestHeader.Overwrite, WebDavOverwriteHeaderValue.NoOverwrite);

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Move

        #region Post

        /// <summary>
        /// Send a POST request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PostAsync(string requestUrl, HttpContent content)
        {
            return await PostAsync(new Uri(requestUrl), content, null, CancellationToken.None);
        }

        /// <summary>
        /// Send a POST request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return await PostAsync(requestUri, content, null, CancellationToken.None);
        }

        /// <summary>
        /// Send a POST request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PostAsync(string requestUrl, HttpContent content, LockToken lockToken)
        {
            return await PostAsync(new Uri(requestUrl), content, lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Send a POST request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PostAsync(Uri requestUri, HttpContent content, LockToken lockToken)
        {
            return await PostAsync(requestUri, content, lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Send a POST request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PostAsync(string requestUrl, HttpContent content, CancellationToken cancellationToken)
        {
            return await PostAsync(new Uri(requestUrl), content, null, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PostAsync(string requestUrl, HttpContent content, LockToken lockToken, CancellationToken cancellationToken)
        {
            return await PostAsync(new Uri(requestUrl), content, lockToken, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            return await PostAsync(requestUri, content, null, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PostAsync(Uri requestUri, HttpContent content, LockToken lockToken, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.Post, requestUri);
            requestMethod.Content = content;

            if (lockToken != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, HttpCompletionOption.ResponseContentRead, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Post

        #region Propfind

        /// <summary>
        /// Send a PROPFIND request to the specified URL (Depth header = '1' and 'allprop').
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>       
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl)
        {
            return await PropFindAsync(new Uri(requestUrl), WebDavDepthHeaderValue.One, string.Empty, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/> (Depth header = '1' and 'allprop').
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>       
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri)
        {
            return await PropFindAsync(requestUri, WebDavDepthHeaderValue.One, string.Empty, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL ('allprop').
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>       
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, string.Empty, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/> ('allprop')
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>       
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth)
        {
            return await PropFindAsync(requestUri, depth, string.Empty, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, string propfindXmlString)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfindXmlString, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, string propfindXmlString)
        {
            return await PropFindAsync(requestUri, depth, propfindXmlString, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, PropFind propfind)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfind, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, PropFind propfind)
        {
            return await PropFindAsync(requestUri, depth, propfind, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpContent"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, string propfindXmlString, HttpCompletionOption completionOption)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfindXmlString, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, string propfindXmlString, HttpCompletionOption completionOption)
        {
            return await PropFindAsync(requestUri, depth, propfindXmlString, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, PropFind propfind, HttpCompletionOption completionOption)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfind, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, PropFind propfind, HttpCompletionOption completionOption)
        {
            return await PropFindAsync(requestUri, depth, propfind, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, string propfindXmlString, CancellationToken cancellationToken)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfindXmlString, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, string propfindXmlString, CancellationToken cancellationToken)
        {
            return await PropFindAsync(requestUri, depth, propfindXmlString, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, PropFind propfind, CancellationToken cancellationToken)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfind, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, PropFind propfind, CancellationToken cancellationToken)
        {
            return await PropFindAsync(requestUri, depth, propfind, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/>  value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, string propfindXmlString, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfindXmlString, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(string requestUrl, WebDavDepthHeaderValue depth, PropFind propfind, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropFindAsync(new Uri(requestUrl), depth, propfind, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfind">The <see cref="PropFind"/> object specifying which properties should be searched for. If null is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, PropFind propfind, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            string requestContentString = string.Empty;

            if (propfind != null)
                requestContentString = WebDavHelper.GetUtf8EncodedXmlWebDavRequestString(PropFindSerializer, propfind);

            return await PropFindAsync(requestUri, depth, requestContentString, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a PROPFIND request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="depth">The <see cref="WebDavDepthHeaderValue"/> to use for the operation.</param>
        /// <param name="propfindXmlString">The XML string specifying which items should be returned by the response. If an empty string is specified here, a so called 'allprop' request will be sent.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropFindAsync(Uri requestUri, WebDavDepthHeaderValue depth, string propfindXmlString, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            // Client must submit a depth header.
            if (depth == null)
                throw new WebDavException("A Depth header must be present on a PROPFIND command.");

            var requestMethod = new HttpRequestMessage(WebDavMethod.PropFind, requestUri);

            // If Depth = 'infinity', the server could response with 403 (Forbidden) when Depth = 'infinity' is not supported.
            requestMethod.Headers.Add(WebDavRequestHeader.Depth, depth.ToString());

            if (!String.IsNullOrEmpty(propfindXmlString))
            {
                var httpContent = new StringContent(propfindXmlString, Encoding.UTF8, MediaTypeXml);
                requestMethod.Content = httpContent;
            }

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Propfind

        #region Proppatch

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString)
        {
            return await PropPatchAsync(requestUri, propPatchXmlString, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate)
        {
            return await PropPatchAsync(requestUri, propertyUpdate, null, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString, LockToken lockToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate, LockToken lockToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString, LockToken lockToken)
        {
            return await PropPatchAsync(requestUri, propPatchXmlString, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate, LockToken lockToken)
        {
            return await PropPatchAsync(requestUri, propertyUpdate, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(requestUri, propPatchXmlString, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>n.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(requestUri, propPatchXmlString, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(requestUri, propertyUpdate, null, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await PropPatchAsync(requestUri, propertyUpdate, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString , HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, string propPatchXmlString, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propPatchXmlString, lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate , HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(string requestUrl, PropertyUpdate propertyUpdate, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropPatchAsync(new Uri(requestUrl), propertyUpdate, lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate , HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            string requestContentString = string.Empty;

            if (propertyUpdate != null)
                requestContentString = WebDavHelper.GetUtf8EncodedXmlWebDavRequestString(PropertyUpdateSerializer, propertyUpdate);

            return await PropPatchAsync(requestUri, requestContentString, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propertyUpdate">A <see cref="PropertyUpdate"/> which specifies the properties and values which should be updated.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, PropertyUpdate propertyUpdate, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            string requestContentString = string.Empty;

            if (propertyUpdate != null)
                requestContentString = WebDavHelper.GetUtf8EncodedXmlWebDavRequestString(PropertyUpdateSerializer, propertyUpdate);

            return await PropPatchAsync(requestUri, requestContentString, lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString , HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await PropPatchAsync(requestUri, propPatchXmlString, null, completionOption, cancellationToken);
        }

        /// <summary>
        /// Sends a PROPPATCH request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="propPatchXmlString">The XML string specifying which changes to which properties should be sent with this request.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PropPatchAsync(Uri requestUri, string propPatchXmlString, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.PropPatch, requestUri);

            if (lockToken != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            if (!String.IsNullOrEmpty(propPatchXmlString))
            {
                var httpContent = new StringContent(propPatchXmlString, Encoding.UTF8, MediaTypeXml);
                requestMethod.Content = httpContent;
            }

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Proppatch

        #region Put

        /// <summary>
        /// Send a PUT request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> content sent to the server.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PutAsync(string requestUrl, HttpContent content)
        {
            return await PutAsync(new Uri(requestUrl), content, null, CancellationToken.None);
        }

        /// <summary>
        /// Send a PUT request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return await PutAsync(requestUri, content, null, CancellationToken.None);
        }

        /// <summary>
        /// Send a PUT request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PutAsync(string requestUrl, HttpContent content, LockToken lockToken)
        {
            return await PutAsync(new Uri(requestUrl), content, lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Send a PUT request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PutAsync(Uri requestUri, HttpContent content, LockToken lockToken)
        {
            return await PutAsync(requestUri, content, lockToken, CancellationToken.None);
        }

        /// <summary>
        /// Send a PUT request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PutAsync(string requestUrl, HttpContent content , CancellationToken cancellationToken)
        {
            return await PutAsync(new Uri(requestUrl), content, null, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PutAsync(string requestUrl, HttpContent content, LockToken lockToken, CancellationToken cancellationToken)
        {
            return await PutAsync(new Uri(requestUrl), content, lockToken, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> PutAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            return await PutAsync(requestUri, content, null, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="content">The <see cref="HttpContent"/> sent to the server.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> to use or null if no lock token should be used.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> PutAsync(Uri requestUri, HttpContent content, LockToken lockToken, CancellationToken cancellationToken)
        {
            var requestMethod = new HttpRequestMessage(WebDavMethod.Put, requestUri);
            requestMethod.Content = content;

            if (lockToken != null)
                requestMethod.Headers.Add(WebDavRequestHeader.If, lockToken.ToString(LockTokenFormat.IfHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, HttpCompletionOption.ResponseContentRead, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Put

        #region Send

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return await SendAsync(request, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public new async Task<WebDavResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            var httpResponseMessage = await base.SendAsync(request, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Send

        #region Unlock

        /// <summary>
        /// Send a UNLOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(string requestUrl, LockToken lockToken)
        {
            return await UnlockAsync(new Uri(requestUrl), lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a UNLOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <returns>The <see cref="Task"/>t representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(Uri requestUri, LockToken lockToken)
        {
            return await UnlockAsync(requestUri, lockToken, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Send a UNLOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(string requestUrl, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await UnlockAsync(new Uri(requestUrl), lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a UNLOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(Uri requestUri, LockToken lockToken, HttpCompletionOption completionOption)
        {
            return await UnlockAsync(requestUri, lockToken, completionOption, CancellationToken.None);
        }

        /// <summary>
        /// Send a UNLOCK request to the specified URL.
        /// </summary>
        /// <param name="requestUrl">The URL the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(string requestUrl, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return await UnlockAsync(new Uri(requestUrl), lockToken, completionOption, cancellationToken);
        }

        /// <summary>
        /// Send a UNLOCK request to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">The <see cref="Uri"/> the request is sent to.</param>
        /// <param name="lockToken">The <see cref="LockToken"/> of a locked resource which should be unlocked.</param>
        /// <param name="completionOption">An <see cref="HttpCompletionOption"/> value that indicates when the operation should be considered completed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel operation.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<WebDavResponseMessage> UnlockAsync(Uri requestUri, LockToken lockToken, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (lockToken == null)
                throw new WebDavException("No lock token specified. A lock token is required for unlocking.");

            var requestMethod = new HttpRequestMessage(WebDavMethod.Unlock, requestUri);
            requestMethod.Headers.Add(WebDavRequestHeader.LockTocken, lockToken.ToString(LockTokenFormat.LockTokenHeader));

            var httpResponseMessage = await this.SendAsync(requestMethod, completionOption, cancellationToken);
            return new WebDavResponseMessage(httpResponseMessage);
        }

        #endregion Unlock

        #region Private methods

        private static Multistatus GetMultistatusRequestResult(HttpResponseMessage responseMessage)
        {
            var taskStream = responseMessage.Content.ReadAsStreamAsync();
            taskStream.Wait();
            return (Multistatus)MultistatusSerializer.Deserialize(taskStream.Result);
        }

        #endregion Private methods
    }
}

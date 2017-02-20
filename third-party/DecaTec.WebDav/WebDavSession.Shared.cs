using DecaTec.WebDav.WebDavArtifacts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DecaTec.WebDav
{
    public partial class WebDavSession : IDisposable
    {
        private readonly WebDavClient webDavClient;
        private readonly ConcurrentDictionary<Uri, PermanentLock> permanentLocks;

        #region Properties

        /// <summary>
        /// Gets or sets the base <see cref="Uri"/> of this WebDavSession.
        /// </summary>
        public Uri BaseUri
        {
            get;
            set;
        }

        #endregion Properties

        #region Public methods

        #region Copy

        /// <summary>
        /// Copies a resource from the source URL to the destination URL (without overwriting).
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(string sourceUrl, string destinationUrl)
        {
            return await CopyAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), false);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/> (without overwriting).
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/>.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(Uri sourceUri, Uri destinationUri)
        {
            return await CopyAsync(sourceUri, destinationUri, false);
        }

        /// <summary>
        /// Copies a resource from the source URL to the destination URL.
        /// </summary>
        /// <param name="sourceUrl">The source URL.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await CopyAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), overwrite);
        }

        /// <summary>
        /// Copies a resource from the source <see cref="Uri"/> to the destination <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="Uri"/>.</param>
        /// <param name="destinationUri">The destination <see cref="Uri"/>.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            sourceUri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, sourceUri, true, false);
            destinationUri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, destinationUri, true, false);
            var lockToken = GetAffectedLockToken(destinationUri);
            var response = await this.webDavClient.CopyAsync(sourceUri, destinationUri, overwrite, WebDavDepthHeaderValue.Infinity, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Copy

        #region Create directory

        /// <summary>
        /// Creates a directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory to create.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CreateDirectoryAsync(string url)
        {
            return await CreateDirectoryAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Creates a directory at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the directory to create.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> CreateDirectoryAsync(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var lockToken = GetAffectedLockToken(uri);
            var response = await this.webDavClient.MkcolAsync(uri, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Create directory

        #region Delete

        /// <summary>
        /// Deletes a directory or file at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory or file to delete.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAsync(string url)
        {
            return await DeleteAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Deletes a directory or file at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the directory or file to delete.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DeleteAsync(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var lockToken = GetAffectedLockToken(uri);
            var response = await this.webDavClient.DeleteAsync(uri, lockToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Delete

        #region Download file

        /// <summary>
        /// Downloads a file from the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="localStream">The <see cref="Stream"/> to save the file to.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> DownloadFileAsync(string url, Stream localStream)
        {
            return await DownloadFileAsync(new Uri(url, UriKind.RelativeOrAbsolute), localStream);
        }

        #endregion Download file

        #region Exists

        /// <summary>
        /// Checks if a file or directory exists at the URL specified.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> ExistsAsync(string url)
        {
            return await ExistsAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Checks if a file or directory exists at the <see cref="Uri"/> specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to check.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> ExistsAsync(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            var response = await this.webDavClient.HeadAsync(uri);
            return response.IsSuccessStatusCode;
        }

        #endregion Exists

        #region List

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the <see cref="Uri"/> specified (using 'allprop').
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the directory which content should be listed. Has to be an absolute URI (including the base URI) or a relative URI (relative to base URI).</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>This method uses a so called 'allprop'. A server should return all known properties to the server.
        /// If not all of the expected properties are return by the server, use an overload of this method specifying a <see cref="PropFind"/> explicitly.</remarks>
        public async Task<IList<WebDavSessionListItem>> ListAsync(Uri uri)
        {
            return await ListAsync(uri, PropFind.CreatePropFindAllProp());
        }

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory which content should be listed. Has to be an absolute URL (including the base URL) or a relative URL (relative to base URL).</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>This method uses a so called 'allprop'. A server should return all known properties to the server.
        /// If not all of the expected properties are return by the server, use an overload of this method specifying a <see cref="PropFind"/> explicitly.</remarks>
        public async Task<IList<WebDavSessionListItem>> ListAsync(string url)
        {
            return await ListAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Retrieves a list of files and directories of the directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the directory which content should be listed. Has to be an absolute URL (including the base URL) or a relative URL (relative to base URL).</param>
        /// <param name="propFind">The <see cref="PropFind"/> to use. Different PropFind  types can be created using the static methods of the class <see cref="PropFind"/>.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IList<WebDavSessionListItem>> ListAsync(string url, PropFind propFind)
        {
            return await ListAsync(new Uri(url, UriKind.RelativeOrAbsolute), propFind);
        }

        #endregion List

        #region Lock

        /// <summary>
        /// Locks a file or directory at the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file or directory to lock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> LockAsync(string url)
        {
            return await LockAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        ///  Locks a file or directory at the URL specified.
        /// </summary>
        /// <param name="uri">The URI of the file or directory to lock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> LockAsync(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);

            if (this.permanentLocks.ContainsKey(uri))
                return true; // Lock already set.

            var lockInfo = new LockInfo();
            lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
            lockInfo.LockType = LockType.CreateWriteLockType();
            var response = await this.webDavClient.LockAsync(uri, WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(), WebDavDepthHeaderValue.Infinity, lockInfo);

            if (!response.IsSuccessStatusCode)
                return false; // Lock already exists.

            var lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);

            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.Content);
            var lockDiscovery = prop.LockDiscovery;

            if (lockDiscovery == null)
                return false;

            var url = uri.ToString();
            var lockGranted = lockDiscovery.ActiveLock.FirstOrDefault(x => url.EndsWith(UriHelper.AddTrailingSlash(x.LockRoot.Href, false), StringComparison.OrdinalIgnoreCase));

            if (lockGranted == null)
            {
                // Try with file expected.
                lockGranted = lockDiscovery.ActiveLock.FirstOrDefault(x => url.EndsWith(UriHelper.AddTrailingSlash(x.LockRoot.Href, true), StringComparison.OrdinalIgnoreCase));
            }

            if (lockGranted == null)
                return false;

            var permanentLock = new PermanentLock(this.webDavClient, lockToken, uri, lockGranted.Timeout);

            if (!this.permanentLocks.TryAdd(uri, permanentLock))
                throw new WebDavException("Lock with lock root " + uri.ToString() + " already exists.");

            return response.IsSuccessStatusCode;
        }

        #endregion Lock

        #region Move

        /// <summary>
        /// Moves a file or directory with the specified URL to another URL (without overwrite)
        /// </summary>
        /// <param name="sourceUrl">The URL of the source.</param>
        /// <param name="destinationUrl">The URL of the destination.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(string sourceUrl, string destinationUrl)
        {
            return await MoveAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), false);
        }

        /// <summary>
        /// Moves a file or directory with the specified URI to another URI (without overwrite).
        /// </summary>
        /// <param name="sourceUri">The URI of the source.</param>
        /// <param name="destinationUri">The URL of the destination.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(Uri sourceUri, Uri destinationUri)
        {
            return await MoveAsync(sourceUri, destinationUri, false);
        }

        /// <summary>
        /// Moves a file or directory with the specified URL to another URL.
        /// </summary>
        /// <param name="sourceUrl">The URL of the source.</param>
        /// <param name="destinationUrl">The URL of the destination.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(string sourceUrl, string destinationUrl, bool overwrite)
        {
            return await MoveAsync(new Uri(sourceUrl, UriKind.RelativeOrAbsolute), new Uri(destinationUrl, UriKind.RelativeOrAbsolute), overwrite);
        }

        /// <summary>
        /// Moves a file or directory with the specified <see cref="Uri"/> to another <see cref="Uri"/>.
        /// </summary>
        /// <param name="sourceUri">The <see cref="Uri"/> of the source.</param>
        /// <param name="destinationUri">The <see cref="Uri"/> of the destination.</param>
        /// <param name="overwrite">True, if an already existing resource should be overwritten, otherwise false.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> MoveAsync(Uri sourceUri, Uri destinationUri, bool overwrite)
        {
            sourceUri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, sourceUri, true, false);
            destinationUri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, destinationUri, true, false);
            var lockTokenSource = GetAffectedLockToken(sourceUri);
            var lockTokenDestination = GetAffectedLockToken(destinationUri);
            var response = await this.webDavClient.MoveAsync(sourceUri, destinationUri, overwrite, lockTokenSource, lockTokenDestination);
            return response.IsSuccessStatusCode;
        }

        #endregion Move

        #region Upload file

        /// <summary>
        /// Uploads a file to the URL specified.
        /// </summary>
        /// <param name="url">The URL of the file to upload.</param>
        /// <param name="localStream">The <see cref="Stream"/> containing the file to upload.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> UploadFileAsync(string url, Stream localStream)
        {
            return await UploadFileAsync(new Uri(url, UriKind.RelativeOrAbsolute), localStream);
        }

        #endregion Upload file

        #region Unlock

        /// <summary>
        /// Unlocks a file or directory at the URL specified. 
        /// </summary>
        /// <param name="url">The URL of the file or directory to unlock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> UnlockAsync(string url)
        {
            return await UnlockAsync(new Uri(url, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Unlocks a file or directory at the <see cref="Uri"/> specified. 
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the file or directory to unlock.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> UnlockAsync(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);
            PermanentLock permanentLock;

            if (!this.permanentLocks.TryRemove(uri, out permanentLock))
                return false;

            var result = await permanentLock.UnlockAsync();
            var success = result.IsSuccessStatusCode;

            if (!success)
            {
                // Lock couldn't be removed on WebDav server, add it again in the permanent locks.
                if (!this.permanentLocks.TryAdd(uri, permanentLock))
                    throw new WebDavException("Failed to unlock resource " + uri.ToString()); ;
            }

            return success;
        }

        #endregion Unlock

        #endregion Public methods

        #region Private methods

        private LockToken GetAffectedLockToken(Uri uri)
        {
            uri = UriHelper.GetCombinedUriWithTrailingSlash(this.BaseUri, uri, true, false);

            foreach (var lockItem in this.permanentLocks)
            {
                var lockUrl = lockItem.Key.ToString();
                var testUrl = uri.ToString();

                if (testUrl.StartsWith(lockUrl))
                    return lockItem.Value.LockToken;
            }

            return null;
        }

        #endregion Private methods

        #region Dispose

        bool disposed = false;

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.

                // Unlock all active locks.
                if (this.permanentLocks != null)
                {
                    foreach (var pLock in this.permanentLocks)
                    {
                        pLock.Value.Dispose();
                    }
                }

                if (this.webDavClient != null)
                {
                    this.webDavClient.Dispose();
                }
            }

            // Free any unmanaged objects here.

            disposed = true;
        }

        #endregion Dispose
    }
}

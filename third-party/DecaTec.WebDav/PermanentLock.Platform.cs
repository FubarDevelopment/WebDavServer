using System.Threading.Tasks;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing a permanent lock for use in <see cref="WebDavSession"/>.
    /// </summary>
    internal partial class PermanentLock
    {
        /// <summary>
        /// Unlocks the currently locked resource.
        /// </summary>
        /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<WebDavResponseMessage> UnlockAsync()
        {
            return await this.WebDavClient.UnlockAsync(this.LockRoot, this.LockToken);
        }
    }
}

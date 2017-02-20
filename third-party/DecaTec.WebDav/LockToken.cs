using System.Text;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing a WebDAV lock token.
    /// </summary>
    public class LockToken
    {

        /// <summary>
        /// Initializes a new instance of LockToken.
        /// </summary>
        /// <param name="lockToken">A lock token string.</param>
        public LockToken(string lockToken)
        {
            if (string.IsNullOrEmpty(lockToken))
                throw new WebDavException("A lock token cannot be null or empty.");

            this.RawLockToken = lockToken;
        }

        /// <summary>
        /// Gets the raw representation of the lock token for serialization purposes.        /// 
        /// Use <see cref="ToString"/> to get the formatted representation for use in headers.
        /// </summary>
        public string RawLockToken { get; }

        /// <summary>
        /// Gets the string representation of a lock token as used in an IF header.
        /// </summary>
        /// <param name="format">The desired <see cref="LockTokenFormat"/>.</param>
        /// <returns>A lock token string with the desired format.</returns>
        public string ToString(LockTokenFormat format)
        {
            var sb = new StringBuilder();

            if (format == LockTokenFormat.IfHeader && !this.RawLockToken.StartsWith("("))
                sb.Append("(");
            else if(!this.RawLockToken.StartsWith("<") && !this.RawLockToken.StartsWith("("))
                sb.Append("<");

            sb.Append(this.RawLockToken);

            if (format == LockTokenFormat.IfHeader && !this.RawLockToken.EndsWith(")"))
                sb.Append(")");
            else if(!this.RawLockToken.EndsWith(">") && !this.RawLockToken.EndsWith(")"))
                sb.Append(">");

            return sb.ToString();
        }
    }
}

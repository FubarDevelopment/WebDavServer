using System.Text;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing a WebDAV lock token.
    /// </summary>
    public class LockToken
    {
        private string lockToken;

        /// <summary>
        /// Initializes a new instance of LockToken.
        /// </summary>
        /// <param name="lockToken">A lock token string.</param>
        public LockToken(string lockToken)
        {
            if (string.IsNullOrEmpty(lockToken))
                throw new WebDavException("A lock token cannot be null or empty.");

            this.lockToken = lockToken;
        }

        /// <summary>
        /// Gets the string representation of a lock token as used in an IF header.
        /// </summary>
        /// <param name="format">The desired <see cref="LockTokenFormat"/>.</param>
        /// <returns>A lock token string with the desired format.</returns>
        public string ToString(LockTokenFormat format)
        {
            var sb = new StringBuilder();

            if (format == LockTokenFormat.IfHeader && !this.lockToken.StartsWith("("))
                sb.Append("(");
            else if(!this.lockToken.StartsWith("<") && !this.lockToken.StartsWith("("))
                sb.Append("<");

            sb.Append(this.lockToken);

            if (format == LockTokenFormat.IfHeader && !this.lockToken.EndsWith(")"))
                sb.Append(")");
            else if(!this.lockToken.EndsWith(">") && !this.lockToken.EndsWith(")"))
                sb.Append(">");

            return sb.ToString();
        }
    }
}


namespace DecaTec.WebDav
{
    /// <summary>
    /// Class defining the values for the WebDAV depth header.
    /// </summary>
    public class WebDavDepthHeaderValue
    {
        private string depthValue;

        /// <summary>
        /// Depth header value when only the resource itself should be affected ('0').
        /// </summary>
        public static readonly WebDavDepthHeaderValue Zero = new WebDavDepthHeaderValue("0");

        /// <summary>
        /// Depth header value when the resource and its internal members should be affected ('1').
        /// </summary>
        public static readonly WebDavDepthHeaderValue One = new WebDavDepthHeaderValue("1");

        /// <summary>
        /// Depth header value when the resource and all its members should be affected ('infinity').
        /// </summary>
        /// <remarks>According to RFC4918, servers do not have to support an 'infinity' depth.</remarks>
        public static readonly WebDavDepthHeaderValue Infinity = new WebDavDepthHeaderValue("infinity");

        private WebDavDepthHeaderValue(string depthValue)
        {
            this.depthValue = depthValue;
        }

        /// <summary>
        /// Gets the string representation of the WebDavDepthHeaderValue.
        /// </summary>
        /// <returns>The string representation of the WebDavDepthHeaderValue.</returns>
        public override string ToString()
        {
            return this.depthValue;
        }
    }
}

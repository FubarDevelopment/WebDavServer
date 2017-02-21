using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'locktoken' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [XmlType(TypeName = WebDavConstants.LockToken, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class WebDavLockToken
    {
        private string hrefField;

        /// <summary>
        /// Gets or sets the Href.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Href)]
        public string Href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }
}

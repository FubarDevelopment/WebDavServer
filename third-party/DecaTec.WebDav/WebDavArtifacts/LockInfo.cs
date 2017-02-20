using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'lockinfo' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [XmlType(TypeName = WebDavConstants.LockInfo, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class LockInfo
    {
        private LockScope lockscopeField;
        private LockType locktypeField;
        private XElement ownerField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.LockScope"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockScope)]
        public LockScope LockScope
        {
            get
            {
                return this.lockscopeField;
            }
            set
            {
                this.lockscopeField = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.LockType"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockType)]
        public LockType LockType
        {
            get
            {
                return this.locktypeField;
            }
            set
            {
                this.locktypeField = value;
            }
        }

        /// <summary>
        /// Gets or sets the owner element.
        /// </summary>
        [XmlAnyElement(Name = WebDavConstants.Owner, Namespace = "DAV:")]
        public XElement Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }
    }
}

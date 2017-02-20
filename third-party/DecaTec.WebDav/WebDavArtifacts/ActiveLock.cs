using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'activelock' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.ActiveLock, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class ActiveLock
    {
        private LockScope lockscopeField;
        private LockType locktypeField;
        private string depthField;
        private XElement ownerField;
        private string timeoutField;
        private WebDavLockToken locktokenField;
        private LockRoot lockRootField;

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
        /// Gets or sets the Depth.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Depth)]
        public string Depth
        {
            get
            {
                return this.depthField;
            }
            set
            {
                this.depthField = value;
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

        /// <summary>
        /// Gets or sets the Timeout.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Timeout)]
        public string Timeout
        {
            get
            {
                return this.timeoutField;
            }
            set
            {
                this.timeoutField = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.WebDavLockToken"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockToken, IsNullable = false)]
        public WebDavLockToken LockToken
        {
            get
            {
                return this.locktokenField;
            }
            set
            {
                this.locktokenField = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.LockRoot"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockRoot, IsNullable = false)]
        public LockRoot LockRoot
        {
            get
            {
                return this.lockRootField;
            }
            set
            {
                this.lockRootField = value;
            }
        }
    }
}

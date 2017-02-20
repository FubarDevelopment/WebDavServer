using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'supportedlock' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.SupportedLock, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class SupportedLock
    {
        private LockEntry[] lockentryField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.LockEntry"/> array.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockEntry)]
        public LockEntry[] LockEntry
        {
            get
            {
                return this.lockentryField;
            }
            set
            {
                this.lockentryField = value;
            }
        }
    }
}

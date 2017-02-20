using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'lockdiscovery' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.LockDiscovery, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class LockDiscovery
    {
        private ActiveLock[] activelockField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.ActiveLock"/> array.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.ActiveLock)]
        public ActiveLock[] ActiveLock
        {
            get
            {
                return this.activelockField;
            }
            set
            {
                this.activelockField = value;
            }
        }
    }
}

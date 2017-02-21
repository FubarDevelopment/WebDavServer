using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'locktype' XML element for WebDAV communication.
    /// RFC4918 only specifies a single lock type, which is the write lock type.
    /// </summary>
    [DataContract]
    [XmlType(TypeName = WebDavConstants.LockType, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class LockType
    {
        /// <summary>
        /// Creates a new LockType (write).
        /// </summary>
        /// <returns>The (write) LockType.</returns>
        public static LockType CreateWriteLockType()
        {
            var lockType = new LockType();
            lockType.Write = new Write();
            return lockType;
        }

        private Write writeField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.Write"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Write)]
        public Write Write
        {
            get
            {
                return this.writeField;
            }
            set
            {
                this.writeField = value;
            }
        }
    }
}

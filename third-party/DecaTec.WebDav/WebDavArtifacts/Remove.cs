using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'remove' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.Remove, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class Remove
    {
        private Prop propField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.Prop"/>.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Prop)]
        public Prop Prop
        {
            get
            {
                return this.propField;
            }
            set
            {
                this.propField = value;
            }
        }
    }
}

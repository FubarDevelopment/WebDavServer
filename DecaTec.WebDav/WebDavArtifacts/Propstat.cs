using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'propstat' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.PropStat, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class Propstat
    {
        private Prop propField;
        private string statusField;
        private string responsedescriptionField;

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

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Status)]
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <summary>
        /// Gets or sets the ResponseDescription.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.ResponseDescription)]
        public string ResponseDescription
        {
            get
            {
                return this.responsedescriptionField;
            }
            set
            {
                this.responsedescriptionField = value;
            }
        }
    }
}

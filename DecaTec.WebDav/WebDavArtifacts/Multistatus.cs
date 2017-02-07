using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'multistatus' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.MultiStatus, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class Multistatus
    {
        private Response[] responseField;
        private string responsedescriptionField;

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.Response"/> array.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Response)]
        public Response[] Response
        {
            get
            {
                return this.responseField;
            }
            set
            {
                this.responseField = value;
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

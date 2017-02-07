using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'response' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.Response, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class Response
    {
        private string hrefField;
        private object[] itemsField;
        private ItemsChoiceType[] itemsElementNameField;
        private string responsedescriptionField;

        /// <summary>
        /// Gets or sets the Href.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Href, Order = 0)]
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

        /// <summary>
        /// Gets or sets the Items.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Href, Type = typeof(string), Order = 1)]
        [XmlElement(ElementName = WebDavConstants.PropStat, Type = typeof(Propstat), Order = 1)]
        [XmlElement(ElementName = WebDavConstants.Status, Type = typeof(string), Order = 1)]
        [XmlChoiceIdentifier(WebDavConstants.ItemsElementName)]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DecaTec.WebDav.WebDavArtifacts.ItemsChoiceType"/> array.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.ItemsElementName, Order = 2)]
        [XmlIgnore()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }

        /// <summary>
        /// Gets or sets the ResponseDescription.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.ResponseDescription, Order = 3)]
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

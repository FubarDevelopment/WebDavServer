using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Enum representing a choice for items in XML fragments for WebDAV communication.
    /// </summary>
    [DataContract]
    [XmlType(Namespace = WebDavConstants.DAV, IncludeInSchema = false)]
    public enum ItemsChoiceType
    {       
        /// <summary>
        /// href.
        /// </summary>
        href,    
        /// <summary>
        /// propstat.
        /// </summary>
        propstat,    
        /// <summary>
        /// status.
        /// </summary>
        status
    }
}

using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'propfind' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [XmlType(TypeName = WebDavConstants.PropFind, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class PropFind
    {
        /// <summary>
        /// Creates a PropFind instance representing an 'allprop'-Propfind.
        /// </summary>
        /// <returns>A PropFind instance containing an <see cref="DecaTec.WebDav.WebDavArtifacts.AllProp"/> element.</returns>
        public static PropFind CreatePropFindAllProp()
        {
            var propFind = new PropFind();
            propFind.Item = new AllProp();
            return propFind;
        }

        /// <summary>
        /// Creates an empty PropFind instance. The server should return all known properties (for the server) by that empty PropFind.
        /// </summary>
        /// <returns>An empty PropFind instance.</returns>
        public static PropFind CreatePropFind()
        {
            return new PropFind();
        }

        /// <summary>
        /// Creates a PropFind instance containing empty property items with the specified names. Useful for obtaining only a few properties from the server.
        /// </summary>
        /// <param name="propertyNames">The property names which should be contained in the PropFind instance.</param>
        /// <returns>A PropFind instance containing the empty <see cref="DecaTec.WebDav.WebDavArtifacts.Prop"/> items specified.</returns>
        public static PropFind CreatePropFindWithEmptyProperties(params string[] propertyNames)
        {
            var propFind = new PropFind();
            var prop = Prop.CreatePropWithEmptyProperties(propertyNames);
            propFind.Item = prop;
            return propFind;
        }

        /// <summary>
        /// Creates a PropFind instance containing empty property items for all the Props defined in RFC4918/RFC4331.
        /// </summary>
        /// <returns>A PropFind instance containing the empty <see cref="DecaTec.WebDav.WebDavArtifacts.Prop"/> items of all Props defined in RFC4918/RFC4331.</returns>
        public static PropFind CreatePropFindWithEmptyPropertiesAll()
        {
            var propFind = new PropFind();
            var prop = Prop.CreatePropWithEmptyPropertiesAll();
            propFind.Item = prop;
            return propFind;
        }

        /// <summary>
        /// Creates a PropFind instance containing a PropertyName item.
        /// </summary>
        /// <returns>A PropFind instance containing a <see cref="DecaTec.WebDav.WebDavArtifacts.PropName"/> item.</returns>
        public static PropFind CreatePropFindWithPropName()
        {
            var propFind = new PropFind();
            propFind.Item = new PropName();
            return propFind;
        }

        private object itemField;

        /// <summary>
        /// Gets or sets the Item.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.AllProp, Type = typeof(AllProp))]
        [XmlElement(ElementName = WebDavConstants.Prop, Type = typeof(Prop))]
        [XmlElement(ElementName = WebDavConstants.PropName, Type =  typeof(PropName))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }
}

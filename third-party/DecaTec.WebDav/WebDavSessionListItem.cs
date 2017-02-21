using System;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class representing a list item from a WebDavSession's list method.
    /// </summary>
    public class WebDavSessionListItem
    {
        /// <summary>
        /// Gets or sets the CreationDate.
        /// </summary>
        public DateTime CreationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ContentLanguage.
        /// </summary>
        public string ContentLanguage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the DisplayName.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ContentLength.
        /// </summary>
        public long ContentLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ContentType.
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the LastModified.
        /// </summary>
        public DateTime LastModified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        public string ETag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        public string Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ResourceType.
        /// </summary>
        public string ResourceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ContentClass.
        /// </summary>
        public string ContentClass
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the DefaultDocument.
        /// </summary>
        public string DefaultDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI (Href).
        /// </summary>
        public Uri Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsCollection.
        /// </summary>
        public bool IsCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsHidden.
        /// </summary>
        public bool IsHidden
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsReadonly.
        /// </summary>
        public bool IsReadonly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsRoot.
        /// </summary>
        public bool IsRoot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsStructuredDocument.
        /// </summary>
        public bool IsStructuredDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the LastAccessed.
        /// </summary>
        public DateTime LastAccessed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ParentName.
        /// </summary>
        public string ParentName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the QuotaAvailableBytes.
        /// </summary>
        public long QuotaAvailableBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the QuotaUsedBytes.
        /// </summary>
        public long QuotaUsedBytes
        {
            get;
            set;
        }        
    }
}

using DecaTec.WebDav.WebDavArtifacts;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class for parsing the content of WebDAV responses.
    /// </summary>
    public static class WebDavResponseContentParser
    {
        private static readonly XmlSerializer MultistatusSerializer = new XmlSerializer(typeof(Multistatus));
        private static readonly XmlSerializer PropSerializer = new XmlSerializer(typeof(Prop));

        /// <summary>
        /// Extracts a <see cref="Multistatus"/> from a <see cref="HttpContent"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> containing the <see cref="Multistatus"/> as XML.</param>
        /// <returns>The <see cref="Task"/>t representing the asynchronous operation.</returns>
        public static async Task<Multistatus> ParseMultistatusResponseContentAsync(this HttpContent content)
        {
            if (content == null)
                return null;

            try
            {
                var contentStream = await content.ReadAsStreamAsync();
                var multistatus = (Multistatus)MultistatusSerializer.Deserialize(contentStream);
                return multistatus;
            }
            catch (Exception ex)
            {
                throw new WebDavException("Failed to parse a multistatus response", ex);
            }
        }

        /// <summary>
        /// Extracts a <see cref="Prop"/> from a <see cref="HttpContent"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> containing the <see cref="Prop"/> as XML.</param>
        /// <returns>The <see cref="Task"/>t representing the asynchronous operation.</returns>
        public static async Task<Prop> ParsePropResponseContentAsync(this HttpContent content)
        {
            if (content == null)
                return null;

            try
            {
                var contentStream = await content.ReadAsStreamAsync();
                var prop = (Prop)PropSerializer.Deserialize(contentStream);
                return prop;
            }
            catch (Exception ex)
            {
                throw new WebDavException("Failed to parse a WebDAV Prop", ex);
            }
        }
    }
}

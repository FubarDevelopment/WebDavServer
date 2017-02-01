using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Tests.Support
{
    public static class EntryExtensions
    {
        public static async Task<IReadOnlyCollection<XElement>> GetPropertyElementsAsync(
            this IEntry entry,
            CancellationToken ct)
        {
            var result = new List<XElement>();
            using (var propEnum = entry.GetProperties().GetEnumerator())
            {
                while (await propEnum.MoveNext(ct).ConfigureAwait(false))
                {
                    var element = await propEnum.Current.GetXmlValueAsync(ct).ConfigureAwait(false);
                    result.Add(element);
                }
            }

            return result;
        }
    }
}

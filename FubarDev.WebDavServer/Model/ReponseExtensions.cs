using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model
{
    public static class ReponseExtensions
    {
        public static IEnumerable<string> GetHrefs(this Response response)
        {
            if (!string.IsNullOrEmpty(response.Href))
                yield return response.Href;
            if (response.ItemsElementName != null)
            {
                var index = 0;
                foreach (var choiceType in response.ItemsElementName)
                {
                    if (choiceType == ItemsChoiceType2.Href)
                        yield return (string)response.Items[index];
                    index += 1;
                }
            }
        }
    }
}

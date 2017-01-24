using System;
using System.Text;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines
{
    public struct ActionResult
    {
        public ITarget Target { get; set; }
        public Uri Href { get; set; }
        public WebDavStatusCodes StatusCode { get; set; }
        public Error Error { get; set; }
        public string Reason { get; set; }

        public bool IsFailure => ((int)StatusCode) >= 300;

        public string GetGroupableStatus()
        {
            var result = new StringBuilder()
                .Append((int)StatusCode);

            if (Error != null)
            {
                result.Append("+error");
                for (var i = 0; i != Error.ItemsElementName.Length; ++i)
                {
                    string textToAppend;
                    switch (Error.ItemsElementName[i])
                    {
                        case ItemsChoiceType.Any:
                            textToAppend = ((XElement)Error.Items[i]).ToString(SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting);
                            break;
                        default:
                            textToAppend = Error.ItemsElementName[i].ToString();
                            break;
                    }

                    result.Append(':').Append(Uri.EscapeDataString(textToAppend));
                }
            }

            return result.ToString();
        }
    }
}

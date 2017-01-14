using System.IO;
using System.Text;

namespace FubarDev.WebDavServer.Formatters
{
    public interface IWebDavOutputFormatter
    {
        string ContentType { get; }

        Encoding Encoding { get; }

        void Serialize<T>(Stream output, T data);
    }
}

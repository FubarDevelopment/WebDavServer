using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer
{
    public class WebDavFeatures
    {
        public IEnumerable<int> Versions { get; }
        public IEnumerable<string> HttpMethods { get; }
    }
}

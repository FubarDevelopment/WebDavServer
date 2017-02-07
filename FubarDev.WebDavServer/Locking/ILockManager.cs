using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Locking
{
    public interface ILockManager
    {
        Task LockAsync(IEntry entry, XElement owner, bool recursive);
    }
}

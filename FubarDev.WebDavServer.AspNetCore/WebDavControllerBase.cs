using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore.Routing;

using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavControllerBase : ControllerBase
    {
        [HttpOptions]
        public IActionResult QueryOptionsAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // GET api/values
        [HttpGet]
        public Task<IActionResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // POST api/values
        [HttpPost]
        public Task PostAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // PUT api/values/5
        [HttpPut]
        public Task PutAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // DELETE api/values/5
        [HttpDelete()]
        public Task DeleteAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpPropFind()]
        public Task<IActionResult> PropFindAsync(string path, [FromBody]Model.Propfind request, [FromHeader(Name = "Depth")] string depth, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpPropPatch]
        public Task PropPatchAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpHead]
        public Task<IActionResult> HeadAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpCopy]
        public Task<IActionResult> CopyAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpMove]
        public Task<IActionResult> MoveAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

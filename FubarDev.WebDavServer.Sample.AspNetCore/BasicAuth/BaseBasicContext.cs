using System;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.Sample.AspNetCore.BasicAuth
{
	public class BaseBasicContext : BaseControlContext
	{
		public BaseBasicContext(HttpContext context, BasicOptions options)
			: base(context)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Options = options;
		}

		public BasicOptions Options { get; }
	}
}

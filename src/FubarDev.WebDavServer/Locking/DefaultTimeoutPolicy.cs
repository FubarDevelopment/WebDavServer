// <copyright file="DefaultTimeoutPolicy.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The default timeout policy.
    /// </summary>
    public class DefaultTimeoutPolicy : ITimeoutPolicy
    {
        private readonly DefaultTimeoutPolicyOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTimeoutPolicy"/> class.
        /// </summary>
        /// <param name="options">The options to configure this <see cref="ITimeoutPolicy"/> implementation.</param>
        public DefaultTimeoutPolicy(IOptions<DefaultTimeoutPolicyOptions> options)
        {
            _options = options?.Value ?? new DefaultTimeoutPolicyOptions();
        }

        /// <inheritdoc />
        public TimeSpan SelectTimeout(IReadOnlyCollection<TimeSpan> timeouts)
        {
            var t = timeouts.ToImmutableList();
            if (_options.AllowInfiniteTimeout)
            {
                t = t.Where(x => x != Model.Headers.TimeoutHeader.Infinite).ToImmutableList();
            }

            if (_options.MaxTimeout != null)
            {
                var timeoutCandidates = t.Where(x => x <= _options.MaxTimeout).ToImmutableList();
                if (timeoutCandidates.Count != 0)
                {
                    return timeoutCandidates.Max();
                }

                return _options.MaxTimeout.Value;
            }

            if (t.Count != 0)
            {
                return t.Max();
            }

            return Model.Headers.TimeoutHeader.Infinite;
        }
    }
}

// <copyright file="TestSystemClock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using FubarDev.WebDavServer.Locking;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestSystemClock : ISystemClock
    {
        private readonly ConcurrentDictionary<DefaultLockTimeRoundingMode, ILockTimeRounding> _roundingForMode = new ConcurrentDictionary<DefaultLockTimeRoundingMode, ILockTimeRounding>();

        private TimeSpan _diff;

        public DateTime UtcNow => DateTime.UtcNow + _diff;

        public void Set(DateTime dt)
        {
            var current = DateTime.UtcNow;
            _diff = dt - current;
        }

        public void RoundTo(DefaultLockTimeRoundingMode roundingMode)
        {
            var now = DateTime.UtcNow;
            var rounded = GetRoundedDate(now, roundingMode);
            _diff = rounded - now;
        }

        private DateTime GetRoundedDate(DateTime dt, DefaultLockTimeRoundingMode roundingMode)
        {
            var rounding = _roundingForMode.GetOrAdd(roundingMode, mode => new DefaultLockTimeRounding(mode));
            return rounding.Round(dt);
        }
    }
}

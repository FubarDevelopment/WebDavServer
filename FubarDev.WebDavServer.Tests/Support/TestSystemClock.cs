// <copyright file="TestSystemClock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Locking;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestSystemClock : ISystemClock
    {
        private TimeSpan _diff;

        public DateTime UtcNow => DateTime.UtcNow + _diff;

        public void Set(DateTime dt)
        {
            var current = DateTime.UtcNow;
            _diff = dt - current;
        }

        public void RoundTo(LockCleanupTaskOptions.DefaultRoundingMode roundingMode)
        {
            var now = DateTime.UtcNow;
            var rounded = GetRoundedDate(now, roundingMode);
            _diff = rounded - now;
        }

        private static DateTime GetRoundedDate(DateTime dt, LockCleanupTaskOptions.DefaultRoundingMode roundingMode)
        {
            switch (roundingMode)
            {
                case LockCleanupTaskOptions.DefaultRoundingMode.OneSecond:
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind)
                        + TimeSpan.FromSeconds(dt.Millisecond != 0 ? 1 : 0);
                case LockCleanupTaskOptions.DefaultRoundingMode.OneHundredMilliseconds:
                    var millis = dt.Millisecond + 99;
                    millis = millis - (millis % 100);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind)
                        + TimeSpan.FromMilliseconds(millis);
            }

            return dt;
        }
    }
}
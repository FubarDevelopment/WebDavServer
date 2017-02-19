// <copyright file="DefaultLockTimeRounding.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The default rounding implementation
    /// </summary>
    public class DefaultLockTimeRounding : ILockTimeRounding
    {
        private readonly DefaultLockTimeRoundingMode _roundingMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLockTimeRounding"/> class.
        /// </summary>
        /// <param name="roundingMode">The rounding mode</param>
        public DefaultLockTimeRounding(DefaultLockTimeRoundingMode roundingMode)
        {
            _roundingMode = roundingMode;
        }

        /// <inheritdoc />
        public DateTime Round(DateTime dt)
        {
            var millis = dt.Millisecond;
            switch (_roundingMode)
            {
                case DefaultLockTimeRoundingMode.OneHundredMilliseconds:
                    millis = millis + 99;
                    millis = millis - (millis % 100);
                    break;
                case DefaultLockTimeRoundingMode.OneSecond:
                    millis = (millis != 0) ? 1000 : 0;
                    break;
                default:
                    return dt;
            }

            return new DateTime(
                    dt.Year,
                    dt.Month,
                    dt.Day,
                    dt.Hour,
                    dt.Minute,
                    dt.Second,
                    dt.Kind)
                .Add(TimeSpan.FromMilliseconds(millis));
        }

        /// <inheritdoc />
        public TimeSpan Round(TimeSpan ts)
        {
            if (ts == TimeoutHeader.Infinite)
                return ts;

            var seconds = 0;
            var millis = ts.Milliseconds;
            switch (_roundingMode)
            {
                case DefaultLockTimeRoundingMode.OneHundredMilliseconds:
                    millis = millis + 99;
                    millis = millis - (millis % 100);
                    break;
                case DefaultLockTimeRoundingMode.OneSecond:
                    seconds = (millis != 0) ? 1 : 0;
                    break;
                default:
                    return ts;
            }

            return new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds)
                .Add(TimeSpan.FromSeconds(seconds))
                .Add(TimeSpan.FromMilliseconds(millis));
        }
    }
}

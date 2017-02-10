// <copyright file="LockCleanupTaskOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The options for the <see cref="LockCleanupTask"/>
    /// </summary>
    public class LockCleanupTaskOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockCleanupTaskOptions"/> class.
        /// </summary>
        public LockCleanupTaskOptions()
        {
            RoundingFunc = new DefaultRounding(DefaultRoundingMode.OneSecond).Round;
        }

        /// <summary>
        /// The rounding modes for the default rounding mode implementation
        /// </summary>
        public enum DefaultRoundingMode
        {
            /// <summary>
            /// Round to the next second
            /// </summary>
            OneSecond,

            /// <summary>
            /// Round to the next 100 milliseconds
            /// </summary>
            OneHundredMilliseconds,
        }

        /// <summary>
        /// Gets or sets the function used for rounding the time span until the next lock needs to be removed
        /// </summary>
        public Func<TimeSpan, TimeSpan> RoundingFunc { get; set; }

        /// <summary>
        /// The default rounding implementation
        /// </summary>
        public class DefaultRounding
        {
            private readonly DefaultRoundingMode _roundingMode;

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultRounding"/> class.
            /// </summary>
            /// <param name="roundingMode">The rounding mode</param>
            public DefaultRounding(DefaultRoundingMode roundingMode)
            {
                _roundingMode = roundingMode;
            }

            /// <summary>
            /// The rounding implementation
            /// </summary>
            /// <param name="ts">The time span to round</param>
            /// <returns>The new timestamp</returns>
            public TimeSpan Round(TimeSpan ts)
            {
                var seconds = 0;
                var millis = ts.Milliseconds;
                switch (_roundingMode)
                {
                    case DefaultRoundingMode.OneHundredMilliseconds:
                        millis = millis + 99;
                        millis = millis - (millis % 100);
                        break;
                    case DefaultRoundingMode.OneSecond:
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
}

// <copyright file="NormalizedRangeItem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Model
{
    public struct NormalizedRangeItem
    {
        public NormalizedRangeItem(long from, long to)
        {
            From = from;
            To = to;
        }

        public long From { get; }

        public long To { get; }

        public long Length => To - From + 1;
    }
}

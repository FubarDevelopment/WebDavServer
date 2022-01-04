// <copyright file="DateTimeOffsetIso8601ConverterTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Live;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Converters;

public class DateTimeOffsetIso8601ConverterTests
{
    [Fact]
    public void TestRoundTrip1()
    {
        var converter = new DateTimeOffsetIso8601Converter();
        var dateTimeOffset = new DateTimeOffset(2017, 1, 1, 2, 3, 4, TimeSpan.FromHours(1));
        var element = converter.ToElement(CreationDateProperty.PropertyName, dateTimeOffset);
        var value = converter.FromElement(element);
        Assert.Equal(dateTimeOffset, value);
    }

    [Fact]
    public void TestRoundTrip2()
    {
        var converter = new DateTimeOffsetIso8601Converter();
        var dateTimeOffset = new DateTimeOffset(2017, 1, 1, 2, 3, 4, TimeSpan.FromHours(1));
        var value = dateTimeOffset;
        for (var i = 0; i != 2; ++i)
        {
            var element = converter.ToElement(CreationDateProperty.PropertyName, dateTimeOffset);
            value = converter.FromElement(element);
        }

        Assert.Equal(dateTimeOffset, value);
    }
}

// <copyright file="Iso8601ConverterTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Props.Converters;

using Xunit;

namespace FubarDev.WebDavServer.Tests.PropertyStore
{
    public class Iso8601ConverterTests
    {
        [Theory]
        [InlineData("2018-03-23T10:00:00Z")]
        [InlineData("2018-03-23T09:00:00-01:00")]
        [InlineData("2018-03-23T11:00:00+01:00")]
        [InlineData("2018-03-23T11:00:00.123+01:00")]
        [InlineData("2018-03-23T11:00:00.1234567+01:00")]
        [InlineData("2018-03-23t10:00:00Z")]
        [InlineData("2018-03-23t09:00:00-01:00")]
        [InlineData("2018-03-23t11:00:00+01:00")]
        [InlineData("2018-03-23t11:00:00.123+01:00")]
        [InlineData("2018-03-23t11:00:00.1234567+01:00")]
        [InlineData("2018-03-23 10:00:00Z")]
        [InlineData("2018-03-23 09:00:00-01:00")]
        [InlineData("2018-03-23 11:00:00+01:00")]
        [InlineData("2018-03-23 11:00:00.123+01:00")]
        [InlineData("2018-03-23 11:00:00.1234567+01:00")]
        [InlineData("2018-03-23T10:00:00z")]
        [InlineData("2018-03-23t10:00:00z")]
        [InlineData("2018-03-23 10:00:00z")]
        public void ParserSuccessTests(string input)
        {
            DateTimeOffsetIso8601Converter.Parse(input);
        }

        [Theory]
        [InlineData("2018-03-23x10:00:00Z", "2018-03-23x10:00:00Z is not a valid ISO 8601 format (no recognized date/time separator)")]
        [InlineData("2018-03-23T10:00:00", "2018-03-23T10:00:00 is not a valid ISO 8601 format (no time zone given)")]
        public void ParserFailureTests(string input, string expectedErrorMessage)
        {
            var ex = Assert.Throws<FormatException>(() => DateTimeOffsetIso8601Converter.Parse(input));
            Assert.Equal(expectedErrorMessage, ex.Message);
        }
    }
}

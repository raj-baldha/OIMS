using System;
using FluentAssertions;
using OIMS.Application.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("  HELLO  ", "hello")]
        [InlineData("World", "world")]
        [InlineData("   test string   ", "test string")]
        [InlineData("Already cleaned", "already cleaned")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        public void Clean_ShouldTrimWhitespaceAndConvertToLowercase(string input, string expected)
        {
            string result = input.Clean();

            result.Should().Be(expected);
        }

        [Fact]
        public void Clean_ShouldThrowNullReferenceException_WhenInputIsNull()
        {
            string nullString = null!;

            Action act = () => nullString.Clean();

            act.Should().Throw<NullReferenceException>();
        }
    }
}

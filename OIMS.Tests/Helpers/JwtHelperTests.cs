using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OIMS.Infrastructure.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class JwtHelperTests
    {
        private const string TestSecurityKey =
            "super_secret_test_key_with_at_least_32_characters_long!";
        private const string TestIssuer = "OIMS-Test-Issuer";
        private const string TestAudience = "OIMS-Test-Audience";
        private const string TestExpiryMinutes = "30";

        private IConfiguration CreateConfiguration(
            string key = TestSecurityKey,
            string issuer = TestIssuer,
            string audience = TestAudience,
            string expiryMinutes = TestExpiryMinutes
        )
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:Key", key },
                { "JwtSettings:Issuer", issuer },
                { "JwtSettings:Audience", audience },
                { "JwtSettings:ExpiryMinutes", expiryMinutes },
            };

            return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken_WhenSettingsAreProvided()
        {
            var configuration = CreateConfiguration();
            var jwtHelper = new JwtHelper(configuration);

            int userId = 10;
            string email = "test@example.com";
            string role = "Administrator";

            string tokenString = jwtHelper.GenerateToken(userId, email, role);

            tokenString.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(tokenString).Should().BeTrue();

            var jwtToken = handler.ReadJwtToken(tokenString);

            jwtToken.Issuer.Should().Be(TestIssuer);
            jwtToken.Audiences.Should().Contain(TestAudience);
            jwtToken
                .Claims.Should()
                .Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "10");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);

            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jwtToken
                .ValidTo.Should()
                .BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(2));
        }

        [Fact]
        public void GenerateToken_ShouldThrowFormatException_WhenExpiryMinutesIsNotInteger()
        {
            var configuration = CreateConfiguration(expiryMinutes: "invalid-number");
            var jwtHelper = new JwtHelper(configuration);

            Action act = () => jwtHelper.GenerateToken(1, "test@example.com", "Employee");

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void GenerateToken_ShouldThrowArgumentNullException_WhenConfigurationValueIsMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:Issuer", TestIssuer },
                { "JwtSettings:Audience", TestAudience },
                { "JwtSettings:ExpiryMinutes", TestExpiryMinutes },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var jwtHelper = new JwtHelper(configuration);

            Action act = () => jwtHelper.GenerateToken(1, "test@example.com", "Employee");

            act.Should().Throw<ArgumentNullException>();
        }
    }
}

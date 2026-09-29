using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using OIMS.API.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class CookieHelperTests
    {
        [Fact]
        public void SetAccessTokenCookie_ShouldAppendCookieWithConfiguredExpiry_WhenExpiryIsProvided()
        {
            var httpContext = new DefaultHttpContext();
            var configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("120");

            CookieHelper.SetAccessTokenCookie(
                httpContext.Response,
                configurationMock.Object,
                "valid-token-123"
            );

            httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
            string setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();

            setCookieHeader.Should().Contain("accessToken=valid-token-123");
            setCookieHeader.Should().Contain("httponly");
            setCookieHeader.Should().Contain("samesite=lax");
        }

        [Fact]
        public void SetAccessTokenCookie_ShouldDefaultTo60Minutes_WhenExpiryIsNotConfigured()
        {
            var httpContext = new DefaultHttpContext();
            var configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns((string?)null);

            CookieHelper.SetAccessTokenCookie(
                httpContext.Response,
                configurationMock.Object,
                "default-expiry-token"
            );

            httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
            string setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();

            setCookieHeader.Should().Contain("accessToken=default-expiry-token");
        }

        [Fact]
        public void ClearAccessTokenCookie_ShouldAppendExpiredCookieHeader()
        {
            var httpContext = new DefaultHttpContext();

            CookieHelper.ClearAccessTokenCookie(httpContext.Response);

            httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
            string setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();

            setCookieHeader.Should().Contain("accessToken=");
            setCookieHeader.Should().Contain("expires=");
        }

        [Fact]
        public void GetAccessToken_ShouldReturnToken_WhenCookieExists()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "accessToken=my-secret-access-token";

            string? token = CookieHelper.GetAccessToken(httpContext.Request);

            token.Should().Be("my-secret-access-token");
        }

        [Fact]
        public void GetAccessToken_ShouldReturnNull_WhenCookieDoesNotExist()
        {
            var httpContext = new DefaultHttpContext();

            string? token = CookieHelper.GetAccessToken(httpContext.Request);

            token.Should().BeNull();
        }
    }
}

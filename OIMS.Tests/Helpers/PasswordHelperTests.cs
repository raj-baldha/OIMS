using FluentAssertions;
using OIMS.Infrastructure.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class PasswordHelperTests
    {
        private readonly PasswordHelper _passwordHelper;

        public PasswordHelperTests()
        {
            _passwordHelper = new PasswordHelper();
        }

        [Fact]
        public void HashPassword_ShouldReturnValidBcryptHash_WhenPasswordIsProvided()
        {
            string plainTextPassword = "SecretPassword123!";

            string hash = _passwordHelper.HashPassword(plainTextPassword);

            hash.Should().NotBeNullOrWhiteSpace();
            hash.Should().NotBe(plainTextPassword);
            hash.Should().StartWith("$2");
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            string plainTextPassword = "CorrectPassword123!";
            string hash = _passwordHelper.HashPassword(plainTextPassword);

            bool isValid = _passwordHelper.VerifyPassword(plainTextPassword, hash);

            isValid.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";
            string hash = _passwordHelper.HashPassword(correctPassword);

            bool isValid = _passwordHelper.VerifyPassword(wrongPassword, hash);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void HashPassword_ShouldGenerateDifferentHashes_ForSamePasswordDueToSalt()
        {
            string plainTextPassword = "SamePassword123!";

            string firstHash = _passwordHelper.HashPassword(plainTextPassword);
            string secondHash = _passwordHelper.HashPassword(plainTextPassword);

            firstHash.Should().NotBe(secondHash);
            _passwordHelper.VerifyPassword(plainTextPassword, firstHash).Should().BeTrue();
            _passwordHelper.VerifyPassword(plainTextPassword, secondHash).Should().BeTrue();
        }
    }
}

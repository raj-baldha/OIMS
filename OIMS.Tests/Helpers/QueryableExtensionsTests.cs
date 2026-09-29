using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OIMS.Application.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class QueryableExtensionsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Paginate_ShouldThrowArgumentException_WhenPageIsLessThanOne(int invalidPage)
        {
            var data = new List<int> { 1, 2, 3 }.AsQueryable();

            Action act = () => data.Paginate(page: invalidPage, pageSize: 10);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Page must be greater than or equal to 1.*")
                .WithParameterName("page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Paginate_ShouldThrowArgumentException_WhenPageSizeIsLessThanOne(
            int invalidPageSize
        )
        {
            var data = new List<int> { 1, 2, 3 }.AsQueryable();

            Action act = () => data.Paginate(page: 1, pageSize: invalidPageSize);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Page size must be greater than 0.*")
                .WithParameterName("pageSize");
        }

        [Fact]
        public void Paginate_ShouldUseDefaultParameters_WhenNoneProvided()
        {
            var data = Enumerable.Range(1, 25).AsQueryable();

            var result = data.Paginate().ToList();

            result.Should().HaveCount(10);
            result.Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Fact]
        public void Paginate_ShouldSkipAndTakeCorrectElements_WhenValidPageAndPageSizeProvided()
        {
            var data = Enumerable.Range(1, 30).AsQueryable();

            var page2 = data.Paginate(page: 2, pageSize: 5).ToList();
            var page3 = data.Paginate(page: 3, pageSize: 5).ToList();

            page2.Should().Equal(6, 7, 8, 9, 10);
            page3.Should().Equal(11, 12, 13, 14, 15);
        }

        [Fact]
        public void Paginate_ShouldReturnEmpty_WhenPageExceedsDataCount()
        {
            var data = new List<int> { 1, 2, 3 }.AsQueryable();

            var result = data.Paginate(page: 5, pageSize: 10).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Paginate_ShouldReturnRemainingElements_WhenPageIsPartiallyFull()
        {
            var data = Enumerable.Range(1, 12).AsQueryable();

            var result = data.Paginate(page: 2, pageSize: 10).ToList();

            result.Should().HaveCount(2);
            result.Should().Equal(11, 12);
        }
    }
}

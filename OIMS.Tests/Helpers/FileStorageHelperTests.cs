using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OIMS.Application.Helpers;
using Xunit;

namespace OIMS.Tests.Helpers
{
    public class FileStorageHelperTests
    {
        [Fact]
        public async Task SaveInvoiceAsync_ShouldCreateDirectoryAndWritePdfFile()
        {
            int orderId = 101;
            byte[] pdfContent = Encoding.UTF8.GetBytes("fake-pdf-content");

            string expectedRelativeUrl = $"/uploads/invoices/invoice-{orderId}.pdf";
            string expectedFilePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "invoices",
                $"invoice-{orderId}.pdf"
            );

            string returnedUrl = await FileStorageHelper.SaveInvoiceAsync(orderId, pdfContent);

            returnedUrl.Should().Be(expectedRelativeUrl);
            File.Exists(expectedFilePath).Should().BeTrue();

            byte[] savedBytes = await File.ReadAllBytesAsync(expectedFilePath);
            savedBytes.Should().BeEquivalentTo(pdfContent);

            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public async Task DeleteInvoiceAsync_ShouldDeleteExistingFile_WhenValidUrlProvided()
        {
            int orderId = 202;
            byte[] pdfContent = Encoding.UTF8.GetBytes("invoice-to-delete");

            string invoiceUrl = await FileStorageHelper.SaveInvoiceAsync(orderId, pdfContent);

            string expectedFilePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "invoices",
                $"invoice-{orderId}.pdf"
            );

            File.Exists(expectedFilePath).Should().BeTrue();

            await FileStorageHelper.DeleteInvoiceAsync(invoiceUrl);

            File.Exists(expectedFilePath).Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteInvoiceAsync_ShouldReturnCompletedTask_WhenUrlIsNullOrEmpty(
            string? invalidUrl
        )
        {
            var task = FileStorageHelper.DeleteInvoiceAsync(invalidUrl!);

            await task;

            task.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteInvoiceAsync_ShouldNotThrow_WhenFileDoesNotExistOnDisk()
        {
            string nonExistentUrl = "/uploads/invoices/invoice-999999.pdf";

            var action = async () => await FileStorageHelper.DeleteInvoiceAsync(nonExistentUrl);

            await action.Should().NotThrowAsync();
        }
    }
}

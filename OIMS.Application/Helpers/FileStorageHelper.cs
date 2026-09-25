namespace OIMS.Application.Helpers
{
    public static class FileStorageHelper
    {
        private const string InvoiceFolder = "uploads/invoices";

        public static async Task<string> SaveInvoiceAsync(
            int orderId,
            byte[] pdf)
        {
            string invoiceFolderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "invoices");

            if (!Directory.Exists(invoiceFolderPath))
            {
                Directory.CreateDirectory(invoiceFolderPath);
            }

            string storedFileName = $"invoice-{orderId}.pdf";

            string filePath = Path.Combine(
                invoiceFolderPath,
                storedFileName);

            await File.WriteAllBytesAsync(filePath, pdf);

            return $"/{InvoiceFolder}/{storedFileName}";
        }

        public static Task DeleteInvoiceAsync(string invoiceUrl)
        {
            if (string.IsNullOrWhiteSpace(invoiceUrl))
            {
                return Task.CompletedTask;
            }

            string relativePath = invoiceUrl
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            string filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
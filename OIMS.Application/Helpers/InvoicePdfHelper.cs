using OIMS.Application.DTOs.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace OIMS.Application.Helpers
{
    public class InvoicePdfHelper
    {
        public static byte[] GenerateInvoicePdf(InvoicePdfDto invoice)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header()
                        .Text("INVOICE")
                        .FontSize(24)
                        .Bold();

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text($"Order ID: #{invoice.OrderId}");
                            column.Item().Text(
                                $"Order Date: {invoice.OrderDate:dd-MM-yyyy}");

                            column.Item().Text(
                                $"Customer: {invoice.CustomerName}");

                            column.Item().Text(
                                $"Email: {invoice.CustomerEmail}");

                            column.Item().Text(
                                $"Shipping Address: {invoice.ShippingAddress}");

                            column.Item().LineHorizontal(1);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("#").Bold();
                                    header.Cell().Text("Product").Bold();
                                    header.Cell().Text("Qty").Bold();
                                    header.Cell().Text("Unit Price").Bold();
                                    header.Cell().Text("Total").Bold();
                                });

                                int index = 1;

                                foreach (var item in invoice.Items)
                                {
                                    table.Cell().Text(index.ToString());
                                    table.Cell().Text(item.ProductName);
                                    table.Cell().Text(item.Quantity.ToString());
                                    table.Cell().Text(item.UnitPrice.ToString("0.00"));
                                    table.Cell().Text(item.LineTotal.ToString("0.00"));

                                    index++;
                                }
                            });

                            column.Item().LineHorizontal(1);

                            column.Item()
                                .AlignRight()
                                .Text($"Subtotal: ₹{invoice.Subtotal:0.00}");

                            column.Item()
                                .AlignRight()
                                .Text(
                                    $"Discount: ₹{invoice.DiscountAmount:0.00}");

                            column.Item()
                                .AlignRight()
                                .Text(
                                    $"Total: ₹{invoice.TotalAmount:0.00}")
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .Text($"Payment Method: {invoice.PaymentMethod}");

                            column.Item()
                                .Text($"Payment Status: {invoice.PaymentStatus}");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for your order!");
                });
            });

            return document.GeneratePdf();
        }
    }
}

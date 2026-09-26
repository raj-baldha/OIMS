using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOriginalFileNameFromOrderDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "original_file_name", table: "order_documents");

            migrationBuilder.AlterColumn<string>(
                name: "stored_file_name",
                table: "order_documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<long>(
                name: "file_size",
                table: "order_documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "order_documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "stored_file_name",
                table: "order_documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255
            );

            migrationBuilder.AlterColumn<long>(
                name: "file_size",
                table: "order_documents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint"
            );

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "order_documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "order_documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true
            );
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyEntitiesForProductAndInventoryMgmtModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "status", table: "products");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "products",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AlterColumn<int>(
                name: "change_type",
                table: "inventory_transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AddColumn<int>(
                name: "quantity_after",
                table: "inventory_transactions",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "quantity_before",
                table: "inventory_transactions",
                type: "int",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_active", table: "products");

            migrationBuilder.DropColumn(name: "quantity_after", table: "inventory_transactions");

            migrationBuilder.DropColumn(name: "quantity_before", table: "inventory_transactions");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AlterColumn<string>(
                name: "change_type",
                table: "inventory_transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int"
            );
        }
    }
}

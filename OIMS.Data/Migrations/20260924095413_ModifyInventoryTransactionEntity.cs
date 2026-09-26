using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyInventoryTransactionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "reference_id", table: "inventory_transactions");

            migrationBuilder.DropColumn(name: "reference_type", table: "inventory_transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reference_id",
                table: "inventory_transactions",
                type: "int",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "reference_type",
                table: "inventory_transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true
            );
        }
    }
}

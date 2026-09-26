using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCustomerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_customers_users_user_id", table: "customers");

            migrationBuilder.DropIndex(name: "IX_customers_user_id", table: "customers");

            migrationBuilder.DropColumn(name: "user_id", table: "customers");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "users",
                type: "int",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "IX_users_CustomerId",
                table: "users",
                column: "CustomerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_id",
                table: "customers",
                column: "id",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_users_customers_CustomerId",
                table: "users",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_users_customers_CustomerId", table: "users");

            migrationBuilder.DropIndex(name: "IX_users_CustomerId", table: "users");

            migrationBuilder.DropIndex(name: "IX_customers_id", table: "customers");

            migrationBuilder.DropColumn(name: "CustomerId", table: "users");

            migrationBuilder.DropColumn(name: "email", table: "customers");

            migrationBuilder.DropColumn(name: "password_hash", table: "customers");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "customers",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_user_id",
                table: "customers",
                column: "user_id",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_customers_users_user_id",
                table: "customers",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}

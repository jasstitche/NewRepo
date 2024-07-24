using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class addedDeleltedAndActiveToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7c0d411e-e1a5-428f-9e15-7fa9f5540d21");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d0ffe033-b52c-4d85-8900-0f35fbcc1544");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5009c2d8-9ba5-4e29-bb27-68d743259fde", null, "User", "USER" },
                    { "94d38b02-92ce-49cb-b272-b6f0aa07cb38", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5009c2d8-9ba5-4e29-bb27-68d743259fde");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "94d38b02-92ce-49cb-b272-b6f0aa07cb38");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Carts");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7c0d411e-e1a5-428f-9e15-7fa9f5540d21", null, "User", "USER" },
                    { "d0ffe033-b52c-4d85-8900-0f35fbcc1544", null, "Admin", "ADMIN" }
                });
        }
    }
}

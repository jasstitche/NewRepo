using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class addedIsAdminToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SamplePages_SampleId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "SampleId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SamplePages_SampleId",
                table: "Orders",
                column: "SampleId",
                principalTable: "SamplePages",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SamplePages_SampleId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "SampleId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SamplePages_SampleId",
                table: "Orders",
                column: "SampleId",
                principalTable: "SamplePages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

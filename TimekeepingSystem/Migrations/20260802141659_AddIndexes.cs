using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimekeepingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Date",
                table: "Attendances",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_Date",
                table: "Attendances");
        }
    }
}

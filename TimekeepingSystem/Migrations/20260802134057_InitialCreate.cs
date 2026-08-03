using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimekeepingSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SalaryPerShift = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CheckOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "Description", "EndTime", "Name", "StartTime" },
                values: new object[,]
                {
                    { 1, "06:00 - 12:00", new TimeSpan(0, 12, 0, 0, 0), "Ca sáng", new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, "12:00 - 18:00", new TimeSpan(0, 18, 0, 0, 0), "Ca chiều", new TimeSpan(0, 12, 0, 0, 0) },
                    { 3, "18:00 - 22:00", new TimeSpan(0, 22, 0, 0, 0), "Ca tối", new TimeSpan(0, 18, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "BaseSalary", "CreatedAt", "Email", "FullName", "IsActive", "Password", "Phone", "Role", "SalaryPerShift", "Username" },
                values: new object[,]
                {
                    { 1, "VP Công ty", 0m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@company.com", "Admin", true, "admin123", "0123456789", "Admin", 0m, "admin" },
                    { 2, "Hà Nội", 5700000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "nguyenvana@company.com", "Nguyễn Văn A", true, "123456", "0909123456", "Worker", 250000m, "worker1" },
                    { 3, "Hồ Chí Minh", 5700000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "tranthib@company.com", "Trần Thị B", true, "123456", "0909987654", "Worker", 250000m, "worker2" },
                    { 4, "Đà Nẵng", 5700000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "levanc@company.com", "Lê Văn C", true, "123456", "0912345678", "Worker", 200000m, "worker3" },
                    { 5, "Hải Phòng", 6000000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "phamthid@company.com", "Phạm Thị D", true, "123456", "0934567890", "Worker", 250000m, "worker4" },
                    { 6, "Cần Thơ", 5800000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "hoangvane@company.com", "Hoàng Văn E", true, "123456", "0967890123", "Worker", 220000m, "worker5" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ShiftId",
                table: "Attendances",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_UserId_Date",
                table: "Attendances",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

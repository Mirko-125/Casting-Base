using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace castingbase.Migrations
{
    /// <inheritdoc />
    public partial class VI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<char>(type: "character(1)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    EMail = table.Column<string>(type: "text", nullable: false),
                    PassHash = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    RegistrationToken = table.Column<string>(type: "text", nullable: true),
                    StepCompleted = table.Column<int>(type: "integer", nullable: false),
                    ProfilePhoto = table.Column<string>(type: "text", nullable: true),
                    UserType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "BaseUser"),
                    Height = table.Column<double>(type: "double precision", nullable: true),
                    Weight = table.Column<double>(type: "double precision", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "Users",
                column: "EMail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiosk.Migrations
{
    public partial class LanguageSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminLanguage",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KitchenLanguage",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderDisplayLanguage",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminLanguage",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "KitchenLanguage",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "OrderDisplayLanguage",
                table: "SiteSettings");
        }
    }
}

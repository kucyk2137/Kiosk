using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiosk.Migrations
{
    public partial class AddEnglishTranslations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "MenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "MenuItemIngredients",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "MenuItemIngredients");
        }
    }
}

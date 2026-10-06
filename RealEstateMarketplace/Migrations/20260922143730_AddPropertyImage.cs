using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateMarketplace.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Properties");
        }
    }
}

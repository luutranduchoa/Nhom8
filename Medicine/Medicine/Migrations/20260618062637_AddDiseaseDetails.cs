using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicine.Migrations
{
    /// <inheritdoc />
    public partial class AddDiseaseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prevention",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedMedications",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symptoms",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TreatmentGuidelines",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "Prevention",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "RecommendedMedications",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "TreatmentGuidelines",
                table: "Diseases");
        }
    }
}

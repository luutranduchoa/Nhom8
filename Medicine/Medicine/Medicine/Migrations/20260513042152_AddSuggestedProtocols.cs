using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicine.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestedProtocols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuggestedProtocols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiseaseId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestedProtocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestedProtocols_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SuggestedProtocolDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    DosageInstruction = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestedProtocolDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestedProtocolDetails_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuggestedProtocolDetails_SuggestedProtocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "SuggestedProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestedProtocolDetails_DrugId",
                table: "SuggestedProtocolDetails",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestedProtocolDetails_ProtocolId",
                table: "SuggestedProtocolDetails",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestedProtocols_DiseaseId",
                table: "SuggestedProtocols",
                column: "DiseaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuggestedProtocolDetails");

            migrationBuilder.DropTable(
                name: "SuggestedProtocols");
        }
    }
}

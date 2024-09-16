using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DelhiHighCourt.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caseDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filename = table.Column<string>(type: "text", nullable: true),
                    Court = table.Column<string>(type: "text", nullable: true),
                    Abbr = table.Column<string>(type: "text", nullable: true),
                    CaseNo = table.Column<string>(type: "text", nullable: true),
                    Dated = table.Column<string>(type: "text", nullable: true),
                    CaseName = table.Column<string>(type: "text", nullable: true),
                    Counsel = table.Column<string>(type: "text", nullable: true),
                    Overrule = table.Column<string>(type: "text", nullable: true),
                    OveruleBy = table.Column<string>(type: "text", nullable: true),
                    Citation = table.Column<string>(type: "text", nullable: true),
                    Coram = table.Column<string[]>(type: "text[]", nullable: true),
                    Act = table.Column<string>(type: "text", nullable: true),
                    Bench = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Headnotes = table.Column<string>(type: "text", nullable: true),
                    CaseReferred = table.Column<string>(type: "text", nullable: true),
                    Ssd = table.Column<string>(type: "text", nullable: true),
                    Reportable = table.Column<bool>(type: "boolean", nullable: true),
                    PdfLink = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    CoramCount = table.Column<int>(type: "integer", nullable: true),
                    Petitioner = table.Column<string>(type: "text", nullable: true),
                    Respondent = table.Column<string>(type: "text", nullable: true),
                    BlaCitation = table.Column<string>(type: "text", nullable: true),
                    QrLink = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caseDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caseDetails");
        }
    }
}

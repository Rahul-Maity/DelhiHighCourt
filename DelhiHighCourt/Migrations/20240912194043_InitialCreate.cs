using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DelhiHighCourt.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Filename = table.Column<string>(type: "text", nullable: false),
                    Court = table.Column<string>(type: "text", nullable: false),
                    Abbr = table.Column<string>(type: "text", nullable: false),
                    CaseNo = table.Column<string>(type: "text", nullable: false),
                    Dated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CaseName = table.Column<string>(type: "text", nullable: false),
                    Counsel = table.Column<string>(type: "text", nullable: false),
                    Overrule = table.Column<string>(type: "text", nullable: false),
                    OveruleBy = table.Column<string>(type: "text", nullable: false),
                    Citation = table.Column<string>(type: "text", nullable: false),
                    Coram = table.Column<string>(type: "text", nullable: false),
                    Act = table.Column<string>(type: "text", nullable: false),
                    Bench = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    Headnotes = table.Column<string>(type: "text", nullable: false),
                    CaseReferred = table.Column<string>(type: "text", nullable: false),
                    Ssd = table.Column<string>(type: "text", nullable: false),
                    Reportable = table.Column<bool>(type: "boolean", nullable: false),
                    PdfLink = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CoramCount = table.Column<int>(type: "integer", nullable: false),
                    Petitioner = table.Column<string>(type: "text", nullable: false),
                    Respondent = table.Column<string>(type: "text", nullable: false),
                    BlaCitation = table.Column<string>(type: "text", nullable: false),
                    QrLink = table.Column<string>(type: "text", nullable: false)
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

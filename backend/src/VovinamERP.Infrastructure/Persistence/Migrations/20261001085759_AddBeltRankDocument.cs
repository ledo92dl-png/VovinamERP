using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeltRankDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "belt_rank_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltRankRecognitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SignedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScanUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_belt_rank_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_rank_documents_belt_rank_recognitions_BeltRankRecognit~",
                        column: x => x.BeltRankRecognitionId,
                        principalTable: "belt_rank_recognitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_documents_BeltRankRecognitionId",
                table: "belt_rank_documents",
                column: "BeltRankRecognitionId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_documents_TenantId_BeltRankRecognitionId",
                table: "belt_rank_documents",
                columns: new[] { "TenantId", "BeltRankRecognitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_documents_TenantId_DocumentNumber",
                table: "belt_rank_documents",
                columns: new[] { "TenantId", "DocumentNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "belt_rank_documents");
        }
    }
}

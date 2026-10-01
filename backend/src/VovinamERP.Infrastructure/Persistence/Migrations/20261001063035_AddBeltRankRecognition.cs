using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeltRankRecognition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "belt_rank_recognitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltRankId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecognitionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeltExamStudentResultId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_belt_rank_recognitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_rank_recognitions_belt_exam_student_results_BeltExamSt~",
                        column: x => x.BeltExamStudentResultId,
                        principalTable: "belt_exam_student_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_belt_rank_recognitions_belt_ranks_BeltRankId",
                        column: x => x.BeltRankId,
                        principalTable: "belt_ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_belt_rank_recognitions_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_BeltExamStudentResultId",
                table: "belt_rank_recognitions",
                column: "BeltExamStudentResultId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_BeltRankId",
                table: "belt_rank_recognitions",
                column: "BeltRankId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_StudentId",
                table: "belt_rank_recognitions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_TenantId_BeltExamStudentResultId",
                table: "belt_rank_recognitions",
                columns: new[] { "TenantId", "BeltExamStudentResultId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_TenantId_StudentId",
                table: "belt_rank_recognitions",
                columns: new[] { "TenantId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_belt_rank_recognitions_TenantId_StudentId_RecognitionDate",
                table: "belt_rank_recognitions",
                columns: new[] { "TenantId", "StudentId", "RecognitionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "belt_rank_recognitions");
        }
    }
}

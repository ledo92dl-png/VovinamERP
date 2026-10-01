using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeltExamManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "belt_exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetBeltRankId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SessionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SourceBeltName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_belt_exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_exams_belt_ranks_TargetBeltRankId",
                        column: x => x.TargetBeltRankId,
                        principalTable: "belt_ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "belt_exam_student_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Ranking = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_belt_exam_student_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_exam_student_results_belt_exams_BeltExamId",
                        column: x => x.BeltExamId,
                        principalTable: "belt_exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_belt_exam_student_results_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "belt_exam_subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MaximumScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_belt_exam_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_exam_subjects_belt_exams_BeltExamId",
                        column: x => x.BeltExamId,
                        principalTable: "belt_exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "belt_exam_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltExamStudentResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeltExamSubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_belt_exam_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_belt_exam_scores_belt_exam_student_results_BeltExamStudentR~",
                        column: x => x.BeltExamStudentResultId,
                        principalTable: "belt_exam_student_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_belt_exam_scores_belt_exam_subjects_BeltExamSubjectId",
                        column: x => x.BeltExamSubjectId,
                        principalTable: "belt_exam_subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_scores_BeltExamStudentResultId",
                table: "belt_exam_scores",
                column: "BeltExamStudentResultId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_scores_BeltExamSubjectId",
                table: "belt_exam_scores",
                column: "BeltExamSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_scores_TenantId_BeltExamStudentResultId_BeltExamS~",
                table: "belt_exam_scores",
                columns: new[] { "TenantId", "BeltExamStudentResultId", "BeltExamSubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_student_results_BeltExamId",
                table: "belt_exam_student_results",
                column: "BeltExamId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_student_results_StudentId",
                table: "belt_exam_student_results",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_student_results_TenantId_BeltExamId_StudentId",
                table: "belt_exam_student_results",
                columns: new[] { "TenantId", "BeltExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_student_results_TenantId_StudentId",
                table: "belt_exam_student_results",
                columns: new[] { "TenantId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_subjects_BeltExamId",
                table: "belt_exam_subjects",
                column: "BeltExamId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_subjects_TenantId_BeltExamId_DisplayOrder",
                table: "belt_exam_subjects",
                columns: new[] { "TenantId", "BeltExamId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_exam_subjects_TenantId_BeltExamId_Name",
                table: "belt_exam_subjects",
                columns: new[] { "TenantId", "BeltExamId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_belt_exams_TargetBeltRankId",
                table: "belt_exams",
                column: "TargetBeltRankId");

            migrationBuilder.CreateIndex(
                name: "IX_belt_exams_TenantId_ExamDate",
                table: "belt_exams",
                columns: new[] { "TenantId", "ExamDate" });

            migrationBuilder.CreateIndex(
                name: "IX_belt_exams_TenantId_TargetBeltRankId_ExamDate",
                table: "belt_exams",
                columns: new[] { "TenantId", "TargetBeltRankId", "ExamDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "belt_exam_scores");

            migrationBuilder.DropTable(
                name: "belt_exam_student_results");

            migrationBuilder.DropTable(
                name: "belt_exam_subjects");

            migrationBuilder.DropTable(
                name: "belt_exams");
        }
    }
}

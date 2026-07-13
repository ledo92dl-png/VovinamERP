using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceDetails",
                table: "AttendanceDetails");

            migrationBuilder.RenameTable(
                name: "AttendanceRecords",
                newName: "attendance_records");

            migrationBuilder.RenameTable(
                name: "AttendanceDetails",
                newName: "attendance_details");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "attendance_details",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "attendance_details",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackfilled",
                table: "attendance_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MarkedAt",
                table: "attendance_details",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "MarkedByUserId",
                table: "attendance_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "attendance_details",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "attendance_details",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendance_records",
                table: "attendance_records",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_attendance_details",
                table: "attendance_details",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "student_guardians",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    Relationship = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_student_guardians", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_TenantId_TrainingSessionId",
                table: "attendance_records",
                columns: new[] { "TenantId", "TrainingSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_details_AttendanceRecordId_StudentId",
                table: "attendance_details",
                columns: new[] { "AttendanceRecordId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_details_MarkedAt",
                table: "attendance_details",
                column: "MarkedAt");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_details_StudentId",
                table: "attendance_details",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_details_TenantId",
                table: "attendance_details",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_GuardianId",
                table: "student_guardians",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_StudentId",
                table: "student_guardians",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_TenantId_StudentId_GuardianId",
                table: "student_guardians",
                columns: new[] { "TenantId", "StudentId", "GuardianId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_TenantId_StudentId_IsPrimary",
                table: "student_guardians",
                columns: new[] { "TenantId", "StudentId", "IsPrimary" });

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_details_attendance_records_AttendanceRecordId",
                table: "attendance_details",
                column: "AttendanceRecordId",
                principalTable: "attendance_records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_details_attendance_records_AttendanceRecordId",
                table: "attendance_details");

            migrationBuilder.DropTable(
                name: "student_guardians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendance_records",
                table: "attendance_records");

            migrationBuilder.DropIndex(
                name: "IX_attendance_records_TenantId_TrainingSessionId",
                table: "attendance_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_attendance_details",
                table: "attendance_details");

            migrationBuilder.DropIndex(
                name: "IX_attendance_details_AttendanceRecordId_StudentId",
                table: "attendance_details");

            migrationBuilder.DropIndex(
                name: "IX_attendance_details_MarkedAt",
                table: "attendance_details");

            migrationBuilder.DropIndex(
                name: "IX_attendance_details_StudentId",
                table: "attendance_details");

            migrationBuilder.DropIndex(
                name: "IX_attendance_details_TenantId",
                table: "attendance_details");

            migrationBuilder.DropColumn(
                name: "IsBackfilled",
                table: "attendance_details");

            migrationBuilder.DropColumn(
                name: "MarkedAt",
                table: "attendance_details");

            migrationBuilder.DropColumn(
                name: "MarkedByUserId",
                table: "attendance_details");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "attendance_details");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "attendance_details");

            migrationBuilder.RenameTable(
                name: "attendance_records",
                newName: "AttendanceRecords");

            migrationBuilder.RenameTable(
                name: "attendance_details",
                newName: "AttendanceDetails");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AttendanceDetails",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "AttendanceDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceDetails",
                table: "AttendanceDetails",
                column: "Id");
        }
    }
}

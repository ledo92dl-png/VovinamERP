using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCreditTuitionAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TuitionAdjustmentId",
                table: "StudentCreditTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCreditTransactions_TuitionAdjustmentId",
                table: "StudentCreditTransactions",
                column: "TuitionAdjustmentId",
                unique: true,
                filter: "\"TuitionAdjustmentId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCreditTransactions_TuitionAdjustmentId",
                table: "StudentCreditTransactions");

            migrationBuilder.DropColumn(
                name: "TuitionAdjustmentId",
                table: "StudentCreditTransactions");
        }
    }
}

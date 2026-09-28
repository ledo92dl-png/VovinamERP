using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIdToReceiptItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "receipt_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_receipt_items_StudentId",
                table: "receipt_items",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_items_TenantId_StudentId",
                table: "receipt_items",
                columns: new[] { "TenantId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_receipt_items_StudentId",
                table: "receipt_items");

            migrationBuilder.DropIndex(
                name: "IX_receipt_items_TenantId_StudentId",
                table: "receipt_items");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "receipt_items");
        }
    }
}

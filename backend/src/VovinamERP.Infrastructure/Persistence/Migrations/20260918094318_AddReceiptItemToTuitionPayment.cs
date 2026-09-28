using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptItemToTuitionPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptItemId",
                table: "TuitionPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPayments_ReceiptItemId",
                table: "TuitionPayments",
                column: "ReceiptItemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TuitionPayments_ReceiptItemId",
                table: "TuitionPayments");

            migrationBuilder.DropColumn(
                name: "ReceiptItemId",
                table: "TuitionPayments");
        }
    }
}

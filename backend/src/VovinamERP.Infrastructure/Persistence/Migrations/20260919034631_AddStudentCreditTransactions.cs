using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCreditTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentCreditTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiptItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    TuitionInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCreditTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCreditTransactions_ReceiptId",
                table: "StudentCreditTransactions",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCreditTransactions_ReceiptItemId",
                table: "StudentCreditTransactions",
                column: "ReceiptItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCreditTransactions_TenantId_StudentId_TransactionDate",
                table: "StudentCreditTransactions",
                columns: new[] { "TenantId", "StudentId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCreditTransactions_TuitionInvoiceId",
                table: "StudentCreditTransactions",
                column: "TuitionInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentCreditTransactions");
        }
    }
}

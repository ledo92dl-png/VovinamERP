using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTuitionInvoiceUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TuitionInvoices_TenantId_InvoiceNumber",
                table: "TuitionInvoices",
                columns: new[] { "TenantId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionInvoices_TenantId_StudentId_Year_Month",
                table: "TuitionInvoices",
                columns: new[] { "TenantId", "StudentId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TuitionInvoices_TenantId_InvoiceNumber",
                table: "TuitionInvoices");

            migrationBuilder.DropIndex(
                name: "IX_TuitionInvoices_TenantId_StudentId_Year_Month",
                table: "TuitionInvoices");
        }
    }
}

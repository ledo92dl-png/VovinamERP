using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTuitionSpecialDiscountTypeAndValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialDiscountType",
                table: "TuitionInvoices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialDiscountValue",
                table: "TuitionInvoices",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialDiscountType",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "SpecialDiscountValue",
                table: "TuitionInvoices");
        }
    }
}

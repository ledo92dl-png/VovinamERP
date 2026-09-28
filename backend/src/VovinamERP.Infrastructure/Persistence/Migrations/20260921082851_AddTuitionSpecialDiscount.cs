using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTuitionSpecialDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SpecialDiscountAmount",
                table: "TuitionInvoices",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "SpecialDiscountApprovedAtUtc",
                table: "TuitionInvoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialDiscountApprovedByUserId",
                table: "TuitionInvoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialDiscountReason",
                table: "TuitionInvoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialDiscountAmount",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "SpecialDiscountApprovedAtUtc",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "SpecialDiscountApprovedByUserId",
                table: "TuitionInvoices");

            migrationBuilder.DropColumn(
                name: "SpecialDiscountReason",
                table: "TuitionInvoices");
        }
    }
}

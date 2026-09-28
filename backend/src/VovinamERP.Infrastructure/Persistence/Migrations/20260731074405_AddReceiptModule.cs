using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VovinamERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TuitionInvoiceId",
                table: "Receipts");

            migrationBuilder.RenameTable(
                name: "Receipts",
                newName: "receipts");

            migrationBuilder.DropColumn(
    name: "TuitionPaymentId",
    table: "receipts");

migrationBuilder.AddColumn<Guid>(
    name: "CollectedByUserId",
    table: "receipts",
    type: "uuid",
    nullable: false,
    defaultValue: Guid.Empty);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "receipts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptNumber",
                table: "receipts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "receipts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "receipts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceImageUrl",
                table: "receipts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "receipts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "receipts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_receipts",
                table: "receipts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "receipt_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipt_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receipt_items_receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_receipts_TenantId_CollectedByUserId",
                table: "receipts",
                columns: new[] { "TenantId", "CollectedByUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_receipts_TenantId_ReceiptDate",
                table: "receipts",
                columns: new[] { "TenantId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_receipts_TenantId_ReceiptNumber",
                table: "receipts",
                columns: new[] { "TenantId", "ReceiptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_receipt_items_ReceiptId",
                table: "receipt_items",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_items_ReceiptId_ItemType",
                table: "receipt_items",
                columns: new[] { "ReceiptId", "ItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_receipt_items_TenantId_ReceiptId",
                table: "receipt_items",
                columns: new[] { "TenantId", "ReceiptId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receipt_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_receipts",
                table: "receipts");

            migrationBuilder.DropIndex(
                name: "IX_receipts_TenantId_CollectedByUserId",
                table: "receipts");

            migrationBuilder.DropIndex(
                name: "IX_receipts_TenantId_ReceiptDate",
                table: "receipts");

            migrationBuilder.DropIndex(
                name: "IX_receipts_TenantId_ReceiptNumber",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "EvidenceImageUrl",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "receipts");

            migrationBuilder.DropColumn(
    name: "CollectedByUserId",
    table: "receipts");
            
            migrationBuilder.RenameTable(
                name: "receipts",
                newName: "Receipts");

            migrationBuilder.AddColumn<Guid>(
    name: "TuitionPaymentId",
    table: "Receipts",
    type: "uuid",
    nullable: false,
    defaultValue: Guid.Empty);
    
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Receipts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptNumber",
                table: "Receipts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Receipts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Receipts",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "TuitionInvoiceId",
                table: "Receipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts",
                column: "Id");
        }
    }
}

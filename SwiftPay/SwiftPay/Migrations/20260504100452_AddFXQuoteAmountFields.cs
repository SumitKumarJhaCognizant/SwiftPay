using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftPay.Migrations
{
    /// <inheritdoc />
    public partial class AddFXQuoteAmountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "FXQuote",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceiverAmount",
                table: "FXQuote",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SendAmount",
                table: "FXQuote",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "FXQuote");

            migrationBuilder.DropColumn(
                name: "ReceiverAmount",
                table: "FXQuote");

            migrationBuilder.DropColumn(
                name: "SendAmount",
                table: "FXQuote");
        }
    }
}

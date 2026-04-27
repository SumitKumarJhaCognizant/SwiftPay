using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftPay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFKeyAmendment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RefundRefs_RemitID",
                table: "RefundRefs",
                column: "RemitID");

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_RemitID",
                table: "Cancellations",
                column: "RemitID");

            migrationBuilder.CreateIndex(
                name: "IX_Amendments_RemitID",
                table: "Amendments",
                column: "RemitID");

            migrationBuilder.AddForeignKey(
                name: "FK_Amendments_RemittanceRequests_RemitID",
                table: "Amendments",
                column: "RemitID",
                principalTable: "RemittanceRequests",
                principalColumn: "RemitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cancellations_RemittanceRequests_RemitID",
                table: "Cancellations",
                column: "RemitID",
                principalTable: "RemittanceRequests",
                principalColumn: "RemitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRefs_RemittanceRequests_RemitID",
                table: "RefundRefs",
                column: "RemitID",
                principalTable: "RemittanceRequests",
                principalColumn: "RemitId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Amendments_RemittanceRequests_RemitID",
                table: "Amendments");

            migrationBuilder.DropForeignKey(
                name: "FK_Cancellations_RemittanceRequests_RemitID",
                table: "Cancellations");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRefs_RemittanceRequests_RemitID",
                table: "RefundRefs");

            migrationBuilder.DropIndex(
                name: "IX_RefundRefs_RemitID",
                table: "RefundRefs");

            migrationBuilder.DropIndex(
                name: "IX_Cancellations_RemitID",
                table: "Cancellations");

            migrationBuilder.DropIndex(
                name: "IX_Amendments_RemitID",
                table: "Amendments");
        }
    }
}

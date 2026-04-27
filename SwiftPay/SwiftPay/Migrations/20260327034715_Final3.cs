using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftPay.Migrations
{
    /// <inheritdoc />
    public partial class Final3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Amendments_RequestedBy",
                table: "Amendments",
                column: "RequestedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Amendments_User_RequestedBy",
                table: "Amendments",
                column: "RequestedBy",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Amendments_User_RequestedBy",
                table: "Amendments");

            migrationBuilder.DropIndex(
                name: "IX_Amendments_RequestedBy",
                table: "Amendments");
        }
    }
}

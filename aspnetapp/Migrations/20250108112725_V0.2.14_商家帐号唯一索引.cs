using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V0214_商家帐号唯一索引 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_StoreAccount_TheStore_Account",
                table: "T_StoreAccount");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreAccount_Account",
                table: "T_StoreAccount",
                column: "Account",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_StoreAccount_Account",
                table: "T_StoreAccount");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreAccount_TheStore_Account",
                table: "T_StoreAccount",
                columns: new[] { "TheStore", "Account" },
                unique: true);
        }
    }
}

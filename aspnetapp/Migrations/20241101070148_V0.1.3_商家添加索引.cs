using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V013_商家添加索引 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_StoreAccount_TheStore",
                table: "T_StoreAccount");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                table: "T_StoreAccount",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8")
                .OldAnnotation("MySql:CharSet", "utf8")
                .OldAnnotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreAccount_TheStore_Account",
                table: "T_StoreAccount",
                columns: new[] { "TheStore", "Account" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_StoreAccount_TheStore_Account",
                table: "T_StoreAccount");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                table: "T_StoreAccount",
                type: "longtext",
                nullable: false,
                collation: "utf8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8")
                .OldAnnotation("MySql:CharSet", "utf8")
                .OldAnnotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreAccount_TheStore",
                table: "T_StoreAccount",
                column: "TheStore",
                unique: true);
        }
    }
}

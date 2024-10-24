using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class v004_Order表添加字段Vehicle_UserName_UserPhone_IdentityCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityCard",
                table: "T_OrderForm",
                type: "longtext",
                nullable: true,
                collation: "utf8_general_ci")
                .Annotation("MySql:CharSet", "utf8");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "T_OrderForm",
                type: "longtext",
                nullable: false,
                collation: "utf8_general_ci")
                .Annotation("MySql:CharSet", "utf8");

            migrationBuilder.AddColumn<string>(
                name: "UserPhone",
                table: "T_OrderForm",
                type: "longtext",
                nullable: false,
                collation: "utf8_general_ci")
                .Annotation("MySql:CharSet", "utf8");

            migrationBuilder.AddColumn<int>(
                name: "Vehicle",
                table: "T_OrderForm",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityCard",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "UserPhone",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "Vehicle",
                table: "T_OrderForm");
        }
    }
}

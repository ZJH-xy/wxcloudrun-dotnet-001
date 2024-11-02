using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V016_删除长租属性 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongTermLease",
                table: "T_OrderForm");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LongTermLease",
                table: "T_OrderForm",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}

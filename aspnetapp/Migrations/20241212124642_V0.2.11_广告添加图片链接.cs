using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V0211_广告添加图片链接 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pictures",
                table: "T_Advertisement",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pictures",
                table: "T_Advertisement");
        }
    }
}

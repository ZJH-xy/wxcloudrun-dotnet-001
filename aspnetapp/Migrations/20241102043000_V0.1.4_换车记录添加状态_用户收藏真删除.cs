using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V014_换车记录添加状态_用户收藏真删除 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "T_UserFavoritesStore");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "T_VehicleReplacementRecord",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "T_VehicleReplacementRecord");

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "T_UserFavoritesStore",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}

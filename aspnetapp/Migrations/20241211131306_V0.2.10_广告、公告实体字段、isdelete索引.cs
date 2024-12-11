using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V0210_广告公告实体字段isdelete索引 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State",
                table: "T_VehicleSummary");

            migrationBuilder.DropColumn(
                name: "Src",
                table: "T_Notice");

            migrationBuilder.AddColumn<string>(
                name: "Src",
                table: "T_Advertisement",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State_IsDe~",
                table: "T_VehicleSummary",
                columns: new[] { "TheOriginalStore", "TheCurrentStore", "State", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreSummary_IsDelete",
                table: "T_StoreSummary",
                column: "IsDelete");

            migrationBuilder.CreateIndex(
                name: "IX_T_Notice_IsDelete",
                table: "T_Notice",
                column: "IsDelete");

            migrationBuilder.CreateIndex(
                name: "IX_T_Advertisement_IsDelete",
                table: "T_Advertisement",
                column: "IsDelete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State_IsDe~",
                table: "T_VehicleSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_StoreSummary_IsDelete",
                table: "T_StoreSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_Notice_IsDelete",
                table: "T_Notice");

            migrationBuilder.DropIndex(
                name: "IX_T_Advertisement_IsDelete",
                table: "T_Advertisement");

            migrationBuilder.DropColumn(
                name: "Src",
                table: "T_Advertisement");

            migrationBuilder.AddColumn<string>(
                name: "Src",
                table: "T_Notice",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State",
                table: "T_VehicleSummary",
                columns: new[] { "TheOriginalStore", "TheCurrentStore", "State" });
        }
    }
}

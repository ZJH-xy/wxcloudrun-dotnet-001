using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V_011_订单时间类型_重命名 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreA_Amount",
                table: "T_RevenueStatistics");

            migrationBuilder.DropColumn(
                name: "StoreB_Amount",
                table: "T_RevenueStatistics");

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyStoreA",
                table: "T_RevenueStatistics",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyStoreB",
                table: "T_RevenueStatistics",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyStoreA",
                table: "T_RevenueStatistics");

            migrationBuilder.DropColumn(
                name: "MoneyStoreB",
                table: "T_RevenueStatistics");

            migrationBuilder.AddColumn<decimal>(
                name: "StoreA_Amount",
                table: "T_RevenueStatistics",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StoreB_Amount",
                table: "T_RevenueStatistics",
                type: "decimal(65,30)",
                nullable: true);
        }
    }
}

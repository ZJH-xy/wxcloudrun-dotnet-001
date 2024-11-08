using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V018_乐观并发控制 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_VehicleSummary",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_VehicleReplacementRecord",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_Users",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_UserFavoritesStore",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_StoreSummary",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_StoreMenu",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_StoreAccount",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_RevenueStatistics",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_OrderForm",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_HomepageAd",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "RowVersion",
                table: "T_AdminAccount",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_VehicleSummary");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_VehicleReplacementRecord");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_UserFavoritesStore");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_StoreSummary");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_StoreMenu");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_StoreAccount");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_RevenueStatistics");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_HomepageAd");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "T_AdminAccount");
        }
    }
}

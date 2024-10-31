using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V_012_订单更改 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedReturnTime",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "StartingTime",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "TheTransferPoint",
                table: "T_OrderForm");

            migrationBuilder.RenameColumn(
                name: "BusinessHoursBegin",
                table: "T_StoreSummary",
                newName: "BusinessHoursEnd");

            migrationBuilder.AddColumn<int>(
                name: "TheStoreMenu",
                table: "T_OrderForm",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TheStoreMenu",
                table: "T_OrderForm");

            migrationBuilder.RenameColumn(
                name: "BusinessHoursEnd",
                table: "T_StoreSummary",
                newName: "BusinessHoursBegin");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedReturnTime",
                table: "T_OrderForm",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartingTime",
                table: "T_OrderForm",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TheTransferPoint",
                table: "T_OrderForm",
                type: "int",
                nullable: true);
        }
    }
}

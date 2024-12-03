using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V021_订单表添加TransactionId微信支付系统生成的订单号SuccessTime支付完成时间 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SuccessTime",
                table: "T_OrderForm",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "T_OrderForm",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_TransactionId",
                table: "T_OrderForm",
                column: "TransactionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_TransactionId",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "SuccessTime",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "T_OrderForm");
        }
    }
}

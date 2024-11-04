using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class V017_超时费 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeFee",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeFee",
                table: "T_OrderForm");
        }
    }
}

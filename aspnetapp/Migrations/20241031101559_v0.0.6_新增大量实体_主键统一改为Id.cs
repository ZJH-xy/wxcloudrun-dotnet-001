using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class v006_新增大量实体_主键统一改为Id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_OriginalStore_CurrentStore",
                table: "T_VehicleSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_TheUser",
                table: "T_OrderForm");

            migrationBuilder.RenameColumn(
                name: "OriginalStore",
                table: "T_VehicleSummary",
                newName: "TheOriginalStore");

            migrationBuilder.RenameColumn(
                name: "CurrentStore",
                table: "T_VehicleSummary",
                newName: "TheCurrentStore");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "T_VehicleSummary",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "T_Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "T_StoreSummary",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Vehicle",
                table: "T_OrderForm",
                newName: "TheVehicle");

            migrationBuilder.RenameColumn(
                name: "TransferPoint",
                table: "T_OrderForm",
                newName: "TheTransferPoint");

            migrationBuilder.RenameColumn(
                name: "ReturnThePoint",
                table: "T_OrderForm",
                newName: "TheReturnThePoint");

            migrationBuilder.RenameColumn(
                name: "RentalLocation",
                table: "T_OrderForm",
                newName: "TheRentalLocation");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "T_OrderForm",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AdId",
                table: "T_HomepageAd",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_Users",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8")
                .OldAnnotation("MySql:CharSet", "utf8")
                .OldAnnotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartingTime",
                table: "T_OrderForm",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "T_AdminAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Account = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Password = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AdminAccount", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_RevenueStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TheStoreA = table.Column<int>(type: "int", nullable: false),
                    TheStoreB = table.Column<int>(type: "int", nullable: false),
                    TheOrder = table.Column<int>(type: "int", nullable: false),
                    StoreA_Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    StoreB_Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_RevenueStatistics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_StoreAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TheStore = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Password = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_StoreAccount", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_StoreMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TheStore = table.Column<int>(type: "int", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Rent = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Deposit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsDelete = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_StoreMenu", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_UserFavoritesStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TheUser = table.Column<int>(type: "int", nullable: false),
                    TheStore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDelete = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_UserFavoritesStore", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_VehicleReplacementRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TheOrder = table.Column<int>(type: "int", nullable: false),
                    TheOldVehicles = table.Column<int>(type: "int", nullable: false),
                    TheNewVehicles = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VehicleReplacementRecord", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State",
                table: "T_VehicleSummary",
                columns: new[] { "TheOriginalStore", "TheCurrentStore", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_T_Users_Phone",
                table: "T_Users",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_TheUser_Status",
                table: "T_OrderForm",
                columns: new[] { "TheUser", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_T_RevenueStatistics_TheStoreA_TheStoreB",
                table: "T_RevenueStatistics",
                columns: new[] { "TheStoreA", "TheStoreB" });

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreAccount_TheStore",
                table: "T_StoreAccount",
                column: "TheStore",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreMenu_TheStore",
                table: "T_StoreMenu",
                column: "TheStore");

            migrationBuilder.CreateIndex(
                name: "IX_T_UserFavoritesStore_TheUser",
                table: "T_UserFavoritesStore",
                column: "TheUser");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleReplacementRecord_TheOrder",
                table: "T_VehicleReplacementRecord",
                column: "TheOrder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_AdminAccount");

            migrationBuilder.DropTable(
                name: "T_RevenueStatistics");

            migrationBuilder.DropTable(
                name: "T_StoreAccount");

            migrationBuilder.DropTable(
                name: "T_StoreMenu");

            migrationBuilder.DropTable(
                name: "T_UserFavoritesStore");

            migrationBuilder.DropTable(
                name: "T_VehicleReplacementRecord");

            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_TheOriginalStore_TheCurrentStore_State",
                table: "T_VehicleSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_Users_Phone",
                table: "T_Users");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_TheUser_Status",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "ActualStartingTime",
                table: "T_OrderForm");

            migrationBuilder.RenameColumn(
                name: "TheOriginalStore",
                table: "T_VehicleSummary",
                newName: "OriginalStore");

            migrationBuilder.RenameColumn(
                name: "TheCurrentStore",
                table: "T_VehicleSummary",
                newName: "CurrentStore");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_VehicleSummary",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_Users",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_StoreSummary",
                newName: "StoreId");

            migrationBuilder.RenameColumn(
                name: "TheVehicle",
                table: "T_OrderForm",
                newName: "Vehicle");

            migrationBuilder.RenameColumn(
                name: "TheTransferPoint",
                table: "T_OrderForm",
                newName: "TransferPoint");

            migrationBuilder.RenameColumn(
                name: "TheReturnThePoint",
                table: "T_OrderForm",
                newName: "ReturnThePoint");

            migrationBuilder.RenameColumn(
                name: "TheRentalLocation",
                table: "T_OrderForm",
                newName: "RentalLocation");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_OrderForm",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_HomepageAd",
                newName: "AdId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "T_Users",
                type: "longtext",
                nullable: false,
                collation: "utf8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8")
                .OldAnnotation("MySql:CharSet", "utf8")
                .OldAnnotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_OriginalStore_CurrentStore",
                table: "T_VehicleSummary",
                columns: new[] { "OriginalStore", "CurrentStore" });

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_TheUser",
                table: "T_OrderForm",
                column: "TheUser");
        }
    }
}

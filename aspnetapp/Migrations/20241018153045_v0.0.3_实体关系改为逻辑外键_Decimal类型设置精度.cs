using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class v003_实体关系改为逻辑外键_Decimal类型设置精度 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_RentalLocationStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_ReturnThePointStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_TransferPointStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropForeignKey(
                name: "FK_T_OrderForm_T_Users_UserId",
                table: "T_OrderForm");

            migrationBuilder.DropForeignKey(
                name: "FK_T_StoreSummary_T_Users_UserId",
                table: "T_StoreSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_T_VehicleSummary_T_StoreSummary_StoreId",
                table: "T_VehicleSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_StoreId",
                table: "T_VehicleSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_StoreSummary_UserId",
                table: "T_StoreSummary");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_RentalLocationStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_ReturnThePointStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropIndex(
                name: "IX_T_OrderForm_TransferPointStoreId",
                table: "T_OrderForm");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "T_StoreSummary");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "T_VehicleSummary",
                newName: "OriginalStore");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "T_OrderForm",
                newName: "TheUser");

            migrationBuilder.RenameColumn(
                name: "TransferPointStoreId",
                table: "T_OrderForm",
                newName: "TransferPoint");

            migrationBuilder.RenameColumn(
                name: "ReturnThePointStoreId",
                table: "T_OrderForm",
                newName: "ReturnThePoint");

            migrationBuilder.RenameColumn(
                name: "RentalLocationStoreId",
                table: "T_OrderForm",
                newName: "RentalLocation");

            migrationBuilder.RenameIndex(
                name: "IX_T_OrderForm_UserId",
                table: "T_OrderForm",
                newName: "IX_T_OrderForm_TheUser");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStore",
                table: "T_VehicleSummary",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Drivinglicense",
                table: "T_VehicleSummary",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rent",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Paid",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OtherFees",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DispatchFee",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DepositRefunded",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Deposit",
                table: "T_OrderForm",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_OriginalStore_CurrentStore",
                table: "T_VehicleSummary",
                columns: new[] { "OriginalStore", "CurrentStore" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_VehicleSummary_OriginalStore_CurrentStore",
                table: "T_VehicleSummary");

            migrationBuilder.DropColumn(
                name: "CurrentStore",
                table: "T_VehicleSummary");

            migrationBuilder.DropColumn(
                name: "Drivinglicense",
                table: "T_VehicleSummary");

            migrationBuilder.RenameColumn(
                name: "OriginalStore",
                table: "T_VehicleSummary",
                newName: "StoreId");

            migrationBuilder.RenameColumn(
                name: "TransferPoint",
                table: "T_OrderForm",
                newName: "TransferPointStoreId");

            migrationBuilder.RenameColumn(
                name: "TheUser",
                table: "T_OrderForm",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ReturnThePoint",
                table: "T_OrderForm",
                newName: "ReturnThePointStoreId");

            migrationBuilder.RenameColumn(
                name: "RentalLocation",
                table: "T_OrderForm",
                newName: "RentalLocationStoreId");

            migrationBuilder.RenameIndex(
                name: "IX_T_OrderForm_TheUser",
                table: "T_OrderForm",
                newName: "IX_T_OrderForm_UserId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "T_StoreSummary",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rent",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Paid",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "OtherFees",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DispatchFee",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DepositRefunded",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Deposit",
                table: "T_OrderForm",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_StoreId",
                table: "T_VehicleSummary",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreSummary_UserId",
                table: "T_StoreSummary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_RentalLocationStoreId",
                table: "T_OrderForm",
                column: "RentalLocationStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_ReturnThePointStoreId",
                table: "T_OrderForm",
                column: "ReturnThePointStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_TransferPointStoreId",
                table: "T_OrderForm",
                column: "TransferPointStoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_RentalLocationStoreId",
                table: "T_OrderForm",
                column: "RentalLocationStoreId",
                principalTable: "T_StoreSummary",
                principalColumn: "StoreId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_ReturnThePointStoreId",
                table: "T_OrderForm",
                column: "ReturnThePointStoreId",
                principalTable: "T_StoreSummary",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderForm_T_StoreSummary_TransferPointStoreId",
                table: "T_OrderForm",
                column: "TransferPointStoreId",
                principalTable: "T_StoreSummary",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_OrderForm_T_Users_UserId",
                table: "T_OrderForm",
                column: "UserId",
                principalTable: "T_Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_T_StoreSummary_T_Users_UserId",
                table: "T_StoreSummary",
                column: "UserId",
                principalTable: "T_Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_VehicleSummary_T_StoreSummary_StoreId",
                table: "T_VehicleSummary",
                column: "StoreId",
                principalTable: "T_StoreSummary",
                principalColumn: "StoreId");
        }
    }
}

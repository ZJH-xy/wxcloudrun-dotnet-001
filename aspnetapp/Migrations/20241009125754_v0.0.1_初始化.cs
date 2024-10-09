using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnetapp.Migrations
{
    public partial class v001_初始化 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8");

            migrationBuilder.CreateTable(
                name: "T_HomepageAd",
                columns: table => new
                {
                    AdId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PictureLink = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Jumplink = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_HomepageAd", x => x.AdId);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Phone = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Password = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Name = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    IdentityCard = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Nickname = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_StoreSummary",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    BusinessHoursStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BusinessHoursBegin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BusinessStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Telephone = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    WeChat = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Address = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    GpsLongitude = table.Column<double>(type: "double", nullable: false),
                    GpsLatitude = table.Column<double>(type: "double", nullable: false),
                    Pictures = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Introduce = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDelete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_StoreSummary", x => x.StoreId);
                    table.ForeignKey(
                        name: "FK_T_StoreSummary_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "UserId");
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_OrderForm",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartingTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpectedReturnTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ActualReturnTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RentalLocationStoreId = table.Column<int>(type: "int", nullable: false),
                    TransferPointStoreId = table.Column<int>(type: "int", nullable: true),
                    ReturnThePointStoreId = table.Column<int>(type: "int", nullable: true),
                    LongTermLease = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Deposit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Rent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DispatchFee = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OtherFees = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Paid = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DepositRefunded = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_OrderForm", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_StoreSummary_RentalLocationStoreId",
                        column: x => x.RentalLocationStoreId,
                        principalTable: "T_StoreSummary",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_StoreSummary_ReturnThePointStoreId",
                        column: x => x.ReturnThePointStoreId,
                        principalTable: "T_StoreSummary",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_StoreSummary_TransferPointStoreId",
                        column: x => x.TransferPointStoreId,
                        principalTable: "T_StoreSummary",
                        principalColumn: "StoreId");
                    table.ForeignKey(
                        name: "FK_T_OrderForm_T_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

            migrationBuilder.CreateTable(
                name: "T_VehicleSummary",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    Model = table.Column<int>(type: "int", nullable: false),
                    PlateNumber = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    FrameNumber = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    Certificate = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Invoice = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PurchaseRegistrationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Owner = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    VehicleIntroduction = table.Column<string>(type: "longtext", nullable: true, collation: "utf8_general_ci")
                        .Annotation("MySql:CharSet", "utf8"),
                    State = table.Column<int>(type: "int", nullable: false),
                    IsCase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StateUpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDelete = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VehicleSummary", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_T_VehicleSummary_T_StoreSummary_StoreId",
                        column: x => x.StoreId,
                        principalTable: "T_StoreSummary",
                        principalColumn: "StoreId");
                })
                .Annotation("MySql:CharSet", "utf8")
                .Annotation("Relational:Collation", "utf8_general_ci");

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

            migrationBuilder.CreateIndex(
                name: "IX_T_OrderForm_UserId",
                table: "T_OrderForm",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_StoreSummary_UserId",
                table: "T_StoreSummary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_T_VehicleSummary_StoreId",
                table: "T_VehicleSummary",
                column: "StoreId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_HomepageAd");

            migrationBuilder.DropTable(
                name: "T_OrderForm");

            migrationBuilder.DropTable(
                name: "T_VehicleSummary");

            migrationBuilder.DropTable(
                name: "T_StoreSummary");

            migrationBuilder.DropTable(
                name: "T_Users");
        }
    }
}

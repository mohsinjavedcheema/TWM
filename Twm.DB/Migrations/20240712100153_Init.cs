using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Twm.DB.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "DataProviders",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizerResults",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Guid = table.Column<string>(nullable: true),
                    StrategyGuid = table.Column<string>(nullable: true),
                    StrategyVersion = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    DataSeriesType = table.Column<string>(nullable: true),
                    DataSeriesValue = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizerResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Guid = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemOptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true),
                    ValueType = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    ValueDate = table.Column<DateTime>(nullable: false),
                    ValueDouble = table.Column<double>(nullable: false),
                    ValueInt = table.Column<int>(nullable: false),
                    ValueBool = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewOptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    DataProviderId = table.Column<int>(nullable: false),
                    IsSystem = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_DataProviders_DataProviderId",
                        column: x => x.DataProviderId,
                        principalSchema: "public",
                        principalTable: "DataProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalMetaDatas",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(nullable: true),
                    DataType = table.Column<string>(nullable: true),
                    InstrumentType = table.Column<string>(nullable: true),
                    DataSeriesValue = table.Column<int>(nullable: false),
                    DataSeriesType = table.Column<string>(nullable: true),
                    IsTest = table.Column<bool>(nullable: false),
                    PeriodStart = table.Column<DateTime>(nullable: false),
                    PeriodEnd = table.Column<DateTime>(nullable: false),
                    DataProviderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalMetaDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalMetaDatas_DataProviders_DataProviderId",
                        column: x => x.DataProviderId,
                        principalSchema: "public",
                        principalTable: "DataProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionOptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    ConnectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionOptions_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalSchema: "public",
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentLists",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: false),
                    ConnectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentLists_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalSchema: "public",
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instruments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DpId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    Base = table.Column<string>(nullable: true),
                    Quote = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    MinLotSize = table.Column<double>(nullable: true),
                    Notional = table.Column<double>(nullable: true),
                    Multiplier = table.Column<double>(nullable: true),
                    PriceIncrements = table.Column<string>(nullable: true),
                    TradingHours = table.Column<string>(nullable: true),
                    ProviderData = table.Column<string>(nullable: true),
                    ConnectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instruments_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalSchema: "public",
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentInstrumentLists",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstrumentId = table.Column<int>(nullable: false),
                    InstrumentListId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentInstrumentLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentInstrumentLists_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalSchema: "public",
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentInstrumentLists_InstrumentLists_InstrumentListId",
                        column: x => x.InstrumentListId,
                        principalSchema: "public",
                        principalTable: "InstrumentLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentMaps",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstInstrumentId = table.Column<int>(nullable: false),
                    SecondInstrumentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentMaps_Instruments_FirstInstrumentId",
                        column: x => x.FirstInstrumentId,
                        principalSchema: "public",
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentMaps_Instruments_SecondInstrumentId",
                        column: x => x.SecondInstrumentId,
                        principalSchema: "public",
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "DataProviders",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[] { 3, "Bybit", "Bybit" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "DataProviders",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[] { 4, "Binance", "Binance" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 24, "General", "EmailPassword", "Email", "Email password", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 23, "General", "EmailUsername", "Email", "Email username", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 22, "General", "EmailPort", "Email", "Email port", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, "int" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 21, "General", "EmailHost", "Email", "Email host", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 20, "Calculation", "CalculateSimulation", "Optimizer", "Calculate simulation", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, "bool" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 18, "General", "TimeZone", "Preferences", "Time zone", "", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 17, "General", "MarkerTextColor", "Display", "Marker text color", "Black", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 16, "General", "PlotExecutions", "Display", "Plot executions", "1", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 15, "General", "IndicatorSeparatorWidth", "Display", "Indicator separator width", "1", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 14, "General", "IndicatorSeparatorColor", "Display", "Indicator separator color", "LightGray", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 13, "General", "TextColor", "Display", "Chart axis text color", "Black", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 12, "General", "ChartHGridColor", "Display", "Chart horizontal grid color", "LightGray", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 11, "General", "ChartVGridColor", "Display", "Chart vertical grid color", "LightGray", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 10, "General", "ChartBackgroundColor", "Display", "Chart background color", "White", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 9, "General", "CandleWickColor", "Display", "Candle wick color", "Black", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 8, "General", "CandleOutlineColor", "Display", "Candle body outline color", "Black", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 7, "General", "UpBarColor", "Display", "Color for up bars", "LimeGreen", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 6, "General", "DownBarColor", "Display", "Color for down bars", "Red", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 5, "General", "TradeSellColor", "Display", "Trade sell color", "Magenta", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 4, "General", "TradeBuyColor", "Display", "Trade buy color", "Blue", false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 3, "General", "CompileDebug", "Project", "Compile in debug", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, "bool" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 2, "General", "CstPrjDllPath", "Project", "Custom project dll path", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 1, "General", "CstPrjPath", "Project", "Custom project path", null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 25, "General", "LogInFile", "Preferences", "Log in file", null, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, "bool" });

            migrationBuilder.InsertData(
                schema: "public",
                table: "SystemOptions",
                columns: new[] { "Id", "Category", "Code", "Group", "Name", "Value", "ValueBool", "ValueDate", "ValueDouble", "ValueInt", "ValueType" },
                values: new object[] { 29, "General", "ReloadOnRecompile", "Project", "Reload script on recompile", null, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 0, "bool" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionOptions_ConnectionId",
                schema: "public",
                table: "ConnectionOptions",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_DataProviderId",
                schema: "public",
                table: "Connections",
                column: "DataProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalMetaDatas_DataProviderId",
                schema: "public",
                table: "HistoricalMetaDatas",
                column: "DataProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentInstrumentLists_InstrumentId",
                schema: "public",
                table: "InstrumentInstrumentLists",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentInstrumentLists_InstrumentListId",
                schema: "public",
                table: "InstrumentInstrumentLists",
                column: "InstrumentListId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentLists_ConnectionId",
                schema: "public",
                table: "InstrumentLists",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentMaps_FirstInstrumentId",
                schema: "public",
                table: "InstrumentMaps",
                column: "FirstInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentMaps_SecondInstrumentId",
                schema: "public",
                table: "InstrumentMaps",
                column: "SecondInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ConnectionId",
                schema: "public",
                table: "Instruments",
                column: "ConnectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionOptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HistoricalMetaDatas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InstrumentInstrumentLists",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InstrumentMaps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptimizerResults",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Presets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SystemOptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ViewOptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InstrumentLists",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Instruments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Connections",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DataProviders",
                schema: "public");
        }
    }
}

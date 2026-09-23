using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: true),
                    IsRestricted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartNumber = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    PackageType = table.Column<string>(type: "text", nullable: false),
                    PinCount = table.Column<int>(type: "integer", nullable: false),
                    MinOperatingTemp = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxOperatingTemp = table.Column<decimal>(type: "numeric", nullable: false),
                    MinVoltage = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxVoltage = table.Column<decimal>(type: "numeric", nullable: false),
                    IsMilSpec = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    AdditionalFeatures = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPartId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchScore = table.Column<int>(type: "integer", nullable: false),
                    MatchReason = table.Column<string>(type: "text", nullable: false),
                    VerifiedByHuman = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeMatches_Parts_SourcePartId",
                        column: x => x.SourcePartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlternativeMatches_Parts_TargetPartId",
                        column: x => x.TargetPartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Datasheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExtractedRawText = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datasheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Datasheets_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeMatches_SourcePartId",
                table: "AlternativeMatches",
                column: "SourcePartId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeMatches_TargetPartId",
                table: "AlternativeMatches",
                column: "TargetPartId");

            migrationBuilder.CreateIndex(
                name: "IX_Datasheets_PartId",
                table: "Datasheets",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ManufacturerId",
                table: "Parts",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeMatches");

            migrationBuilder.DropTable(
                name: "Datasheets");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Manufacturers");
        }
    }
}

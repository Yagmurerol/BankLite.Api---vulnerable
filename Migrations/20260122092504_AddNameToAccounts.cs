using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankLite.Api.Migrations
{
    public partial class AddNameToAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "TwoFactorChallenges");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "PayloadJson",
                table: "Receipts",
                newName: "ContentJson");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Transfers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtpRequired",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OtpVerified",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // ✅ SADECE BİR KERE EKLENİYOR
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TermDeposits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    PayoutAccountId = table.Column<int>(type: "int", nullable: false),
                    Principal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaturityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayoutAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermDeposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermDeposits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermDeposits_UserId",
                table: "TermDeposits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers",
                column: "FromAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "TermDeposits");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "OtpRequired",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "OtpVerified",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "ContentJson",
                table: "Receipts",
                newName: "PayloadJson");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Transfers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TwoFactorChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorChallenges_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TwoFactorChallenges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorChallenges_TransferId",
                table: "TwoFactorChallenges",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorChallenges_UserId",
                table: "TwoFactorChallenges",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers",
                column: "FromAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

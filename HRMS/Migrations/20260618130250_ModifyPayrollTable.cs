using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPayrollTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayrollMonth",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PayrollYear",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "Deductions",
                table: "Payrolls",
                newName: "Tax");

            migrationBuilder.RenameColumn(
                name: "Allowances",
                table: "Payrolls",
                newName: "PF");

            migrationBuilder.AddColumn<decimal>(
                name: "DA",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);



            migrationBuilder.AlterColumn<string>(
                name: "HolidayName",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Holidays",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "Holidays",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DA",
                table: "Payrolls");



            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "Holidays");

            migrationBuilder.RenameColumn(
                name: "Tax",
                table: "Payrolls",
                newName: "Deductions");

            migrationBuilder.RenameColumn(
                name: "PF",
                table: "Payrolls",
                newName: "Allowances");

            migrationBuilder.AddColumn<int>(
                name: "PayrollMonth",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PayrollYear",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "HolidayName",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class Ini3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComment",
                table: "LeaveRequests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdjustedAt",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdjustedBy",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdjustmentReason",
                table: "Attendances",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "CheckInLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckInLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInPhotoUrl",
                table: "Attendances",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutPhotoUrl",
                table: "Attendances",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AttendanceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceId = table.Column<int>(type: "int", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceHistories_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceHistories_AttendanceId",
                table: "AttendanceHistories",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceHistories_ChangedBy",
                table: "AttendanceHistories",
                column: "ChangedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ReviewerComment",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "AdjustedAt",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AdjustedBy",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AdjustmentReason",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInPhotoUrl",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutPhotoUrl",
                table: "Attendances");
        }
    }
}

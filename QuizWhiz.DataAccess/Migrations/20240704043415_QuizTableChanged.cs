using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class QuizTableChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ScheduleId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "OptionA",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionB",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionC",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionD",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "AnswerText",
                table: "Answers",
                newName: "OptionText");

            migrationBuilder.AlterColumn<int>(
                name: "WinningAmount",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "Quizzes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsAnswer",
                table: "Answers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OptionNo",
                table: "Answers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "IsAnswer",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "OptionNo",
                table: "Answers");

            migrationBuilder.RenameColumn(
                name: "OptionText",
                table: "Answers",
                newName: "AnswerText");

            migrationBuilder.AlterColumn<int>(
                name: "WinningAmount",
                table: "Quizzes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Quizzes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OptionA",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionB",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionC",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionD",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ScheduleId",
                table: "Quizzes",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes",
                column: "ScheduleId",
                principalTable: "QuizSchedules",
                principalColumn: "ScheduleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

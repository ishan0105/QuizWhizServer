using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangesinQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes",
                column: "ScheduleId",
                principalTable: "QuizSchedules",
                principalColumn: "ScheduleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleId",
                table: "Quizzes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizSchedules_ScheduleId",
                table: "Quizzes",
                column: "ScheduleId",
                principalTable: "QuizSchedules",
                principalColumn: "ScheduleId");
        }
    }
}

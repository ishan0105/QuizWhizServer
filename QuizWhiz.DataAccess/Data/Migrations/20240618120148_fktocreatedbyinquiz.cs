using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class fktocreatedbyinquiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_UserId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Quizzes",
                newName: "UsersUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_UserId",
                table: "Quizzes",
                newName: "IX_Quizzes_UsersUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_UsersUserId",
                table: "Quizzes",
                column: "UsersUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_UsersUserId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "UsersUserId",
                table: "Quizzes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_UsersUserId",
                table: "Quizzes",
                newName: "IX_Quizzes_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_UserId",
                table: "Quizzes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

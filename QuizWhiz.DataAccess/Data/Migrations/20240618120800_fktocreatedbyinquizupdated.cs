using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class fktocreatedbyinquizupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_UsersUserId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "UsersUserId",
                table: "Quizzes",
                newName: "ModifiedByUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_UsersUserId",
                table: "Quizzes",
                newName: "IX_Quizzes_ModifiedByUserUserId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserUserId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedByUserUserId",
                table: "Quizzes",
                column: "CreatedByUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedByUserUserId",
                table: "Quizzes",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_ModifiedByUserUserId",
                table: "Quizzes",
                column: "ModifiedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedByUserUserId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_ModifiedByUserUserId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_CreatedByUserUserId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "ModifiedByUserUserId",
                table: "Quizzes",
                newName: "UsersUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_ModifiedByUserUserId",
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
    }
}

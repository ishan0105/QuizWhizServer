using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ModifiedByUserUserId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserUserId",
                table: "Quizzes");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ModifiedBy",
                table: "Quizzes",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_ModifiedBy",
                table: "Quizzes",
                column: "ModifiedBy",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_ModifiedBy",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_CreatedBy",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ModifiedBy",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserUserId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedByUserUserId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedByUserUserId",
                table: "Quizzes",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ModifiedByUserUserId",
                table: "Quizzes",
                column: "ModifiedByUserUserId");

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
    }
}

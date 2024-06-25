using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DifficultyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "DifficultyId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuizDifficulties",
                columns: table => new
                {
                    DifficultyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DifficultyName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizDifficulties", x => x.DifficultyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_DifficultyId",
                table: "Quizzes",
                column: "DifficultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizDifficulties_DifficultyId",
                table: "Quizzes",
                column: "DifficultyId",
                principalTable: "QuizDifficulties",
                principalColumn: "DifficultyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizDifficulties_DifficultyId",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizDifficulties");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_DifficultyId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "DifficultyId",
                table: "Quizzes");

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Quizzes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

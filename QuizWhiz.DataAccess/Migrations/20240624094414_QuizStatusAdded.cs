using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class QuizStatusAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizStatus",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuizStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_StatusId",
                table: "Quizzes",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizStatuses_StatusId",
                table: "Quizzes",
                column: "StatusId",
                principalTable: "QuizStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizStatuses_StatusId",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_StatusId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Quizzes");

            migrationBuilder.AddColumn<string>(
                name: "QuizStatus",
                table: "Quizzes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class QuizParticipantsChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFiftyUsed",
                table: "QuizParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHeartUsed",
                table: "QuizParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipUsed",
                table: "QuizParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFiftyUsed",
                table: "QuizParticipants");

            migrationBuilder.DropColumn(
                name: "IsHeartUsed",
                table: "QuizParticipants");

            migrationBuilder.DropColumn(
                name: "IsSkipUsed",
                table: "QuizParticipants");
        }
    }
}

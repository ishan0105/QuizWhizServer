using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LifelineValueadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "Lifeline",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "Lifeline");
        }
    }
}

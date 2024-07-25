using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuizWhiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class usercoinsuserlifelinelifelinetables_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lifeline",
                columns: table => new
                {
                    LifelineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lifeline", x => x.LifelineId);
                });

            migrationBuilder.CreateTable(
                name: "UserCoins",
                columns: table => new
                {
                    UserCoinsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    NoOfCoins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoins", x => x.UserCoinsId);
                    table.ForeignKey(
                        name: "FK_UserCoins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLifeline",
                columns: table => new
                {
                    UserLifelineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LifelineId = table.Column<int>(type: "integer", nullable: false),
                    LifelineCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLifeline", x => x.UserLifelineId);
                    table.ForeignKey(
                        name: "FK_UserLifeline_Lifeline_LifelineId",
                        column: x => x.LifelineId,
                        principalTable: "Lifeline",
                        principalColumn: "LifelineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLifeline_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCoins_UserId",
                table: "UserCoins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLifeline_LifelineId",
                table: "UserLifeline",
                column: "LifelineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLifeline_UserId",
                table: "UserLifeline",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCoins");

            migrationBuilder.DropTable(
                name: "UserLifeline");

            migrationBuilder.DropTable(
                name: "Lifeline");
        }
    }
}

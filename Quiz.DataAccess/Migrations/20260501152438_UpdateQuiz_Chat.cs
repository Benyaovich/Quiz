using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuiz_Chat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Chat",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chat",
                table: "Quizzes");
        }
    }
}

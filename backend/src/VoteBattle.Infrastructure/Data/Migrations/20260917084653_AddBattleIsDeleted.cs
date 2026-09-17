using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoteBattle.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBattleIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Battles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Battles_IsDeleted",
                table: "Battles",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Battles_IsDeleted",
                table: "Battles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Battles");
        }
    }
}

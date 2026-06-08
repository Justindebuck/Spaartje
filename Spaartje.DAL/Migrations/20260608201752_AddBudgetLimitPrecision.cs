using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spaartje.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetLimitPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BudgetLimit",
                table: "Categories",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetLimit",
                table: "Categories");
        }
    }
}

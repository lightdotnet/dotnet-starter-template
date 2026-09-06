using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSQL.Approval
{
    /// <inheritdoc />
    public partial class AddApprovalRequestContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "approval",
                table: "ApprovalRequests",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                schema: "approval",
                table: "ApprovalRequests");
        }
    }
}

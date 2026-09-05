using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqlite.Organization
{
    /// <inheritdoc />
    public partial class AddMembershipAssignmentTypeAndIsManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignmentType",
                schema: "organization",
                table: "EmployeeOrgUnitMemberships",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                schema: "organization",
                table: "EmployeeOrgUnitMemberships",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentType",
                schema: "organization",
                table: "EmployeeOrgUnitMemberships");

            migrationBuilder.DropColumn(
                name: "IsManager",
                schema: "organization",
                table: "EmployeeOrgUnitMemberships");
        }
    }
}

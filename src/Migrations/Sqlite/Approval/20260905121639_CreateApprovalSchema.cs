using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqlite.Approval
{
    /// <inheritdoc />
    public partial class CreateApprovalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "approval");

            migrationBuilder.CreateTable(
                name: "ApprovalRequests",
                schema: "approval",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    RequestType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequestId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    RequesterUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    RequesterEmployeeId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    DeepLinkUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalizedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                schema: "approval",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ApprovalRequestId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    ApproverUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ApproverEmployeeId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DecidedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ApprovalRequests_ApprovalRequestId",
                        column: x => x.ApprovalRequestId,
                        principalSchema: "approval",
                        principalTable: "ApprovalRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_RequesterUserId",
                schema: "approval",
                table: "ApprovalRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_RequestType_RequestId",
                schema: "approval",
                table: "ApprovalRequests",
                columns: new[] { "RequestType", "RequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApprovalRequestId",
                schema: "approval",
                table: "ApprovalSteps",
                column: "ApprovalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApproverUserId",
                schema: "approval",
                table: "ApprovalSteps",
                column: "ApproverUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalSteps",
                schema: "approval");

            migrationBuilder.DropTable(
                name: "ApprovalRequests",
                schema: "approval");
        }
    }
}

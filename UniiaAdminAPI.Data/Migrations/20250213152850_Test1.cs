using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniiaAdmin.Data.Migrations
{
    /// <inheritdoc />
    public partial class Test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicationRoleClaims",
                columns: table => new
                {
                    PublicationId = table.Column<int>(type: "integer", nullable: false),
                    IdentityRoleClaimId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationRoleClaims", x => new { x.PublicationId, x.IdentityRoleClaimId });
                    table.ForeignKey(
                        name: "FK_PublicationRoleClaims_AspNetRoleClaims_IdentityRoleClaimId",
                        column: x => x.IdentityRoleClaimId,
                        principalTable: "AspNetRoleClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublicationRoleClaims_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationRoleClaims_IdentityRoleClaimId",
                table: "PublicationRoleClaims",
                column: "IdentityRoleClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationRoleClaims");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Network.API.Migrations
{
    public partial class AddedFriendEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    FrienderId = table.Column<int>(nullable: false),
                    FriendeeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => new { x.FrienderId, x.FriendeeId });
                    table.ForeignKey(
                        name: "FK_Friends_Users_FriendeeId",
                        column: x => x.FriendeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friends_Users_FrienderId",
                        column: x => x.FrienderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_FriendeeId",
                table: "Friends",
                column: "FriendeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends");
        }
    }
}

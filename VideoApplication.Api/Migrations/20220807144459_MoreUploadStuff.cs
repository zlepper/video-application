using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoApplication.Api.Migrations
{
    public partial class MoreUploadStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Videos",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ProcessedDuration",
                table: "Videos",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "ProcessingState",
                table: "Videos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VideoAudioTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAudioTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAudioTracks_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoVideoTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    FrameRate = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoVideoTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoVideoTracks_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAudioTracks_VideoId",
                table: "VideoAudioTracks",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoVideoTracks_VideoId",
                table: "VideoVideoTracks",
                column: "VideoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoAudioTracks");

            migrationBuilder.DropTable(
                name: "VideoVideoTracks");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ProcessedDuration",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ProcessingState",
                table: "Videos");
        }
    }
}

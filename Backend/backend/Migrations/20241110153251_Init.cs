using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    RepoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RepoName = table.Column<int>(type: "int", nullable: false),
                    Visibility = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.RepoId);
                    table.ForeignKey(
                        name: "FK_Repositories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    CommitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    CommitHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    ParentCommitId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.CommitId);
                    table.ForeignKey(
                        name: "FK_Commits_Commits_ParentCommitId",
                        column: x => x.ParentCommitId,
                        principalTable: "Commits",
                        principalColumn: "CommitId");
                    table.ForeignKey(
                        name: "FK_Commits_Repositories_RepoId",
                        column: x => x.RepoId,
                        principalTable: "Repositories",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commits_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommitId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RepositoryRepoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Commits_CommitId",
                        column: x => x.CommitId,
                        principalTable: "Commits",
                        principalColumn: "CommitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branches_Repositories_RepositoryRepoId",
                        column: x => x.RepositoryRepoId,
                        principalTable: "Repositories",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommitId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileBlob = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_Files_Commits_CommitId",
                        column: x => x.CommitId,
                        principalTable: "Commits",
                        principalColumn: "CommitId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CommitId",
                table: "Branches",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RepositoryRepoId",
                table: "Branches",
                column: "RepositoryRepoId");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_AuthorId",
                table: "Commits",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_ParentCommitId",
                table: "Commits",
                column: "ParentCommitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commits_RepoId",
                table: "Commits",
                column: "RepoId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CommitId",
                table: "Files",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_UserId",
                table: "Repositories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

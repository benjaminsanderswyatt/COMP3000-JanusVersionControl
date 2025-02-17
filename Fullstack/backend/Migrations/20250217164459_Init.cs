using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessTokenBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    Expires = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTokenBlacklists", x => x.Id);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    CommitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommitHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    BranchName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    TreeHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    AuthorName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    AuthorEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    Message = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    CommittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.CommitId);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(63)", maxLength: 63, nullable: false, collation: "utf8mb4_unicode_ci"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    PasswordHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_unicode_ci"),
                    Salt = table.Column<byte[]>(type: "longblob", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "CommitParents",
                columns: table => new
                {
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitParents", x => new { x.ChildId, x.ParentId });
                    table.ForeignKey(
                        name: "FK_CommitParents_Commits_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Commits",
                        principalColumn: "CommitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitParents_Commits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Commits",
                        principalColumn: "CommitId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    RepoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    RepoName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    RepoDescription = table.Column<string>(type: "varchar(511)", maxLength: 511, nullable: false, collation: "utf8mb4_unicode_ci"),
                    IsPrivate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.RepoId);
                    table.ForeignKey(
                        name: "FK_Repositories_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BranchName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_unicode_ci"),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    LatestCommitHash = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Repositories_RepoId",
                        column: x => x.RepoId,
                        principalTable: "Repositories",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "RepoAccess",
                columns: table => new
                {
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoAccess", x => new { x.RepoId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RepoAccess_Repositories_RepoId",
                        column: x => x.RepoId,
                        principalTable: "Repositories",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepoAccess_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.InsertData(
                table: "Commits",
                columns: new[] { "CommitId", "AuthorEmail", "AuthorName", "BranchName", "CommitHash", "CommittedAt", "Message", "TreeHash" },
                values: new object[,]
                {
                    { 1, "user@1.com", "User1", "main", "abcd1234efgh5678ijkl9012mnop3456qrst7890", new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(805), "Initial commit", "treehash1" },
                    { 2, "user@2.com", "User2", "branch", "mnop3456qrst7890abcd1234efgh5678ijkl9012", new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(807), "Setup project structure", "treehash2" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "PasswordHash", "ProfilePicturePath", "RefreshToken", "RefreshTokenExpiryTime", "Salt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(531), "user@1.com", "password", null, null, null, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "User1" },
                    { 2, new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(541), "user@2.com", "password", null, null, null, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "User2" }
                });

            migrationBuilder.InsertData(
                table: "CommitParents",
                columns: new[] { "ChildId", "ParentId" },
                values: new object[] { 2, 1 });

            migrationBuilder.InsertData(
                table: "Repositories",
                columns: new[] { "RepoId", "CreatedAt", "IsPrivate", "OwnerId", "RepoDescription", "RepoName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(664), false, 1, "First seeded", "Repo1" },
                    { 2, new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(668), true, 2, "Sec seeded", "Repo2" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchName", "CreatedAt", "LatestCommitHash", "RepoId" },
                values: new object[,]
                {
                    { 1, "main", new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(774), null, 1 },
                    { 2, "branch", new DateTime(2025, 2, 17, 16, 44, 58, 997, DateTimeKind.Utc).AddTicks(778), null, 1 }
                });

            migrationBuilder.InsertData(
                table: "RepoAccess",
                columns: new[] { "RepoId", "UserId", "AccessLevel" },
                values: new object[,]
                {
                    { 1, 1, 3 },
                    { 1, 2, 1 },
                    { 2, 2, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RepoId_BranchName",
                table: "Branches",
                columns: new[] { "RepoId", "BranchName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommitParents_ParentId",
                table: "CommitParents",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_CommitHash",
                table: "Commits",
                column: "CommitHash");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_CommittedAt",
                table: "Commits",
                column: "CommittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RepoAccess_UserId",
                table: "RepoAccess",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_OwnerId_RepoName",
                table: "Repositories",
                columns: new[] { "OwnerId", "RepoName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessTokenBlacklists");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "CommitParents");

            migrationBuilder.DropTable(
                name: "RepoAccess");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

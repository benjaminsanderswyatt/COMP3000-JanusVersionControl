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
            migrationBuilder.CreateTable(
                name: "AccessTokenBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    Expires = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    BlacklistedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTokenBlacklists", x => x.Id);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityName = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    Action = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    KeyValues = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    OldValues = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    NewValues = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci"),
                    RefreshTokenExpiryTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
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
                    SplitFromCommitHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true, collation: "utf8mb4_unicode_ci"),
                    LatestCommitHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true, collation: "utf8mb4_unicode_ci"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ParentBranch = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Branches_ParentBranch",
                        column: x => x.ParentBranch,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepoAccess_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "RepoInvites",
                columns: table => new
                {
                    InviteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    InviterUserId = table.Column<int>(type: "int", nullable: false),
                    InviteeUserId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoInvites", x => x.InviteId);
                    table.ForeignKey(
                        name: "FK_RepoInvites_Repositories_RepoId",
                        column: x => x.RepoId,
                        principalTable: "Repositories",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepoInvites_Users_InviteeUserId",
                        column: x => x.InviteeUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepoInvites_Users_InviterUserId",
                        column: x => x.InviterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    CommitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommitHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TreeHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    Message = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci"),
                    CommittedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.CommitId);
                    table.ForeignKey(
                        name: "FK_Commits_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ParentBranch",
                table: "Branches",
                column: "ParentBranch");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RepoId",
                table: "Branches",
                column: "RepoId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RepoId_BranchName",
                table: "Branches",
                columns: new[] { "RepoId", "BranchName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommitParents_ChildId",
                table: "CommitParents",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitParents_ParentId",
                table: "CommitParents",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_BranchId",
                table: "Commits",
                column: "BranchId");

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
                name: "IX_RepoInvites_InviteeUserId",
                table: "RepoInvites",
                column: "InviteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepoInvites_InviterUserId",
                table: "RepoInvites",
                column: "InviterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepoInvites_RepoId",
                table: "RepoInvites",
                column: "RepoId");

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
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CommitParents");

            migrationBuilder.DropTable(
                name: "RepoAccess");

            migrationBuilder.DropTable(
                name: "RepoInvites");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

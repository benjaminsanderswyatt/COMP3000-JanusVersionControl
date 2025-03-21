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
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "PasswordHash", "ProfilePicturePath", "RefreshToken", "RefreshTokenExpiryTime", "Salt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(4676), new TimeSpan(0, 0, 0, 0, 0)), "user@1.com", "9yFX7gZH4kzXPKv9cJCjeMHOmGAeu3ZN/xk38xqSs9M=", null, null, null, new byte[] { 162, 83, 152, 5, 53, 24, 84, 255, 223, 133, 139, 80, 66, 12, 68, 253 }, "User1" },
                    { 2, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(4731), new TimeSpan(0, 0, 0, 0, 0)), "user@2.com", "dPvEXf/wGCzCRsuhocgnj++alDjKzUHP+De3XxhrHYo=", null, null, null, new byte[] { 234, 67, 94, 49, 49, 35, 63, 199, 120, 112, 65, 180, 201, 164, 223, 150 }, "User2" }
                });

            migrationBuilder.InsertData(
                table: "Repositories",
                columns: new[] { "RepoId", "CreatedAt", "IsPrivate", "OwnerId", "RepoDescription", "RepoName" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5114), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "First seeded", "Repo1" },
                    { 2, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5129), new TimeSpan(0, 0, 0, 0, 0)), true, 2, "Sec seeded", "Repo2" },
                    { 3, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5131), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "This repo has many commits", "RepoWithManyCommits" },
                    { 4, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5197), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 4", "Repo4" },
                    { 5, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5281), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 5", "Repo5" },
                    { 6, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5336), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 6", "Repo6" },
                    { 7, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5361), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 7", "Repo7" },
                    { 8, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5393), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 8", "Repo8" },
                    { 9, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5428), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 9", "Repo9" },
                    { 10, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5450), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 10", "Repo10" },
                    { 11, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5526), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 11", "Repo11" },
                    { 12, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5559), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 12", "Repo12" },
                    { 13, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5580), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 13", "Repo13" },
                    { 14, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5598), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 14", "Repo14" },
                    { 15, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5619), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 15", "Repo15" },
                    { 16, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5715), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 16", "Repo16" },
                    { 17, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5787), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 17", "Repo17" },
                    { 18, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5808), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 18", "Repo18" },
                    { 19, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5862), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 19", "Repo19" },
                    { 20, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5883), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 20", "Repo20" },
                    { 21, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5901), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 21", "Repo21" },
                    { 22, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5919), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 22", "Repo22" },
                    { 23, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5939), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 23", "Repo23" },
                    { 24, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5957), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 24", "Repo24" },
                    { 25, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5974), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 25", "Repo25" },
                    { 26, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(5993), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 26", "Repo26" },
                    { 27, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6011), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 27", "Repo27" },
                    { 28, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6054), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 28", "Repo28" },
                    { 29, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6079), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 29", "Repo29" },
                    { 30, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6098), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 30", "Repo30" },
                    { 31, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6211), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 31", "Repo31" },
                    { 32, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6270), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 32", "Repo32" },
                    { 33, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6323), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 33", "Repo33" },
                    { 34, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6343), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 34", "Repo34" },
                    { 35, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6362), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 35", "Repo35" },
                    { 36, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6381), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 36", "Repo36" },
                    { 37, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6398), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 37", "Repo37" },
                    { 38, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6416), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 38", "Repo38" },
                    { 39, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6433), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 39", "Repo39" },
                    { 40, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6452), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 40", "Repo40" },
                    { 41, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6469), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 41", "Repo41" },
                    { 42, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6488), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 42", "Repo42" },
                    { 43, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6506), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 43", "Repo43" },
                    { 44, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6526), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 44", "Repo44" },
                    { 45, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6545), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 45", "Repo45" },
                    { 46, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6564), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 46", "Repo46" },
                    { 47, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6583), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 47", "Repo47" },
                    { 48, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6628), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 48", "Repo48" },
                    { 49, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6648), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 49", "Repo49" },
                    { 50, new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(6667), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 50", "Repo50" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchName", "CreatedAt", "CreatedBy", "LatestCommitHash", "ParentBranch", "RepoId", "SplitFromCommitHash" },
                values: new object[,]
                {
                    { 1, "main", new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(7451), new TimeSpan(0, 0, 0, 0, 0)), 1, "925cc242245c8df69d12021001277c54ec4b321c", null, 1, null },
                    { 3, "main", new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(7457), new TimeSpan(0, 0, 0, 0, 0)), 1, "branch3_commit_100", null, 3, null }
                });

            migrationBuilder.InsertData(
                table: "RepoAccess",
                columns: new[] { "RepoId", "UserId", "AccessLevel" },
                values: new object[,]
                {
                    { 1, 1, 3 },
                    { 1, 2, 1 },
                    { 2, 2, 3 },
                    { 3, 1, 3 },
                    { 4, 1, 3 },
                    { 5, 1, 3 },
                    { 6, 1, 3 },
                    { 7, 1, 3 },
                    { 8, 1, 3 },
                    { 9, 1, 3 },
                    { 10, 1, 3 },
                    { 11, 1, 3 },
                    { 12, 1, 3 },
                    { 13, 1, 3 },
                    { 14, 1, 3 },
                    { 15, 1, 3 },
                    { 16, 1, 3 },
                    { 17, 1, 3 },
                    { 18, 1, 3 },
                    { 19, 1, 3 },
                    { 20, 1, 3 },
                    { 21, 1, 3 },
                    { 22, 1, 3 },
                    { 23, 1, 3 },
                    { 24, 1, 3 },
                    { 25, 1, 3 },
                    { 26, 1, 3 },
                    { 27, 1, 3 },
                    { 28, 1, 3 },
                    { 29, 1, 3 },
                    { 30, 1, 3 },
                    { 31, 1, 3 },
                    { 32, 1, 3 },
                    { 33, 1, 3 },
                    { 34, 1, 3 },
                    { 35, 1, 3 },
                    { 36, 1, 3 },
                    { 37, 1, 3 },
                    { 38, 1, 3 },
                    { 39, 1, 3 },
                    { 40, 1, 3 },
                    { 41, 1, 3 },
                    { 42, 1, 3 },
                    { 43, 1, 3 },
                    { 44, 1, 3 },
                    { 45, 1, 3 },
                    { 46, 1, 3 },
                    { 47, 1, 3 },
                    { 48, 1, 3 },
                    { 49, 1, 3 },
                    { 50, 1, 3 }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchName", "CreatedAt", "CreatedBy", "LatestCommitHash", "ParentBranch", "RepoId", "SplitFromCommitHash" },
                values: new object[] { 2, "branch", new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 978, DateTimeKind.Unspecified).AddTicks(7455), new TimeSpan(0, 0, 0, 0, 0)), 1, "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", 1, 1, "925cc242245c8df69d12021001277c54ec4b321c" });

            migrationBuilder.InsertData(
                table: "Commits",
                columns: new[] { "CommitId", "BranchId", "CommitHash", "CommittedAt", "CreatedBy", "Message", "TreeHash" },
                values: new object[,]
                {
                    { 1, 1, "925cc242245c8df69d12021001277c54ec4b321c", new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 979, DateTimeKind.Unspecified).AddTicks(6255), new TimeSpan(0, 0, 0, 0, 0)), "Janus", "Initial commit", "" },
                    { 3, 3, "branch3_initial_commit_hash", new DateTimeOffset(new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Initial commit for branch 3", "" },
                    { 4, 3, "branch3_commit_1", new DateTimeOffset(new DateTime(2025, 3, 1, 0, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 1 for branch 3", "" },
                    { 5, 3, "branch3_commit_2", new DateTimeOffset(new DateTime(2025, 3, 3, 0, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 2 for branch 3", "" },
                    { 6, 3, "branch3_commit_3", new DateTimeOffset(new DateTime(2025, 3, 6, 0, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 3 for branch 3", "" },
                    { 7, 3, "branch3_commit_4", new DateTimeOffset(new DateTime(2025, 3, 6, 1, 37, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 4 for branch 3", "" },
                    { 8, 3, "branch3_commit_5", new DateTimeOffset(new DateTime(2025, 3, 6, 2, 38, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 5 for branch 3", "" },
                    { 9, 3, "branch3_commit_6", new DateTimeOffset(new DateTime(2025, 3, 6, 3, 25, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 6 for branch 3", "" },
                    { 10, 3, "branch3_commit_7", new DateTimeOffset(new DateTime(2025, 3, 6, 3, 28, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 7 for branch 3", "" },
                    { 11, 3, "branch3_commit_8", new DateTimeOffset(new DateTime(2025, 3, 6, 4, 57, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 8 for branch 3", "" },
                    { 12, 3, "branch3_commit_9", new DateTimeOffset(new DateTime(2025, 3, 6, 6, 43, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 9 for branch 3", "" },
                    { 13, 3, "branch3_commit_10", new DateTimeOffset(new DateTime(2025, 3, 8, 6, 43, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 10 for branch 3", "" },
                    { 14, 3, "branch3_commit_11", new DateTimeOffset(new DateTime(2025, 3, 8, 7, 47, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 11 for branch 3", "" },
                    { 15, 3, "branch3_commit_12", new DateTimeOffset(new DateTime(2025, 3, 8, 9, 12, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 12 for branch 3", "" },
                    { 16, 3, "branch3_commit_13", new DateTimeOffset(new DateTime(2025, 3, 8, 10, 26, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 13 for branch 3", "" },
                    { 17, 3, "branch3_commit_14", new DateTimeOffset(new DateTime(2025, 3, 11, 10, 26, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 14 for branch 3", "" },
                    { 18, 3, "branch3_commit_15", new DateTimeOffset(new DateTime(2025, 3, 11, 10, 56, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 15 for branch 3", "" },
                    { 19, 3, "branch3_commit_16", new DateTimeOffset(new DateTime(2025, 3, 11, 11, 22, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 16 for branch 3", "" },
                    { 20, 3, "branch3_commit_17", new DateTimeOffset(new DateTime(2025, 3, 11, 12, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 17 for branch 3", "" },
                    { 21, 3, "branch3_commit_18", new DateTimeOffset(new DateTime(2025, 3, 11, 13, 52, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 18 for branch 3", "" },
                    { 22, 3, "branch3_commit_19", new DateTimeOffset(new DateTime(2025, 3, 11, 14, 50, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 19 for branch 3", "" },
                    { 23, 3, "branch3_commit_20", new DateTimeOffset(new DateTime(2025, 3, 11, 15, 48, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 20 for branch 3", "" },
                    { 24, 3, "branch3_commit_21", new DateTimeOffset(new DateTime(2025, 3, 13, 15, 48, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 21 for branch 3", "" },
                    { 25, 3, "branch3_commit_22", new DateTimeOffset(new DateTime(2025, 3, 13, 16, 22, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 22 for branch 3", "" },
                    { 26, 3, "branch3_commit_23", new DateTimeOffset(new DateTime(2025, 3, 13, 16, 58, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 23 for branch 3", "" },
                    { 27, 3, "branch3_commit_24", new DateTimeOffset(new DateTime(2025, 3, 13, 18, 40, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 24 for branch 3", "" },
                    { 28, 3, "branch3_commit_25", new DateTimeOffset(new DateTime(2025, 3, 13, 20, 22, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 25 for branch 3", "" },
                    { 29, 3, "branch3_commit_26", new DateTimeOffset(new DateTime(2025, 3, 13, 21, 36, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 26 for branch 3", "" },
                    { 30, 3, "branch3_commit_27", new DateTimeOffset(new DateTime(2025, 3, 13, 22, 36, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 27 for branch 3", "" },
                    { 31, 3, "branch3_commit_28", new DateTimeOffset(new DateTime(2025, 3, 13, 23, 40, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 28 for branch 3", "" },
                    { 32, 3, "branch3_commit_29", new DateTimeOffset(new DateTime(2025, 3, 14, 23, 40, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 29 for branch 3", "" },
                    { 33, 3, "branch3_commit_30", new DateTimeOffset(new DateTime(2025, 3, 15, 23, 40, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 30 for branch 3", "" },
                    { 34, 3, "branch3_commit_31", new DateTimeOffset(new DateTime(2025, 3, 16, 0, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 31 for branch 3", "" },
                    { 35, 3, "branch3_commit_32", new DateTimeOffset(new DateTime(2025, 3, 16, 0, 32, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 32 for branch 3", "" },
                    { 36, 3, "branch3_commit_33", new DateTimeOffset(new DateTime(2025, 3, 17, 0, 32, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 33 for branch 3", "" },
                    { 37, 3, "branch3_commit_34", new DateTimeOffset(new DateTime(2025, 3, 20, 0, 32, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 34 for branch 3", "" },
                    { 38, 3, "branch3_commit_35", new DateTimeOffset(new DateTime(2025, 3, 22, 0, 32, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 35 for branch 3", "" },
                    { 39, 3, "branch3_commit_36", new DateTimeOffset(new DateTime(2025, 3, 25, 0, 32, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 36 for branch 3", "" },
                    { 40, 3, "branch3_commit_37", new DateTimeOffset(new DateTime(2025, 3, 25, 1, 17, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 37 for branch 3", "" },
                    { 41, 3, "branch3_commit_38", new DateTimeOffset(new DateTime(2025, 3, 25, 2, 35, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 38 for branch 3", "" },
                    { 42, 3, "branch3_commit_39", new DateTimeOffset(new DateTime(2025, 3, 25, 2, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 39 for branch 3", "" },
                    { 43, 3, "branch3_commit_40", new DateTimeOffset(new DateTime(2025, 3, 25, 3, 19, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 40 for branch 3", "" },
                    { 44, 3, "branch3_commit_41", new DateTimeOffset(new DateTime(2025, 3, 25, 5, 4, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 41 for branch 3", "" },
                    { 45, 3, "branch3_commit_42", new DateTimeOffset(new DateTime(2025, 3, 25, 6, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 42 for branch 3", "" },
                    { 46, 3, "branch3_commit_43", new DateTimeOffset(new DateTime(2025, 3, 25, 6, 18, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 43 for branch 3", "" },
                    { 47, 3, "branch3_commit_44", new DateTimeOffset(new DateTime(2025, 3, 26, 6, 18, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 44 for branch 3", "" },
                    { 48, 3, "branch3_commit_45", new DateTimeOffset(new DateTime(2025, 3, 26, 7, 44, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 45 for branch 3", "" },
                    { 49, 3, "branch3_commit_46", new DateTimeOffset(new DateTime(2025, 3, 26, 9, 36, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 46 for branch 3", "" },
                    { 50, 3, "branch3_commit_47", new DateTimeOffset(new DateTime(2025, 3, 26, 10, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 47 for branch 3", "" },
                    { 51, 3, "branch3_commit_48", new DateTimeOffset(new DateTime(2025, 3, 26, 10, 44, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 48 for branch 3", "" },
                    { 52, 3, "branch3_commit_49", new DateTimeOffset(new DateTime(2025, 3, 26, 11, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 49 for branch 3", "" },
                    { 53, 3, "branch3_commit_50", new DateTimeOffset(new DateTime(2025, 3, 26, 12, 29, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 50 for branch 3", "" },
                    { 54, 3, "branch3_commit_51", new DateTimeOffset(new DateTime(2025, 3, 26, 13, 46, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 51 for branch 3", "" },
                    { 55, 3, "branch3_commit_52", new DateTimeOffset(new DateTime(2025, 3, 26, 15, 23, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 52 for branch 3", "" },
                    { 56, 3, "branch3_commit_53", new DateTimeOffset(new DateTime(2025, 3, 26, 17, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 53 for branch 3", "" },
                    { 57, 3, "branch3_commit_54", new DateTimeOffset(new DateTime(2025, 3, 26, 17, 23, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 54 for branch 3", "" },
                    { 58, 3, "branch3_commit_55", new DateTimeOffset(new DateTime(2025, 3, 28, 17, 23, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 55 for branch 3", "" },
                    { 59, 3, "branch3_commit_56", new DateTimeOffset(new DateTime(2025, 3, 28, 18, 39, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 56 for branch 3", "" },
                    { 60, 3, "branch3_commit_57", new DateTimeOffset(new DateTime(2025, 3, 30, 18, 39, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 57 for branch 3", "" },
                    { 61, 3, "branch3_commit_58", new DateTimeOffset(new DateTime(2025, 3, 31, 18, 39, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 58 for branch 3", "" },
                    { 62, 3, "branch3_commit_59", new DateTimeOffset(new DateTime(2025, 4, 3, 18, 39, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 59 for branch 3", "" },
                    { 63, 3, "branch3_commit_60", new DateTimeOffset(new DateTime(2025, 4, 3, 20, 9, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 60 for branch 3", "" },
                    { 64, 3, "branch3_commit_61", new DateTimeOffset(new DateTime(2025, 4, 3, 21, 51, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 61 for branch 3", "" },
                    { 65, 3, "branch3_commit_62", new DateTimeOffset(new DateTime(2025, 4, 3, 21, 58, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 62 for branch 3", "" },
                    { 66, 3, "branch3_commit_63", new DateTimeOffset(new DateTime(2025, 4, 3, 23, 34, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 63 for branch 3", "" },
                    { 67, 3, "branch3_commit_64", new DateTimeOffset(new DateTime(2025, 4, 3, 23, 54, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 64 for branch 3", "" },
                    { 68, 3, "branch3_commit_65", new DateTimeOffset(new DateTime(2025, 4, 5, 23, 54, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 65 for branch 3", "" },
                    { 69, 3, "branch3_commit_66", new DateTimeOffset(new DateTime(2025, 4, 5, 23, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 66 for branch 3", "" },
                    { 70, 3, "branch3_commit_67", new DateTimeOffset(new DateTime(2025, 4, 6, 0, 50, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 67 for branch 3", "" },
                    { 71, 3, "branch3_commit_68", new DateTimeOffset(new DateTime(2025, 4, 8, 0, 50, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 68 for branch 3", "" },
                    { 72, 3, "branch3_commit_69", new DateTimeOffset(new DateTime(2025, 4, 11, 0, 50, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 69 for branch 3", "" },
                    { 73, 3, "branch3_commit_70", new DateTimeOffset(new DateTime(2025, 4, 11, 2, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 70 for branch 3", "" },
                    { 74, 3, "branch3_commit_71", new DateTimeOffset(new DateTime(2025, 4, 14, 2, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 71 for branch 3", "" },
                    { 75, 3, "branch3_commit_72", new DateTimeOffset(new DateTime(2025, 4, 14, 2, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 72 for branch 3", "" },
                    { 76, 3, "branch3_commit_73", new DateTimeOffset(new DateTime(2025, 4, 14, 3, 11, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 73 for branch 3", "" },
                    { 77, 3, "branch3_commit_74", new DateTimeOffset(new DateTime(2025, 4, 17, 3, 11, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 74 for branch 3", "" },
                    { 78, 3, "branch3_commit_75", new DateTimeOffset(new DateTime(2025, 4, 17, 3, 33, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 75 for branch 3", "" },
                    { 79, 3, "branch3_commit_76", new DateTimeOffset(new DateTime(2025, 4, 17, 4, 11, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 76 for branch 3", "" },
                    { 80, 3, "branch3_commit_77", new DateTimeOffset(new DateTime(2025, 4, 20, 4, 11, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 77 for branch 3", "" },
                    { 81, 3, "branch3_commit_78", new DateTimeOffset(new DateTime(2025, 4, 20, 5, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 78 for branch 3", "" },
                    { 82, 3, "branch3_commit_79", new DateTimeOffset(new DateTime(2025, 4, 20, 5, 24, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 79 for branch 3", "" },
                    { 83, 3, "branch3_commit_80", new DateTimeOffset(new DateTime(2025, 4, 20, 6, 2, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 80 for branch 3", "" },
                    { 84, 3, "branch3_commit_81", new DateTimeOffset(new DateTime(2025, 4, 21, 6, 2, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 81 for branch 3", "" },
                    { 85, 3, "branch3_commit_82", new DateTimeOffset(new DateTime(2025, 4, 21, 7, 1, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 82 for branch 3", "" },
                    { 86, 3, "branch3_commit_83", new DateTimeOffset(new DateTime(2025, 4, 22, 7, 1, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 83 for branch 3", "" },
                    { 87, 3, "branch3_commit_84", new DateTimeOffset(new DateTime(2025, 4, 22, 8, 6, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 84 for branch 3", "" },
                    { 88, 3, "branch3_commit_85", new DateTimeOffset(new DateTime(2025, 4, 23, 8, 6, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 85 for branch 3", "" },
                    { 89, 3, "branch3_commit_86", new DateTimeOffset(new DateTime(2025, 4, 23, 8, 17, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 86 for branch 3", "" },
                    { 90, 3, "branch3_commit_87", new DateTimeOffset(new DateTime(2025, 4, 23, 9, 46, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 87 for branch 3", "" },
                    { 91, 3, "branch3_commit_88", new DateTimeOffset(new DateTime(2025, 4, 25, 9, 46, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 88 for branch 3", "" },
                    { 92, 3, "branch3_commit_89", new DateTimeOffset(new DateTime(2025, 4, 25, 10, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 89 for branch 3", "" },
                    { 93, 3, "branch3_commit_90", new DateTimeOffset(new DateTime(2025, 4, 25, 11, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 90 for branch 3", "" },
                    { 94, 3, "branch3_commit_91", new DateTimeOffset(new DateTime(2025, 4, 26, 11, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 91 for branch 3", "" },
                    { 95, 3, "branch3_commit_92", new DateTimeOffset(new DateTime(2025, 4, 29, 11, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 92 for branch 3", "" },
                    { 96, 3, "branch3_commit_93", new DateTimeOffset(new DateTime(2025, 4, 30, 11, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 93 for branch 3", "" },
                    { 97, 3, "branch3_commit_94", new DateTimeOffset(new DateTime(2025, 4, 30, 13, 42, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 94 for branch 3", "" },
                    { 98, 3, "branch3_commit_95", new DateTimeOffset(new DateTime(2025, 4, 30, 14, 16, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 95 for branch 3", "" },
                    { 99, 3, "branch3_commit_96", new DateTimeOffset(new DateTime(2025, 4, 30, 15, 47, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 96 for branch 3", "" },
                    { 100, 3, "branch3_commit_97", new DateTimeOffset(new DateTime(2025, 4, 30, 16, 2, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 97 for branch 3", "" },
                    { 101, 3, "branch3_commit_98", new DateTimeOffset(new DateTime(2025, 5, 3, 16, 2, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 98 for branch 3", "" },
                    { 102, 3, "branch3_commit_99", new DateTimeOffset(new DateTime(2025, 5, 3, 17, 23, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 99 for branch 3", "" },
                    { 103, 3, "branch3_commit_100", new DateTimeOffset(new DateTime(2025, 5, 3, 17, 43, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User1", "Commit 100 for branch 3", "" }
                });

            migrationBuilder.InsertData(
                table: "CommitParents",
                columns: new[] { "ChildId", "ParentId" },
                values: new object[,]
                {
                    { 4, 3 },
                    { 5, 4 },
                    { 6, 5 },
                    { 7, 6 },
                    { 8, 7 },
                    { 9, 8 },
                    { 10, 9 },
                    { 11, 10 },
                    { 12, 11 },
                    { 13, 12 },
                    { 14, 13 },
                    { 15, 14 },
                    { 16, 15 },
                    { 17, 16 },
                    { 18, 17 },
                    { 19, 18 },
                    { 20, 19 },
                    { 21, 20 },
                    { 22, 21 },
                    { 23, 22 },
                    { 24, 23 },
                    { 25, 24 },
                    { 26, 25 },
                    { 27, 26 },
                    { 28, 27 },
                    { 29, 28 },
                    { 30, 29 },
                    { 31, 30 },
                    { 32, 31 },
                    { 33, 32 },
                    { 34, 33 },
                    { 35, 34 },
                    { 36, 35 },
                    { 37, 36 },
                    { 38, 37 },
                    { 39, 38 },
                    { 40, 39 },
                    { 41, 40 },
                    { 42, 41 },
                    { 43, 42 },
                    { 44, 43 },
                    { 45, 44 },
                    { 46, 45 },
                    { 47, 46 },
                    { 48, 47 },
                    { 49, 48 },
                    { 50, 49 },
                    { 51, 50 },
                    { 52, 51 },
                    { 53, 52 },
                    { 54, 53 },
                    { 55, 54 },
                    { 56, 55 },
                    { 57, 56 },
                    { 58, 57 },
                    { 59, 58 },
                    { 60, 59 },
                    { 61, 60 },
                    { 62, 61 },
                    { 63, 62 },
                    { 64, 63 },
                    { 65, 64 },
                    { 66, 65 },
                    { 67, 66 },
                    { 68, 67 },
                    { 69, 68 },
                    { 70, 69 },
                    { 71, 70 },
                    { 72, 71 },
                    { 73, 72 },
                    { 74, 73 },
                    { 75, 74 },
                    { 76, 75 },
                    { 77, 76 },
                    { 78, 77 },
                    { 79, 78 },
                    { 80, 79 },
                    { 81, 80 },
                    { 82, 81 },
                    { 83, 82 },
                    { 84, 83 },
                    { 85, 84 },
                    { 86, 85 },
                    { 87, 86 },
                    { 88, 87 },
                    { 89, 88 },
                    { 90, 89 },
                    { 91, 90 },
                    { 92, 91 },
                    { 93, 92 },
                    { 94, 93 },
                    { 95, 94 },
                    { 96, 95 },
                    { 97, 96 },
                    { 98, 97 },
                    { 99, 98 },
                    { 100, 99 },
                    { 101, 100 },
                    { 102, 101 },
                    { 103, 102 }
                });

            migrationBuilder.InsertData(
                table: "Commits",
                columns: new[] { "CommitId", "BranchId", "CommitHash", "CommittedAt", "CreatedBy", "Message", "TreeHash" },
                values: new object[] { 2, 2, "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", new DateTimeOffset(new DateTime(2025, 3, 21, 14, 28, 50, 979, DateTimeKind.Unspecified).AddTicks(6257), new TimeSpan(0, 0, 0, 0, 0)), "User2", "Next commit", "517e4c52e1020d3bc9901cb81093943d4919b55c" });

            migrationBuilder.InsertData(
                table: "CommitParents",
                columns: new[] { "ChildId", "ParentId" },
                values: new object[] { 2, 1 });

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
                name: "IX_RepoInvites_Status",
                table: "RepoInvites",
                column: "Status");

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

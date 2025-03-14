﻿using System;
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
                        principalColumn: "BranchId");
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

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    CommitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommitHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TreeHash = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, collation: "utf8mb4_unicode_ci"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
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
                    { 1, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(8650), new TimeSpan(0, 0, 0, 0, 0)), "user@1.com", "DY0z4fhmIiYfbIgSgj5Jhbj5YyxpuCO47pmaDr8pCfA=", null, null, null, new byte[] { 77, 55, 171, 17, 123, 4, 22, 57, 106, 17, 110, 152, 186, 50, 73, 245 }, "User1" },
                    { 2, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(8690), new TimeSpan(0, 0, 0, 0, 0)), "user@2.com", "qGEO64vosatHLY1wWhYWSHOdQnFfByselNb3EfwDcyU=", null, null, null, new byte[] { 97, 194, 9, 18, 142, 205, 26, 52, 110, 196, 135, 196, 199, 182, 197, 181 }, "User2" }
                });

            migrationBuilder.InsertData(
                table: "Repositories",
                columns: new[] { "RepoId", "CreatedAt", "IsPrivate", "OwnerId", "RepoDescription", "RepoName" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9063), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "First seeded", "Repo1" },
                    { 2, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9077), new TimeSpan(0, 0, 0, 0, 0)), true, 2, "Sec seeded", "Repo2" },
                    { 3, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9128), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 3", "Repo3" },
                    { 4, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9199), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 4", "Repo4" },
                    { 5, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9318), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 5", "Repo5" },
                    { 6, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9346), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 6", "Repo6" },
                    { 7, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9369), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 7", "Repo7" },
                    { 8, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9394), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 8", "Repo8" },
                    { 9, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9415), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 9", "Repo9" },
                    { 10, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9460), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 10", "Repo10" },
                    { 11, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9493), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 11", "Repo11" },
                    { 12, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9510), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 12", "Repo12" },
                    { 13, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9532), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 13", "Repo13" },
                    { 14, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9550), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 14", "Repo14" },
                    { 15, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9624), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 15", "Repo15" },
                    { 16, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9657), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 16", "Repo16" },
                    { 17, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9675), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 17", "Repo17" },
                    { 18, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9698), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 18", "Repo18" },
                    { 19, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9716), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 19", "Repo19" },
                    { 20, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9734), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 20", "Repo20" },
                    { 21, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9752), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 21", "Repo21" },
                    { 22, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9802), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 22", "Repo22" },
                    { 23, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9819), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 23", "Repo23" },
                    { 24, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9837), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 24", "Repo24" },
                    { 25, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9853), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 25", "Repo25" },
                    { 26, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9871), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 26", "Repo26" },
                    { 27, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9887), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 27", "Repo27" },
                    { 28, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9915), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 28", "Repo28" },
                    { 29, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 543, DateTimeKind.Unspecified).AddTicks(9936), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 29", "Repo29" },
                    { 30, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(40), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 30", "Repo30" },
                    { 31, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(90), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 31", "Repo31" },
                    { 32, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(109), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 32", "Repo32" },
                    { 33, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(126), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 33", "Repo33" },
                    { 34, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(146), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 34", "Repo34" },
                    { 35, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(164), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 35", "Repo35" },
                    { 36, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(181), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 36", "Repo36" },
                    { 37, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(198), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 37", "Repo37" },
                    { 38, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(215), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 38", "Repo38" },
                    { 39, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(259), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 39", "Repo39" },
                    { 40, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(277), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 40", "Repo40" },
                    { 41, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(295), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 41", "Repo41" },
                    { 42, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(312), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 42", "Repo42" },
                    { 43, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(329), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 43", "Repo43" },
                    { 44, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(347), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 44", "Repo44" },
                    { 45, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(365), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 45", "Repo45" },
                    { 46, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(382), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 46", "Repo46" },
                    { 47, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(399), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 47", "Repo47" },
                    { 48, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(416), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 48", "Repo48" },
                    { 49, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(434), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 49", "Repo49" },
                    { 50, new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(452), new TimeSpan(0, 0, 0, 0, 0)), false, 1, "Seeded 50", "Repo50" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "BranchId", "BranchName", "CreatedAt", "CreatedBy", "LatestCommitHash", "ParentBranch", "RepoId", "SplitFromCommitHash" },
                values: new object[] { 1, "main", new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(1510), new TimeSpan(0, 0, 0, 0, 0)), 1, "925cc242245c8df69d12021001277c54ec4b321c", null, 1, null });

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
                values: new object[] { 2, "branch", new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(1512), new TimeSpan(0, 0, 0, 0, 0)), 1, "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", 1, 1, "925cc242245c8df69d12021001277c54ec4b321c" });

            migrationBuilder.InsertData(
                table: "Commits",
                columns: new[] { "CommitId", "BranchId", "CommitHash", "CommittedAt", "CreatedBy", "Message", "TreeHash" },
                values: new object[,]
                {
                    { 1, 1, "925cc242245c8df69d12021001277c54ec4b321c", new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(1557), new TimeSpan(0, 0, 0, 0, 0)), 0, "Initial commit", "" },
                    { 2, 2, "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", new DateTimeOffset(new DateTime(2025, 3, 10, 19, 33, 54, 544, DateTimeKind.Unspecified).AddTicks(1560), new TimeSpan(0, 0, 0, 0, 0)), 2, "Next commit", "517e4c52e1020d3bc9901cb81093943d4919b55c" }
                });

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
                name: "CommitParents");

            migrationBuilder.DropTable(
                name: "RepoAccess");

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

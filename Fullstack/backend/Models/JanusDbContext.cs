using backend.Auth;
using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public class JanusDbContext : DbContext
    {
        public JanusDbContext(DbContextOptions<JanusDbContext> options) : base(options) { }

        public DbSet<AccessTokenBlacklist> AccessTokenBlacklists { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<RepoAccess> RepoAccess { get; set; }
        public DbSet<RepoInvite> RepoInvites { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Commit> Commits { get; set; }
        public DbSet<CommitParent> CommitParents { get; set; }


        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Case insensitve
            modelBuilder.UseCollation("utf8mb4_unicode_ci");


            // Unique constraints & Indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();



            modelBuilder.Entity<RepoAccess>()
                .HasIndex(ra => ra.UserId);



            modelBuilder.Entity<Repository>()
                .HasIndex(r => new { r.OwnerId, r.RepoName })
                .IsUnique();


            modelBuilder.Entity<RepoInvite>(entity =>
            {
                entity.HasIndex(ri => ri.InviteeUserId);
                entity.HasIndex(ri => ri.Status);
                entity.HasIndex(ri => ri.RepoId);
            });



            modelBuilder.Entity<Branch>()
                .HasIndex(b => new { b.RepoId, b.BranchName })
                .IsUnique();

            modelBuilder.Entity<Branch>()
                .HasIndex(b => b.RepoId);



            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.CommitHash);

            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.CommittedAt);

            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.BranchId);



            modelBuilder.Entity<CommitParent>()
                .HasIndex(cp => cp.ChildId);







            // Composite PK
            modelBuilder.Entity<CommitParent>()
                .HasKey(cp => new { cp.ChildId, cp.ParentId });

            modelBuilder.Entity<RepoAccess>()
                .HasKey(ra => new { ra.RepoId, ra.UserId });



            // Delete cascade
            modelBuilder.Entity<CommitParent>()
                .HasOne(cp => cp.Child)
                .WithMany(c => c.Parents)
                .HasForeignKey(cp => cp.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepoAccess>()
                .HasOne(ra => ra.Repository)
                .WithMany(r => r.RepoAccesses)
                .HasForeignKey(ra => ra.RepoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Parent)
                .WithMany()
                .HasForeignKey(b => b.ParentBranch)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<RepoInvite>()
                    .HasOne(ri => ri.Repository)
                    .WithMany(r => r.RepoInvites)
                    .HasForeignKey(ri => ri.RepoId)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepoInvite>()
                .HasOne(ri => ri.Inviter)
                .WithMany(u => u.SentInvites)
                .HasForeignKey(ri => ri.InviterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepoInvite>()
                .HasOne(ri => ri.Invitee)
                .WithMany(u => u.ReceivedInvites)
                .HasForeignKey(ri => ri.InviteeUserId)
                .OnDelete(DeleteBehavior.Cascade);








            // ---------- Seed Data ----------

            // Generate hashes correctly
            var user1Salt = PasswordSecurity.GenerateSalt();
            var user1HashedPassword = PasswordSecurity.ComputeHash("password", user1Salt);

            var user2Salt = PasswordSecurity.GenerateSalt();
            var user2HashedPassword = PasswordSecurity.ComputeHash("password", user2Salt);



            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "User1", Email = "user@1.com", PasswordHash = user1HashedPassword, Salt = user1Salt },
                new User { UserId = 2, Username = "User2", Email = "user@2.com", PasswordHash = user2HashedPassword, Salt = user2Salt }
            );


            // Seed Repos
            modelBuilder.Entity<Repository>().HasData(
                new Repository { RepoId = 1, OwnerId = 1, RepoName = "Repo1", RepoDescription = "First seeded", IsPrivate = false },
                new Repository { RepoId = 2, OwnerId = 2, RepoName = "Repo2", RepoDescription = "Sec seeded", IsPrivate = true },
                new Repository { RepoId = 3, OwnerId = 1, RepoName = "RepoWithManyCommits", RepoDescription = "This repo has many commits", IsPrivate = false }
            );

            // Seed many repositories
            for (int i = 4; i <= 50; i++)
            {
                modelBuilder.Entity<Repository>().HasData(
                    new Repository { RepoId = i, OwnerId = 1, RepoName = $"Repo{i}", RepoDescription = $"Seeded {i}", IsPrivate = false }
                );
            }

            // Seed the repo accesses for the many repos
            for (int i = 4; i <= 50; i++)
            {
                modelBuilder.Entity<RepoAccess>().HasData(
                    new RepoAccess { RepoId = i, UserId = 1, AccessLevel = AccessLevel.OWNER }
                );
            }



            // Seed Repo Access
            modelBuilder.Entity<RepoAccess>().HasData(
                new RepoAccess { RepoId = 1, UserId = 1, AccessLevel = AccessLevel.OWNER },
                new RepoAccess { RepoId = 1, UserId = 2, AccessLevel = AccessLevel.WRITE },
                new RepoAccess { RepoId = 2, UserId = 2, AccessLevel = AccessLevel.OWNER },
                new RepoAccess { RepoId = 3, UserId = 1, AccessLevel = AccessLevel.OWNER }
            );


            // Seed Branches
            modelBuilder.Entity<Branch>().HasData(
                new Branch { BranchId = 1, RepoId = 1, BranchName = "main", ParentBranch = null, SplitFromCommitHash = null, LatestCommitHash = "925cc242245c8df69d12021001277c54ec4b321c", CreatedBy = 1, CreatedAt = DateTimeOffset.UtcNow },
                new Branch { BranchId = 2, RepoId = 1, BranchName = "branch", ParentBranch = 1, SplitFromCommitHash = "925cc242245c8df69d12021001277c54ec4b321c", LatestCommitHash = "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", CreatedBy = 1, CreatedAt = DateTimeOffset.UtcNow },
                new Branch { BranchId = 3, RepoId = 3, BranchName = "main", ParentBranch = null, SplitFromCommitHash = null, LatestCommitHash = "branch3_commit_100", CreatedBy = 1, CreatedAt = DateTimeOffset.UtcNow }
            );




            // Seed commits for repo with many commits branch 3
            var baseDate = new DateTimeOffset(new DateTime(2025, 3, 1), TimeSpan.Zero);
            var random = new Random(12345);
            var currentCommitDate = baseDate;

            int initialCommitId = 3;
            string initialCommitHash = "branch3_initial_commit_hash";

            // Create an initial commit
            modelBuilder.Entity<Commit>().HasData(
                new Commit
                {
                    CommitId = initialCommitId,
                    CommitHash = initialCommitHash,
                    BranchId = 3,
                    TreeHash = "",
                    CreatedBy = "User1",
                    Message = "Initial commit for branch 3",
                    CommittedAt = currentCommitDate
                }
            );

            // Generate 100 additional commits
            int totalAdditionalCommits = 100;
            for (int i = 1; i <= totalAdditionalCommits; i++)
            {
                int commitId = initialCommitId + i;
                string currentCommitHash = $"branch3_commit_{i}";

                int rndSkipScale = random.Next(0, 100);

                if (rndSkipScale < 70)
                {
                    currentCommitDate = currentCommitDate.AddMinutes(random.Next(1, 120)); // 1-120 min

                }
                else
                {
                    currentCommitDate = currentCommitDate.AddDays(random.Next(1, 4)); // 1-3 days
                }

                // Create the commit
                modelBuilder.Entity<Commit>().HasData(
                    new Commit
                    {
                        CommitId = commitId,
                        CommitHash = currentCommitHash,
                        BranchId = 3,
                        TreeHash = "",
                        CreatedBy = "User1",
                        Message = $"Commit {i} for branch 3",
                        CommittedAt = currentCommitDate
                    }
                );

                // Create a commit parent
                modelBuilder.Entity<CommitParent>().HasData(
                    new CommitParent
                    {
                        ChildId = commitId,
                        ParentId = commitId - 1
                    }
                );
            }











            // Seed Commits
            modelBuilder.Entity<Commit>().HasData(
                new Commit
                {
                    CommitId = 1,
                    CommitHash = "925cc242245c8df69d12021001277c54ec4b321c",
                    BranchId = 1,
                    TreeHash = "",
                    CreatedBy = "Janus",
                    Message = "Initial commit",
                    CommittedAt = DateTimeOffset.UtcNow
                },
                new Commit
                {
                    CommitId = 2,
                    CommitHash = "18bd7fcf86b444b0270f93d333f7c5457e4abcbe",
                    BranchId = 2,
                    TreeHash = "517e4c52e1020d3bc9901cb81093943d4919b55c",
                    CreatedBy = "User2",
                    Message = "Next commit",
                    CommittedAt = DateTimeOffset.UtcNow
                }
            );


            // Seed Commit Parents (Commit 2 is a child of Commit 1)
            modelBuilder.Entity<CommitParent>().HasData(
                new CommitParent { ChildId = 2, ParentId = 1 }
            );




            base.OnModelCreating(modelBuilder);
        }




        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();

            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip non changed entries
                if (entry.Entity is AuditLog ||
                    entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Detached)
                    continue;

                var auditEntry = new AuditEntry(entry);

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.Action = "Added";

                        // Get the updated value
                        foreach (var property in entry.Properties)
                        {
                            auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        break;

                    case EntityState.Deleted:
                        auditEntry.Action = "Deleted";

                        // Get the original value
                        foreach (var property in entry.Properties)
                        {
                            auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                        }
                        break;

                    case EntityState.Modified:
                        auditEntry.Action = "Modified";

                        foreach (var property in entry.Properties)
                        {
                            if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                            {
                                auditEntry.ChangedColumns.Add(property.Metadata.Name);
                                auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                                auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                            }
                        }
                        break;
                }


                if (auditEntry.Action != "Modified" || auditEntry.ChangedColumns.Any())
                {
                    auditEntries.Add(auditEntry);
                }
            }

            // Add the audit log
            foreach (var audit in auditEntries)
            {
                AuditLogs.Add(audit.ToAuditLog());
            }

            // Save the changed and log the audit
            return await base.SaveChangesAsync(cancellationToken);
        }






    }

}


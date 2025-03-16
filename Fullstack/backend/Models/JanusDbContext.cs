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
                new Repository { RepoId = 2, OwnerId = 2, RepoName = "Repo2", RepoDescription = "Sec seeded", IsPrivate = true }
            );

            // Seed many repositories
            for (int i = 3; i <= 50; i++)
            {
                modelBuilder.Entity<Repository>().HasData(
                    new Repository { RepoId = i, OwnerId = 1, RepoName = $"Repo{i}", RepoDescription = $"Seeded {i}", IsPrivate = false }
                );
            }

            // Seed the repo accesses for the many repos
            for (int i = 3; i <= 50; i++)
            {
                modelBuilder.Entity<RepoAccess>().HasData(
                    new RepoAccess { RepoId = i, UserId = 1, AccessLevel = AccessLevel.OWNER }
                );
            }



            // Seed Repo Access
            modelBuilder.Entity<RepoAccess>().HasData(
                new RepoAccess { RepoId = 1, UserId = 1, AccessLevel = AccessLevel.OWNER },
                new RepoAccess { RepoId = 1, UserId = 2, AccessLevel = AccessLevel.WRITE },
                new RepoAccess { RepoId = 2, UserId = 2, AccessLevel = AccessLevel.OWNER }
            );


            // Seed Branches
            modelBuilder.Entity<Branch>().HasData(
                new Branch { BranchId = 1, RepoId = 1, BranchName = "main", ParentBranch = null, SplitFromCommitHash = null, LatestCommitHash = "925cc242245c8df69d12021001277c54ec4b321c", CreatedBy = 1, CreatedAt = DateTimeOffset.UtcNow },
                new Branch { BranchId = 2, RepoId = 1, BranchName = "branch", ParentBranch = 1, SplitFromCommitHash = "925cc242245c8df69d12021001277c54ec4b321c", LatestCommitHash = "18bd7fcf86b444b0270f93d333f7c5457e4abcbe", CreatedBy = 1, CreatedAt = DateTimeOffset.UtcNow }
            );


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


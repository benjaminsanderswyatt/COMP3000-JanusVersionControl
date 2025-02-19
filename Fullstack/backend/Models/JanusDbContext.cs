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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Case insensitve
            modelBuilder.UseCollation("utf8mb4_unicode_ci");


            // Unique constraints
            modelBuilder.Entity<Repository>()
                .HasIndex(r => new { r.OwnerId, r.RepoName })
                .IsUnique();

            modelBuilder.Entity<Branch>()
                .HasIndex(b => new { b.RepoId, b.BranchName })
                .IsUnique();



            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();




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
                .OnDelete(DeleteBehavior.Restrict);



            // Indexes
            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.CommitHash);

            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.CommittedAt);







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


            // Seed Repo Access
            modelBuilder.Entity<RepoAccess>().HasData(
                new RepoAccess { RepoId = 1, UserId = 1, AccessLevel = AccessLevel.OWNER },
                new RepoAccess { RepoId = 1, UserId = 2, AccessLevel = AccessLevel.WRITE },
                new RepoAccess { RepoId = 2, UserId = 2, AccessLevel = AccessLevel.OWNER }
            );


            // Seed Branches
            modelBuilder.Entity<Branch>().HasData(
                new Branch { BranchId = 1, RepoId = 1, BranchName = "main", ParentBranch = null, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Branch { BranchId = 2, RepoId = 1, BranchName = "branch", ParentBranch = 1, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            );


            // Seed Commits
            modelBuilder.Entity<Commit>().HasData(
                new Commit
                {
                    CommitId = 1,
                    CommitHash = "b62a22ce626f3648da1c6d5ea620cf683fe2e0ef",
                    BranchId = 1,
                    TreeHash = "",
                    AuthorName = "janus",
                    AuthorEmail = "janus",
                    Message = "Initial commit",
                    CommittedAt = DateTime.UtcNow
                },
                new Commit
                {
                    CommitId = 2,
                    CommitHash = "a86144601800e35b72f4427206042b4ec0da4288",
                    BranchId = 2,
                    TreeHash = "b2ee222a7d29faa635c1e98886bed90b8510c969",
                    AuthorName = "User2",
                    AuthorEmail = "user@2.com",
                    Message = "Next commit",
                    CommittedAt = DateTime.UtcNow
                }
            );


            // Seed Commit Parents (Commit 2 is a child of Commit 1)
            modelBuilder.Entity<CommitParent>().HasData(
                new CommitParent { ChildId = 2, ParentId = 1 }
            );




            base.OnModelCreating(modelBuilder);

        }
    }

}

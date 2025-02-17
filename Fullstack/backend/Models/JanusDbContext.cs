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


            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "User1", Email = "user@1.com", PasswordHash = "password", Salt = new byte[16] },
                new User { UserId = 2, Username = "User2", Email = "user@2.com", PasswordHash = "password", Salt = new byte[16] }
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
                new Branch { BranchId = 1, RepoId = 1, BranchName = "main", CreatedAt = DateTime.UtcNow },
                new Branch { BranchId = 2, RepoId = 1, BranchName = "branch", CreatedAt = DateTime.UtcNow }
            );


            // Seed Commits
            modelBuilder.Entity<Commit>().HasData(
                new Commit
                {
                    CommitId = 1,
                    CommitHash = "f7b1c205158daf2ee72d31cc1838455368c15cb3",
                    BranchName = "main",
                    TreeHash = "",
                    AuthorName = "janus",
                    AuthorEmail = "janus",
                    Message = "Initial commit",
                    CommittedAt = DateTime.UtcNow
                },
                new Commit
                {
                    CommitId = 2,
                    CommitHash = "915b84e9f8ce43018350092a25c4f65e6e290165",
                    BranchName = "branch",
                    TreeHash = "c65dca236a008513a28342c778c7c34a0b9b50f0",
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

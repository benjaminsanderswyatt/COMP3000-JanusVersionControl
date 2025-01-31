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
        public DbSet<Tree> Trees { get; set; }
        public DbSet<File> Files { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .HasKey(cp => new { cp.ChildHash, cp.ParentHash });

            //modelBuilder.Entity<Tree>()
            //    .HasKey(t => new { t.TreeHash, t.EntryName });

            modelBuilder.Entity<RepoAccess>()
                .HasKey(ra => new { ra.RepoId, ra.UserId });



            // Delete cascade
            modelBuilder.Entity<CommitParent>()
                .HasOne(cp => cp.Child)
                .WithMany(c => c.Parents)
                .HasForeignKey(cp => cp.ChildHash)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepoAccess>()
                .HasOne(ra => ra.Repository)
                .WithMany(r => r.RepoAccesses)
                .HasForeignKey(ra => ra.RepoId)
                .OnDelete(DeleteBehavior.Restrict);



            // Indexes
            modelBuilder.Entity<Commit>()
                .HasIndex(c => c.CommittedAt);

            modelBuilder.Entity<File>()
                .HasIndex(f => f.FileHash);


            Tree.ConfigureEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);

        }
    }

}

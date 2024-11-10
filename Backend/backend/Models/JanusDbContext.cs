using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public class JanusDbContext : DbContext
    {
        public JanusDbContext(DbContextOptions<JanusDbContext> options) : base(options) { }

        public DbSet<User> Users {  get; set; }
        public DbSet<Repository> Repositories {  get; set; }
        public DbSet<Branch> Branches {  get; set; }
        public DbSet<Commit> Commits {  get; set; }
        public DbSet<File> Files {  get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User to Repository (One-to-Many)
            modelBuilder.Entity<Repository>()
                .HasOne(r => r.User)
                .WithMany(u => u.Repositories)
                .HasForeignKey(r => r.UserId);

            // Repository to Commit (One-to-Many)
            modelBuilder.Entity<Commit>()
                .HasOne(c => c.Repository)
                .WithMany(r => r.Commits)
                .HasForeignKey(c => c.RepoId);

            // Commit to File (One-to-Many)
            modelBuilder.Entity<File>()
                .HasOne(f => f.Commit)
                .WithMany(c => c.Files)
                .HasForeignKey(f => f.CommitId);

            // Branch to Commit (Many-to-One)
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Commit)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.CommitId);

            // Commit to User (Many-to-One)
            modelBuilder.Entity<Commit>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Commits)
                .HasForeignKey(c => c.AuthorId);

            // Commit to Parent Commit (One-to-One (self-reference for parent commit))
            modelBuilder.Entity<Commit>()
                .HasOne(c => c.ParentCommit)
                .WithOne()
                .HasForeignKey<Commit>(c => c.ParentCommitId);

            base.OnModelCreating(modelBuilder);
        }
    }

}

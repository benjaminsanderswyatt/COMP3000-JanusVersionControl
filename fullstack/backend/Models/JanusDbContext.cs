using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public class JanusDbContext : DbContext
    {
        public JanusDbContext(DbContextOptions<JanusDbContext> options) : base(options) { }

        public DbSet<User> Users {  get; set; }
        public DbSet<AccessToken> AccessTokens {  get; set; }
        public DbSet<Repository> Repositories {  get; set; }
        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<Branch> Branches {  get; set; }
        public DbSet<Commit> Commits {  get; set; }
        public DbSet<File> Files {  get; set; }
        public DbSet<FileContent> FileContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manual set foreign keys as Collaborator has compound primary keys
            modelBuilder.Entity<Collaborator>()
                .HasKey(c => new { c.RepoId, c.UserId });

            modelBuilder.Entity<Collaborator>()
                .HasOne(c => c.User)
                .WithMany(u => u.Collaborators)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Collaborator>()
                .HasOne(c => c.Repository)
                .WithMany(r => r.Collaborators)
                .HasForeignKey(c => c.RepoId);

            base.OnModelCreating(modelBuilder);

        }
    }

}

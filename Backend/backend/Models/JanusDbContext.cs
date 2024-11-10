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

            // Users to Repo  (One to Many)
            modelBuilder.Entity<Repository>()
                .HasOne(r => r.User)
                .WithMany(u => u.Repositories)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete User delete their Repos

            // Repo to Commit (One to Many)
            modelBuilder.Entity<Commit>()
                .HasOne(c => c.Repository)
                .WithMany(r => r.Commits)
                .HasForeignKey(c => c.RepoId)
                .OnDelete(DeleteBehavior.Cascade); // Delete repo delete related commits

            // Commit to User (Many to One)
            modelBuilder.Entity<Commit>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Commits)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);  // When Commit author is deleted its commits should not be deleted

            // Branch to Repo (Many to One)
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Repository)
                .WithMany(r => r.Branches)
                .HasForeignKey(b => b.RepoId)
                .OnDelete(DeleteBehavior.Cascade); // Delete branches if repo is deleted

            // Branches to Commits (One to Many)
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Commit) 
                .WithMany()
                .HasForeignKey(b => b.CommitId)
                .OnDelete(DeleteBehavior.SetNull); // When Commit is deleted the Branch isnt deleted

            // Files to Commits (Many to One)
            modelBuilder.Entity<File>()
                .HasOne(f => f.Commit)
                .WithMany(c => c.Files)
                .HasForeignKey(f => f.CommitId)
                .OnDelete(DeleteBehavior.Cascade);  // Delete files when commit is deleted
        

            base.OnModelCreating(modelBuilder);

        }
    }

}
